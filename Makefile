#General vars
CONFIG=Release
ARGS?=/restore /p:Configuration=$(CONFIG)
VS_PATH?=/Applications/Visual\ Studio\ \(Preview\).app
VS_DEBUG_PATH?=../vsmac/main/build/bin/VisualStudio.app

NET_VERSION=net471
PROJECT_NAME=VSMacExtension
PROJECT_VERSION=1.1
all:
	echo "Building $(PROJECT_NAME)..."
	msbuild /restore $(PROJECT_NAME).sln

clean:
	find . -type d -name bin -exec rm -rf {} \;
	find . -type d -name obj -exec rm -rf {} \;
	find . -type d -name packages -exec rm -rf {} \;

pack: all
	mono $(VS_PATH)/Contents/MonoBundle/vstool.exe setup pack $(CURDIR)/$(PROJECT_NAME)/bin/$(CONFIG)/$(NET_VERSION)/$(PROJECT_NAME).dll

pack_debug: all
	mono $(VS_DEBUG_PATH)/Contents/MonoBundle/vstool.exe setup pack $(CURDIR)/$(PROJECT_NAME)/bin/$(CONFIG)/$(NET_VERSION)/$(PROJECT_NAME).dll

install: pack
	mono $(VS_PATH)/Contents/MonoBundle/vstool.exe setup install $(CURDIR)/$(PROJECT_NAME).$(PROJECT_NAME)_$(PROJECT_VERSION).mpack

install_debug: pack_debug
	mono $(VS_DEBUG_PATH)/Contents/MonoBundle/vstool.exe setup install $(CURDIR)/$(PROJECT_NAME).$(PROJECT_NAME)_$(PROJECT_VERSION).mpack

.PHONY: all clean pack pack_debug install install_debug
