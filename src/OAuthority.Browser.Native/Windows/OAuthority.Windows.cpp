// OAuthority.Windows.cpp
// Native WebView2 browser window for OAuthority on Windows
//
// Requirements:
//   - WebView2 Runtime installed (ships with modern Windows / Edge)
//   - Microsoft.Web.WebView2 NuGet package for the loader
//   - WebView2LoaderShim.lib or WebView2Loader.dll
//
// TODO: Implement using WebView2 COM API
// Reference: https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/win32

#include <windows.h>
#include <wrl.h>
#include <string>

// WebView2 headers - requires WebView2 SDK
// #include <WebView2.h>

extern "C" {

__declspec(dllexport) char* OAuthority_Browse(
    const char* title,
    const char* authUrl,
    const char* redirectUri,
    int width,
    int height)
{
    // TODO: Implement WebView2 window
    // 1. Create Win32 window
    // 2. Create WebView2 environment and controller
    // 3. Navigate to authUrl
    // 4. Hook NavigationStarting event to intercept redirectUri
    // 5. Return the redirect URL
    return nullptr;
}

__declspec(dllexport) void OAuthority_Free(char* ptr)
{
    free(ptr);
}

} // extern "C"