#include "../include/Overlay.h"
#include <dwmapi.h>
#include <d3dcompiler.h>
#include <chrono>

#pragma comment(lib, "dwmapi.lib")
#pragma comment(lib, "d3dcompiler.lib")

// Global instance
static std::unique_ptr<PinPoint::Overlay> g_overlay = nullptr;

// Simple vertex structure
struct Vertex {
    DirectX::XMFLOAT3 Position;
    DirectX::XMFLOAT4 Color;
};

// Constant buffer structure
struct ConstantBuffer {
    DirectX::XMFLOAT4X4 Transform;
    DirectX::XMFLOAT4 Color;
};

// Shader code (very basic shaders for crosshair rendering)
const char* g_vertexShaderCode = R"(
cbuffer ConstantBuffer : register(b0)
{
    matrix Transform;
    float4 Color;
};

struct VS_INPUT
{
    float3 Position : POSITION;
    float4 Color : COLOR;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output;
    output.Position = mul(float4(input.Position, 1.0f), Transform);
    output.Color = input.Color * Color;
    return output;
}
)";

const char* g_pixelShaderCode = R"(
struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

float4 main(PS_INPUT input) : SV_TARGET
{
    return input.Color;
}
)";

namespace PinPoint {

    Overlay::Overlay() {
    }

    Overlay::~Overlay() {
        Shutdown();
    }

    bool Overlay::Initialize() {
        Utilities::Log("Initializing overlay...", Utilities::LogLevel::Info);
        
        if (!CreateOverlayWindow()) {
            Utilities::Log("Failed to create overlay window", Utilities::LogLevel::Error);
            return false;
        }
        
        if (!InitializeDirectX()) {
            Utilities::Log("Failed to initialize DirectX", Utilities::LogLevel::Error);
            return false;
        }
        
        if (!CreateRenderTarget()) {
            Utilities::Log("Failed to create render target", Utilities::LogLevel::Error);
            return false;
        }
        
        if (!CreateShaders()) {
            Utilities::Log("Failed to create shaders", Utilities::LogLevel::Error);
            return false;
        }
        
        if (!CreateGeometry()) {
            Utilities::Log("Failed to create geometry", Utilities::LogLevel::Error);
            return false;
        }
        
        // Start the render thread
        StartRenderThread();
        
        m_initialized = true;
        Utilities::Log("Overlay initialized successfully", Utilities::LogLevel::Info);
        return true;
    }

    void Overlay::Shutdown() {
        if (!m_initialized) return;
        
        Utilities::Log("Shutting down overlay...", Utilities::LogLevel::Info);
        
        // Stop the render thread
        StopRenderThread();
        
        CleanupDirectX();
        
        if (m_hwnd) {
            DestroyWindow(m_hwnd);
            m_hwnd = NULL;
        }
        
        m_initialized = false;
        Utilities::Log("Overlay shutdown complete", Utilities::LogLevel::Info);
    }

    bool Overlay::CreateOverlayWindow() {
        Utilities::Log("Creating overlay window...", Utilities::LogLevel::Info);
        
        // Register window class
        WNDCLASSEX wc = {};
        wc.cbSize = sizeof(WNDCLASSEX);
        wc.style = CS_HREDRAW | CS_VREDRAW;
        wc.lpfnWndProc = WindowProc;
        wc.hInstance = GetModuleHandle(NULL);
        wc.hCursor = LoadCursor(NULL, IDC_ARROW);
        wc.lpszClassName = L"PinPointOverlayClass";
        
        if (!RegisterClassEx(&wc)) {
            Utilities::Log("Failed to register window class: " + Utilities::GetLastErrorAsString(), Utilities::LogLevel::Error);
            return false;
        }
        
        // Get screen dimensions
        m_width = GetSystemMetrics(SM_CXSCREEN);
        m_height = GetSystemMetrics(SM_CYSCREEN);
        
        // Create window
        m_hwnd = CreateWindowEx(
            WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_LAYERED,  // Extended window style
            L"PinPointOverlayClass",                           // Window class name
            L"PinPoint Overlay",                               // Window title
            WS_POPUP,                                          // Window style
            0, 0,                                              // Position
            m_width, m_height,                                 // Size
            NULL,                                              // Parent window
            NULL,                                              // Menu
            GetModuleHandle(NULL),                             // Instance handle
            this                                               // Additional data
        );
        
        if (!m_hwnd) {
            Utilities::Log("Failed to create window: " + Utilities::GetLastErrorAsString(), Utilities::LogLevel::Error);
            return false;
        }
        
        // Make the window transparent
        SetLayeredWindowAttributes(m_hwnd, RGB(0, 0, 0), 0, LWA_COLORKEY);
        
        // Create a borderless, click-through window
        MARGINS margins = {-1};
        DwmExtendFrameIntoClientArea(m_hwnd, &margins);
        
        // Set window to pass-through for mouse events
        LONG exStyle = GetWindowLong(m_hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
        SetWindowLong(m_hwnd, GWL_EXSTYLE, exStyle);
        
        Utilities::Log("Overlay window created successfully", Utilities::LogLevel::Info);
        return true;
    }

    bool Overlay::InitializeDirectX() {
        Utilities::Log("Initializing DirectX...", Utilities::LogLevel::Info);
        
        // Create device and swap chain
        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
        swapChainDesc.Width = m_width;
        swapChainDesc.Height = m_height;
        swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        swapChainDesc.SampleDesc.Count = 1;
        swapChainDesc.SampleDesc.Quality = 0;
        swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.BufferCount = 2;
        swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
        swapChainDesc.AlphaMode = DXGI_ALPHA_MODE_PREMULTIPLIED;
        
        D3D_FEATURE_LEVEL featureLevels[] = { D3D_FEATURE_LEVEL_11_0 };
        UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
        
#ifdef _DEBUG
        creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
        
        // Create device
        ID3D11Device* device = nullptr;
        ID3D11DeviceContext* context = nullptr;
        
        HRESULT hr = D3D11CreateDevice(
            nullptr,
            D3D_DRIVER_TYPE_HARDWARE,
            nullptr,
            creationFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,
            &device,
            nullptr,
            &context
        );
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create D3D11 device: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        m_device = device;
        m_deviceContext = context;
        
        // Get DXGI factory
        IDXGIDevice1* dxgiDevice = nullptr;
        hr = m_device->QueryInterface(__uuidof(IDXGIDevice1), (void**)&dxgiDevice);
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to get DXGI device: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        IDXGIAdapter* dxgiAdapter = nullptr;
        hr = dxgiDevice->GetAdapter(&dxgiAdapter);
        dxgiDevice->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to get DXGI adapter: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        IDXGIFactory2* dxgiFactory = nullptr;
        hr = dxgiAdapter->GetParent(__uuidof(IDXGIFactory2), (void**)&dxgiFactory);
        dxgiAdapter->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to get DXGI factory: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        // Create swap chain
        hr = dxgiFactory->CreateSwapChainForHwnd(
            m_device,
            m_hwnd,
            &swapChainDesc,
            nullptr,
            nullptr,
            &m_swapChain
        );
        
        dxgiFactory->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create swap chain: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        Utilities::Log("DirectX initialized successfully", Utilities::LogLevel::Info);
        return true;
    }

    bool Overlay::CreateRenderTarget() {
        if (!m_swapChain || !m_device) return false;
        
        // Get back buffer
        ID3D11Texture2D* backBuffer = nullptr;
        HRESULT hr = m_swapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&backBuffer);
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to get back buffer: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        // Create render target view
        hr = m_device->CreateRenderTargetView(backBuffer, nullptr, &m_renderTargetView);
        backBuffer->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create render target view: " + std::to_string(hr), Utilities::LogLevel::Error);
            return false;
        }
        
        return true;
    }

    bool Overlay::CreateShaders() {
        if (!m_device || !m_deviceContext) return false;
        
        // Compile vertex shader
        ID3DBlob* vsBlob = nullptr;
        ID3DBlob* errorBlob = nullptr;
        
        HRESULT hr = D3DCompile(
            g_vertexShaderCode,
            strlen(g_vertexShaderCode),
            nullptr,
            nullptr,
            nullptr,
            "main",
            "vs_5_0",
            D3DCOMPILE_ENABLE_STRICTNESS,
            0,
            &vsBlob,
            &errorBlob
        );
        
        if (FAILED(hr)) {
            std::string errorMsg = "Failed to compile vertex shader: ";
            if (errorBlob) {
                errorMsg += std::string((char*)errorBlob->GetBufferPointer(), errorBlob->GetBufferSize());
                errorBlob->Release();
            }
            Utilities::Log(errorMsg, Utilities::LogLevel::Error);
            return false;
        }
        
        // Create vertex shader
        hr = m_device->CreateVertexShader(
            vsBlob->GetBufferPointer(),
            vsBlob->GetBufferSize(),
            nullptr,
            &m_vertexShader
        );
        
        if (FAILED(hr)) {
            vsBlob->Release();
            Utilities::Log("Failed to create vertex shader", Utilities::LogLevel::Error);
            return false;
        }
        
        // Define input layout
        D3D11_INPUT_ELEMENT_DESC layout[] = {
            { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
            { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 }
        };
        
        // Create input layout
        hr = m_device->CreateInputLayout(
            layout,
            ARRAYSIZE(layout),
            vsBlob->GetBufferPointer(),
            vsBlob->GetBufferSize(),
            &m_inputLayout
        );
        
        vsBlob->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create input layout", Utilities::LogLevel::Error);
            return false;
        }
        
        // Compile pixel shader
        ID3DBlob* psBlob = nullptr;
        
        hr = D3DCompile(
            g_pixelShaderCode,
            strlen(g_pixelShaderCode),
            nullptr,
            nullptr,
            nullptr,
            "main",
            "ps_5_0",
            D3DCOMPILE_ENABLE_STRICTNESS,
            0,
            &psBlob,
            &errorBlob
        );
        
        if (FAILED(hr)) {
            std::string errorMsg = "Failed to compile pixel shader: ";
            if (errorBlob) {
                errorMsg += std::string((char*)errorBlob->GetBufferPointer(), errorBlob->GetBufferSize());
                errorBlob->Release();
            }
            Utilities::Log(errorMsg, Utilities::LogLevel::Error);
            return false;
        }
        
        // Create pixel shader
        hr = m_device->CreatePixelShader(
            psBlob->GetBufferPointer(),
            psBlob->GetBufferSize(),
            nullptr,
            &m_pixelShader
        );
        
        psBlob->Release();
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create pixel shader", Utilities::LogLevel::Error);
            return false;
        }
        
        // Create constant buffer
        D3D11_BUFFER_DESC cbDesc = {};
        cbDesc.ByteWidth = sizeof(ConstantBuffer);
        cbDesc.Usage = D3D11_USAGE_DYNAMIC;
        cbDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
        cbDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        
        hr = m_device->CreateBuffer(&cbDesc, nullptr, &m_constantBuffer);
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create constant buffer", Utilities::LogLevel::Error);
            return false;
        }
        
        return true;
    }

    bool Overlay::CreateGeometry() {
        if (!m_device) return false;
        
        // Create a simple crosshair geometry
        // We'll update this based on the crosshair style later
        Vertex vertices[] = {
            // Horizontal line
            { { -1.0f, 0.0f, 0.0f }, { 1.0f, 1.0f, 1.0f, 1.0f } },
            { { 1.0f, 0.0f, 0.0f }, { 1.0f, 1.0f, 1.0f, 1.0f } },
            
            // Vertical line
            { { 0.0f, -1.0f, 0.0f }, { 1.0f, 1.0f, 1.0f, 1.0f } },
            { { 0.0f, 1.0f, 0.0f }, { 1.0f, 1.0f, 1.0f, 1.0f } }
        };
        
        D3D11_BUFFER_DESC bufferDesc = {};
        bufferDesc.ByteWidth = sizeof(vertices);
        bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
        bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
        bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        
        D3D11_SUBRESOURCE_DATA initData = {};
        initData.pSysMem = vertices;
        
        HRESULT hr = m_device->CreateBuffer(&bufferDesc, &initData, &m_vertexBuffer);
        
        if (FAILED(hr)) {
            Utilities::Log("Failed to create vertex buffer", Utilities::LogLevel::Error);
            return false;
        }
        
        return true;
    }

    void Overlay::Show(bool show) {
        m_visible = show;
        ShowWindow(m_hwnd, show ? SW_SHOW : SW_HIDE);
    }

    void Overlay::Render() {
        if (!m_initialized || !m_visible) return;
        
        // Clear the render target
        float clearColor[4] = { 0.0f, 0.0f, 0.0f, 0.0f };
        m_deviceContext->ClearRenderTargetView(m_renderTargetView, clearColor);
        
        // Set the render target
        m_deviceContext->OMSetRenderTargets(1, &m_renderTargetView, nullptr);
        
        // Set viewport
        D3D11_VIEWPORT viewport = {};
        viewport.Width = static_cast<float>(m_width);
        viewport.Height = static_cast<float>(m_height);
        viewport.MinDepth = 0.0f;
        viewport.MaxDepth = 1.0f;
        m_deviceContext->RSSetViewports(1, &viewport);
        
        // Render the crosshair
        RenderCrosshair();
        
        // Present the frame
        m_swapChain->Present(1, 0);
        
        // Make sure window stays on top
        SetWindowPos(m_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    void Overlay::RenderCrosshair() {
        // Update transformation matrix
        ConstantBuffer cb;
        DirectX::XMMATRIX transform = DirectX::XMMatrixIdentity();
        
        // Scale based on crosshair size
        float scaleX = m_crosshairSettings.size / static_cast<float>(m_width);
        float scaleY = m_crosshairSettings.size / static_cast<float>(m_height);
        transform = DirectX::XMMatrixMultiply(transform, DirectX::XMMatrixScaling(scaleX, scaleY, 1.0f));
        
        // Center on screen
        transform = DirectX::XMMatrixMultiply(transform, DirectX::XMMatrixTranslation(0.0f, 0.0f, 0.0f));
        
        // Convert to screen space coordinates
        DirectX::XMMATRIX screenTransform = DirectX::XMMatrixIdentity();
        screenTransform = DirectX::XMMatrixMultiply(screenTransform, DirectX::XMMatrixScaling(2.0f / m_width, 2.0f / m_height, 1.0f));
        screenTransform = DirectX::XMMatrixMultiply(screenTransform, DirectX::XMMatrixTranslation(-1.0f, -1.0f, 0.0f));
        
        // Combine transformations
        transform = DirectX::XMMatrixMultiply(transform, screenTransform);
        
        // Store in constant buffer
        DirectX::XMStoreFloat4x4(&cb.Transform, DirectX::XMMatrixTranspose(transform));
        cb.Color = m_crosshairSettings.color;
        
        // Update constant buffer
        D3D11_MAPPED_SUBRESOURCE mappedResource;
        m_deviceContext->Map(m_constantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
        memcpy(mappedResource.pData, &cb, sizeof(ConstantBuffer));
        m_deviceContext->Unmap(m_constantBuffer, 0);
        
        // Set shaders and resources
        m_deviceContext->VSSetShader(m_vertexShader, nullptr, 0);
        m_deviceContext->PSSetShader(m_pixelShader, nullptr, 0);
        m_deviceContext->VSSetConstantBuffers(0, 1, &m_constantBuffer);
        m_deviceContext->PSSetConstantBuffers(0, 1, &m_constantBuffer);
        m_deviceContext->IASetInputLayout(m_inputLayout);
        
        // Set vertex buffer
        UINT stride = sizeof(Vertex);
        UINT offset = 0;
        m_deviceContext->IASetVertexBuffers(0, 1, &m_vertexBuffer, &stride, &offset);
        m_deviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
        
        // Draw the crosshair
        m_deviceContext->Draw(4, 0);
    }

    void Overlay::UpdateCrosshair(const CrosshairSettings& settings) {
        std::lock_guard<std::mutex> lock(m_settingsMutex);
        
        m_crosshairSettings = settings;
        
        // Update vertex buffer with new crosshair geometry if style changed
        // This is a simple implementation - more complex styles would need more vertices
        if (m_vertexBuffer) {
            Vertex vertices[4];
            
            switch (m_crosshairSettings.style) {
                case 0: // Cross
                    // Horizontal line
                    vertices[0] = { { -1.0f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    vertices[1] = { { 1.0f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    
                    // Vertical line
                    vertices[2] = { { 0.0f, -1.0f, 0.0f }, m_crosshairSettings.color };
                    vertices[3] = { { 0.0f, 1.0f, 0.0f }, m_crosshairSettings.color };
                    break;
                    
                case 1: // Dot (represented as a small cross)
                    // Smaller horizontal line
                    vertices[0] = { { -0.2f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    vertices[1] = { { 0.2f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    
                    // Smaller vertical line
                    vertices[2] = { { 0.0f, -0.2f, 0.0f }, m_crosshairSettings.color };
                    vertices[3] = { { 0.0f, 0.2f, 0.0f }, m_crosshairSettings.color };
                    break;
                    
                default: // Default to cross
                    // Horizontal line
                    vertices[0] = { { -1.0f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    vertices[1] = { { 1.0f, 0.0f, 0.0f }, m_crosshairSettings.color };
                    
                    // Vertical line
                    vertices[2] = { { 0.0f, -1.0f, 0.0f }, m_crosshairSettings.color };
                    vertices[3] = { { 0.0f, 1.0f, 0.0f }, m_crosshairSettings.color };
                    break;
            }
            
            // Update vertex buffer
            D3D11_MAPPED_SUBRESOURCE mappedResource;
            m_deviceContext->Map(m_vertexBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
            memcpy(mappedResource.pData, vertices, sizeof(vertices));
            m_deviceContext->Unmap(m_vertexBuffer, 0);
        }
    }

    void Overlay::CleanupDirectX() {
        if (m_renderTargetView) {
            m_renderTargetView->Release();
            m_renderTargetView = nullptr;
        }
        
        if (m_swapChain) {
            m_swapChain->Release();
            m_swapChain = nullptr;
        }
        
        if (m_vertexBuffer) {
            m_vertexBuffer->Release();
            m_vertexBuffer = nullptr;
        }
        
        if (m_constantBuffer) {
            m_constantBuffer->Release();
            m_constantBuffer = nullptr;
        }
        
        if (m_inputLayout) {
            m_inputLayout->Release();
            m_inputLayout = nullptr;
        }
        
        if (m_vertexShader) {
            m_vertexShader->Release();
            m_vertexShader = nullptr;
        }
        
        if (m_pixelShader) {
            m_pixelShader->Release();
            m_pixelShader = nullptr;
        }
        
        if (m_deviceContext) {
            m_deviceContext->Release();
            m_deviceContext = nullptr;
        }
        
        if (m_device) {
            m_device->Release();
            m_device = nullptr;
        }
    }

    void Overlay::ResizeBuffers() {
        if (!m_initialized) return;
        
        // Release old views
        if (m_renderTargetView) {
            m_renderTargetView->Release();
            m_renderTargetView = nullptr;
        }
        
        // Update width and height
        m_width = GetSystemMetrics(SM_CXSCREEN);
        m_height = GetSystemMetrics(SM_CYSCREEN);
        
        // Resize window
        SetWindowPos(m_hwnd, HWND_TOPMOST, 0, 0, m_width, m_height, SWP_SHOWWINDOW);
        
        // Resize swap chain
        HRESULT hr = m_swapChain->ResizeBuffers(0, m_width, m_height, DXGI_FORMAT_UNKNOWN, 0);
        
        if (SUCCEEDED(hr)) {
            // Recreate render target view
            CreateRenderTarget();
        }
    }

    LRESULT CALLBACK Overlay::WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
        // Get the Overlay instance from window data
        Overlay* overlay = reinterpret_cast<Overlay*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
        
        switch (uMsg) {
            case WM_CREATE:
                {
                    // Store the Overlay instance in window data
                    CREATESTRUCT* createStruct = reinterpret_cast<CREATESTRUCT*>(lParam);
                    SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(createStruct->lpCreateParams));
                }
                return 0;
                
            case WM_DESTROY:
                return 0;
                
            case WM_SIZE:
                if (overlay) {
                    overlay->ResizeBuffers();
                }
                return 0;
        }
        
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    void Overlay::StartRenderThread() {
        if (m_threadRunning) return;
        
        m_threadRunning = true;
        m_renderThread = std::thread(&Overlay::RenderThreadFunction, this);
        
        Utilities::Log("Render thread started", Utilities::LogLevel::Info);
    }
    
    void Overlay::StopRenderThread() {
        if (!m_threadRunning) return;
        
        m_threadRunning = false;
        
        if (m_renderThread.joinable()) {
            m_renderThread.join();
        }
        
        Utilities::Log("Render thread stopped", Utilities::LogLevel::Info);
    }
    
    void Overlay::RenderThreadFunction() {
        // Set thread priority to high to ensure smooth rendering
        SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_ABOVE_NORMAL);
        
        MSG msg = {};
        
        // Message and render loop
        while (m_threadRunning) {
            // Process any window messages
            while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
                
                if (msg.message == WM_QUIT) {
                    m_threadRunning = false;
                    break;
                }
            }
            
            // Render a frame
            if (m_initialized && m_visible) {
                Render();
            }
            
            // Sleep to limit frame rate and reduce CPU usage
            std::this_thread::sleep_for(std::chrono::milliseconds(16)); // ~60 FPS
        }
    }
}

// Export functions implementation
extern "C" {
    
    bool InitializeOverlay() {
        if (!g_overlay) {
            g_overlay = std::make_unique<PinPoint::Overlay>();
        }
        
        return g_overlay->Initialize();
    }
    
    void ShutdownOverlay() {
        if (g_overlay) {
            g_overlay->Shutdown();
            g_overlay.reset();
        }
    }
    
    void ShowOverlay(bool show) {
        if (g_overlay) {
            g_overlay->Show(show);
        }
    }
    
    void SetCrosshairColor(float r, float g, float b, float a) {
        if (g_overlay && g_overlay->IsInitialized()) {
            PinPoint::CrosshairSettings settings;
            settings.color = { r, g, b, a };
            g_overlay->UpdateCrosshair(settings);
        }
    }
    
    void SetCrosshairSize(float size) {
        if (g_overlay && g_overlay->IsInitialized()) {
            PinPoint::CrosshairSettings settings;
            settings.size = size;
            g_overlay->UpdateCrosshair(settings);
        }
    }
    
    void SetCrosshairThickness(float thickness) {
        if (g_overlay && g_overlay->IsInitialized()) {
            PinPoint::CrosshairSettings settings;
            settings.thickness = thickness;
            g_overlay->UpdateCrosshair(settings);
        }
    }
    
    void SetCrosshairStyle(int style) {
        if (g_overlay && g_overlay->IsInitialized()) {
            PinPoint::CrosshairSettings settings;
            settings.style = style;
            g_overlay->UpdateCrosshair(settings);
        }
    }
}
