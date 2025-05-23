
// This is just for testing the overlay as a standalone application
// In the actual application, this code won't be needed since we'll use the DLL exports
int main() {
    // Initialize the overlay
    PinPoint::Overlay overlay;
    
    if (!overlay.Initialize()) {
        std::cout << "Failed to initialize overlay" << std::endl;
        return 1;
    }
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