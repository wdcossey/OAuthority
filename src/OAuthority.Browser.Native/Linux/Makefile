CC=c++
CFLAGS=-std=c++2a -Wall -O2 -shared -fPIC

# Auto-detect architecture → .NET Runtime Identifier
UNAME_M := $(shell uname -m)
ifeq ($(UNAME_M), x86_64)
  RID = linux-x64
else ifeq ($(UNAME_M), aarch64)
  RID = linux-arm64
else
  RID = linux-$(UNAME_M)
endif

DEST=../../runtimes/$(RID)/native/OAuthority.Native.so

all: build

install-deps:
	sudo dnf install -y gtk3-devel webkit2gtk4.1-devel || \
	sudo apt-get install -y libgtk-3-dev libwebkit2gtk-4.1-dev

build:
	mkdir -p ../../runtimes/$(RID)/native
	$(CC) -o $(DEST) $(CFLAGS) \
		OAuthority.Linux.cpp \
		`pkg-config --cflags --libs gtk+-3.0 webkit2gtk-4.1`
	@echo "Built: $(DEST)"

clean:
	rm -f $(DEST)