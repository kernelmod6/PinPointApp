#pragma once

#include <string>
#include <iostream>
#include <fstream>
#include <Windows.h>

namespace PinPoint {
    namespace Utilities {
        
        // Log levels
        enum class LogLevel {
            Debug,
            Info,
            Warning,
            Error
        };
        
        // Log a message with a specific log level
        void Log(const std::string& message, LogLevel level = LogLevel::Info);
        
        // Get the directory where the application is running
        std::string GetExecutablePath();
        
        // Create a console window for debug output
        void CreateConsoleWindow();
        
        // Close the console window
        void CloseConsoleWindow();
        
        // Window utilities
        HWND FindWindowByProcessId(DWORD processId);
        bool IsWindowFullscreen(HWND hwnd);
        
        // String utilities
        std::wstring StringToWideString(const std::string& str);
        std::string WideStringToString(const std::wstring& wstr);
        
        // Error handling
        std::string GetLastErrorAsString();
    }
}