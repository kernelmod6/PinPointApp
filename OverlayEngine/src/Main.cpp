#include "../include/Overlay.h"
#include <iostream>
#include <thread>
#include <chrono>
#include <Windows.h>

// This is just for testing the overlay as a standalone application
// In the actual application, this code won't be needed since we'll use the DLL exports
int main() {
    // Initialize the overlay
    PinPoint::Overlay overlay;
    
    if (!overlay.Initialize()) {
        std::cout << "Failed to initialize overlay" << std::endl;
        return 1;
    }
    
    // Show the overlay
    overlay.Show(true);
    
    // Create a crosshair with green color
    PinPoint::CrosshairSettings settings;
    settings.color = { 0.0f, 1.0f, 0.0f, 1.0f }; // Green
    settings.size = 20.0f;
    settings.thickness = 2.0f;
    settings.style = 0; // Cross
    
    overlay.UpdateCrosshair(settings);
    
    // Main loop - run for 30 seconds
    std::cout << "Overlay running for 30 seconds..." << std::endl;
    for (int i = 0; i < 30; i++) {
        // Render the overlay
        overlay.Render();
        
        // Sleep for a bit
        std::this_thread::sleep_for(std::chrono::seconds(1));
        
        // Change color every 5 seconds
        if (i % 5 == 0) {
            settings.color = { 
                static_cast<float>(rand()) / RAND_MAX, 
                static_cast<float>(rand()) / RAND_MAX, 
                static_cast<float>(rand()) / RAND_MAX, 
                1.0f 
            };
            overlay.UpdateCrosshair(settings);
        }
    }
    
    // Shutdown the overlay
    overlay.Shutdown();
    
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        // Initialize once for each new process
        // Return FALSE to fail DLL load
        OutputDebugStringA("OverlayEngine.dll loaded successfully");
        break;
        
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
} 