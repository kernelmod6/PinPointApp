#pragma once

#include <Windows.h>
#include <d3d11.h>
#include <dxgi1_2.h>
#include <DirectXMath.h>
#include <string>
#include <memory>
#include <vector>
#include <thread>
#include <atomic>
#include <mutex>
#include "Utilities.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

namespace PinPoint {

    // Structure to hold crosshair settings
    struct CrosshairSettings {
        float size = 20.0f;
        DirectX::XMFLOAT4 color = { 1.0f, 1.0f, 1.0f, 1.0f }; // RGBA
        float thickness = 2.0f;
        int style = 0; // 0: Cross, 1: Dot, 2: Circle, etc.
    };

    class Overlay {
    public:
        Overlay();
        ~Overlay();

        // Initialize the overlay
        bool Initialize();

        // Shutdown and cleanup
        void Shutdown();

        // Show or hide the overlay
        void Show(bool show);

        // Render frame
        void Render();

        // Update crosshair settings
        void UpdateCrosshair(const CrosshairSettings& settings);

        // Check if overlay is initialized
        bool IsInitialized() const { return m_initialized; }

    private:
        // Window creation and management
        bool CreateOverlayWindow();
        bool InitializeDirectX();
        void CleanupDirectX();
        void ResizeBuffers();

        // Rendering
        bool CreateRenderTarget();
        bool CreateShaders();
        bool CreateGeometry();
        void RenderCrosshair();

        // Render thread
        void RenderThreadFunction();
        void StartRenderThread();
        void StopRenderThread();

        // Window procedure
        static LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

        // Window handle
        HWND m_hwnd = NULL;
        
        // DirectX variables
        ID3D11Device* m_device = nullptr;
        ID3D11DeviceContext* m_deviceContext = nullptr;
        IDXGISwapChain1* m_swapChain = nullptr;
        ID3D11RenderTargetView* m_renderTargetView = nullptr;
        
        // Shader variables
        ID3D11VertexShader* m_vertexShader = nullptr;
        ID3D11PixelShader* m_pixelShader = nullptr;
        ID3D11InputLayout* m_inputLayout = nullptr;
        ID3D11Buffer* m_vertexBuffer = nullptr;
        ID3D11Buffer* m_constantBuffer = nullptr;

        // Crosshair settings
        CrosshairSettings m_crosshairSettings;
        std::mutex m_settingsMutex;

        // Thread variables
        std::thread m_renderThread;
        std::atomic<bool> m_threadRunning{false};

        // State variables
        bool m_initialized = false;
        bool m_visible = true;
        int m_width = 0;
        int m_height = 0;
    };

}

// Exported C-style functions for interop with C#
extern "C" {
    __declspec(dllexport) bool InitializeOverlay();
    __declspec(dllexport) void ShutdownOverlay();
    __declspec(dllexport) void ShowOverlay(bool show);
    __declspec(dllexport) void SetCrosshairColor(float r, float g, float b, float a);
    __declspec(dllexport) void SetCrosshairSize(float size);
    __declspec(dllexport) void SetCrosshairThickness(float thickness);
    __declspec(dllexport) void SetCrosshairStyle(int style);
}
