#include "../include/Utilities.h"
#include <Windows.h>
#include <string>
#include <fstream>
#include <chrono>
#include <ctime>
#include <sstream>
#include <iomanip>
#include <iostream>

namespace PinPoint {
    namespace Utilities {
        
        // Log file
        static std::ofstream s_logFile;
        
        // Log a message with a specific log level
        void Log(const std::string& message, LogLevel level) {
            // Level prefixes
            const char* levelStrings[] = {
                "[DEBUG] ",
                "[INFO] ",
                "[WARNING] ",
                "[ERROR] "
            };
            
            // Get level string
            const char* levelStr = levelStrings[static_cast<int>(level)];
            
            // Output to console
            std::cout << levelStr << message << std::endl;
            
            // Open log file if not already open
            if (!s_logFile.is_open()) {
                s_logFile.open("PinPoint.log", std::ios::out | std::ios::app);
            }
            
            // Write to log file
            if (s_logFile.is_open()) {
                s_logFile << levelStr << message << std::endl;
                s_logFile.flush();
            }
        }
        
        // Get the directory where the application is running
        std::string GetExecutablePath() {
            char buffer[MAX_PATH];
            GetModuleFileNameA(NULL, buffer, MAX_PATH);
            
            std::string::size_type pos = std::string(buffer).find_last_of("\\/");
            return std::string(buffer).substr(0, pos);
        }
        
        // Create a console window for debug output
        void CreateConsoleWindow() {
            if (AllocConsole()) {
                FILE* pFile = nullptr;
                freopen_s(&pFile, "CONOUT$", "w", stdout);
                freopen_s(&pFile, "CONOUT$", "w", stderr);
                
                SetConsoleTitle(L"PinPoint Debug Console");
                
                Log("Debug console created", LogLevel::Debug);
            }
        }
        
        // Close the console window
        void CloseConsoleWindow() {
            FreeConsole();
            
            // Close the log file
            if (s_logFile.is_open()) {
                s_logFile.close();
            }
        }
        
        // Window utilities
        struct EnumWindowsData {
            DWORD processId;
            HWND result;
        };
        
        BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam) {
            EnumWindowsData* data = reinterpret_cast<EnumWindowsData*>(lParam);
            
            DWORD windowProcessId;
            GetWindowThreadProcessId(hwnd, &windowProcessId);
            
            if (windowProcessId == data->processId) {
                data->result = hwnd;
                return FALSE; // Stop enumeration
            }
            
            return TRUE; // Continue enumeration
        }
        
        HWND FindWindowByProcessId(DWORD processId) {
            EnumWindowsData data = { processId, NULL };
            EnumWindows(EnumWindowsProc, reinterpret_cast<LPARAM>(&data));
            return data.result;
        }
        
        bool IsWindowFullscreen(HWND hwnd) {
            RECT windowRect;
            if (GetWindowRect(hwnd, &windowRect)) {
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;
                
                return (windowRect.left == 0 && 
                        windowRect.top == 0 && 
                        width == GetSystemMetrics(SM_CXSCREEN) && 
                        height == GetSystemMetrics(SM_CYSCREEN));
            }
            return false;
        }
        
        // String utilities
        std::wstring StringToWideString(const std::string& str) {
            if (str.empty()) return std::wstring();
            
            int size_needed = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
            std::wstring wstr(size_needed, 0);
            MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstr[0], size_needed);
            
            return wstr;
        }
        
        std::string WideStringToString(const std::wstring& wstr) {
            if (wstr.empty()) return std::string();
            
            int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
            std::string str(size_needed, 0);
            WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &str[0], size_needed, NULL, NULL);
            
            return str;
        }
        
        // Error handling
        std::string GetLastErrorAsString() {
            DWORD error = GetLastError();
            if (error == 0) {
                return "No error";
            }
            
            LPSTR messageBuffer = nullptr;
            size_t size = FormatMessageA(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                NULL, 
                error, 
                MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), 
                (LPSTR)&messageBuffer, 
                0, 
                NULL
            );
            
            std::string message(messageBuffer, size);
            LocalFree(messageBuffer);
            
            return message;
        }
    }
}