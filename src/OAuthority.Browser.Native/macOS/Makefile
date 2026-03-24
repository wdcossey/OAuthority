CC=c++
CFLAGS=-std=c++2a -Wall -O2 -shared -fPIC
FRAMEWORKS=-framework Cocoa -framework WebKit

DEST_X64=../../runtimes/osx-x64/native/OAuthority.Native.dylib
DEST_ARM64=../../runtimes/osx-arm64/native/OAuthority.Native.dylib
DEST_UNIVERSAL=../../runtimes/osx/native/OAuthority.Native.dylib

all: universal

x64:
	mkdir -p ../../runtimes/osx-x64/native
	$(CC) -o $(DEST_X64) $(CFLAGS) -arch x86_64 $(FRAMEWORKS) OAuthority.Mac.mm
	@echo "Built: $(DEST_X64)"

arm64:
	mkdir -p ../../runtimes/osx-arm64/native
	$(CC) -o $(DEST_ARM64) $(CFLAGS) -arch arm64 $(FRAMEWORKS) OAuthority.Mac.mm
	@echo "Built: $(DEST_ARM64)"

universal: x64 arm64
	mkdir -p ../../runtimes/osx/native
	lipo -create -output $(DEST_UNIVERSAL) $(DEST_X64) $(DEST_ARM64)
	@echo "Built universal: $(DEST_UNIVERSAL)"

clean:
	rm -f $(DEST_X64) $(DEST_ARM64) $(DEST_UNIVERSAL)
