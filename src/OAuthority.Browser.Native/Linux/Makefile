CC=c++
CFLAGS=-std=c++2a -Wall -O2 -shared -fPIC
DEST=../../runtimes/linux-x64/native/OAuthority.Native.so

all: build

install-deps:
	sudo dnf install -y gtk3-devel webkit2gtk4.1-devel || \
	sudo apt-get install -y libgtk-3-dev libwebkit2gtk-4.1-dev

build:
	mkdir -p ../../runtimes/linux-x64/native
	$(CC) -o $(DEST) $(CFLAGS) \
		OAuthority.Linux.cpp \
		`pkg-config --cflags --libs gtk+-3.0 webkit2gtk-4.1`
	@echo "Built: $(DEST)"

clean:
	rm -f $(DEST)