// OAuthority.Mac.mm
// Native WKWebView browser window for OAuthority on macOS
//
// BUILD:
//   c++ -o OAuthority.Native.dylib -std=c++2a -shared -fPIC \
//       -framework Cocoa -framework WebKit \
//       OAuthority.Mac.mm

#import <Cocoa/Cocoa.h>
#import <WebKit/WebKit.h>
#include <string>

// TODO: Implement using WKWebView + WKNavigationDelegate
// 1. Create NSWindow with WKWebView
// 2. Implement WKNavigationDelegate to intercept redirect URI
// 3. Run NSApplication event loop until redirect is captured

extern "C" {

char* OAuthority_Browse(
    const char* title,
    const char* authUrl,
    const char* redirectUri,
    int width,
    int height)
{
    // TODO: Implement WKWebView window
    return nullptr;
}

void OAuthority_Free(char* ptr)
{
    free(ptr);
}

} // extern "C"