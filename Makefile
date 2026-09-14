# ============================================================================
# Dungeon Lord — one-command export (Linux desktop + Web/WASM)
#
#   make setup        bootstrap Godot (C#) + .NET SDK + export templates
#   make export-all   one-command build: exports Linux + Web into builds/
#   make export-linux  export desktop Linux build only
#   make export-web    export Web (WASM) build only
#   make run          launch the Linux build
#   make serve        serve the Web build locally (python3 http.server)
#   make test         Python tooling tests
#   make clean        remove exports
#
# Everything is self-hosting: on a fresh Linux machine with only `make`,
# `curl` and `unzip` available, `make setup && make export-all` produces both
# builds under builds/.
# ============================================================================

SHELL      := /bin/bash
GODOT_TAG  := 4.2.2-stable
GODOT_VER  := 4.2.2
TOOLS_DIR  := .tools
GODOT_DIR  := $(TOOLS_DIR)/godot
DOTNET_DIR := $(TOOLS_DIR)/dotnet
BUILD_DIR  := builds
TEMPL_DIR  := $(HOME)/.local/share/godot/export_templates/$(GODOT_VER).stable

GODOT_ZIP  := $(TOOLS_DIR)/Godot_v$(GODOT_TAG)_mono_linux_x86_64.zip
TEMPL_TPZ  := $(TOOLS_DIR)/Godot_v$(GODOT_TAG)_export_templates.tpz
GODOT_BIN  := $(GODOT_DIR)/Godot_v$(GODOT_TAG)_mono_linux_x86_64/Godot_v$(GODOT_TAG)_mono_linux.x86_64
DOTNET_BIN := $(DOTNET_DIR)/dotnet

GODOT_URL  := https://github.com/godotengine/godot/releases/download/$(GODOT_TAG)/Godot_v$(GODOT_TAG)_mono_linux_x86_64.zip
# C#/.NET projects require the MONO export templates (they ship the `_mono`
# variants of each platform template); the standard tpz does not.
TEMPL_URL  := https://github.com/godotengine/godot/releases/download/$(GODOT_TAG)/Godot_v$(GODOT_TAG)_mono_export_templates.tpz
DOTNET_URL := https://dot.net/v1/dotnet-install.sh

# ----------------------------------------------------------------------------
# Targets
# ----------------------------------------------------------------------------

.PHONY: setup export-all export-linux export-web run serve test clean

setup: $(GODOT_BIN) $(DOTNET_BIN) templates
	@echo ""
	@echo "Setup complete. Run 'make export-all' to build into $(BUILD_DIR)/"

export-all: export-linux export-web
	@echo ""
	@echo "Export complete:"
	@ls -la $(BUILD_DIR)/dungeon_lord.x86_64 \
	      $(BUILD_DIR)/dungeon_lord.pck \
	      $(BUILD_DIR)/web/index.html 2>/dev/null || true

export-linux:
	@echo "==> Exporting Linux desktop build"
	$(MAKE) _export PRESET="Linux"
	@echo "==> Done: $(BUILD_DIR)/dungeon_lord.x86_64"

export-web:
	@echo "==> Exporting Web (WASM) build"
	$(MAKE) _export PRESET="Web"
	@echo "==> Done: $(BUILD_DIR)/web/index.html"

run: $(BUILD_DIR)/dungeon_lord.x86_64
	./$(BUILD_DIR)/dungeon_lord.x86_64

serve:
	@python3 -m http.server 8080 --directory $(BUILD_DIR)/web

test:
	@if [ -d .venv ]; then source .venv/bin/activate; fi; \
	python3 -m pytest tests/ -v

clean:
	rm -rf $(BUILD_DIR) .godot

# ----------------------------------------------------------------------------
# Internal helpers (do not call directly)
# ----------------------------------------------------------------------------

_export: $(GODOT_BIN) $(DOTNET_BIN) templates
	@mkdir -p $(BUILD_DIR)
	@echo "    using dotnet: $(DOTNET_BIN)"
	@export PATH="$(DOTNET_DIR):$$PATH"; \
	export DOTNET_ROOT="$(DOTNET_DIR)"; \
	$(GODOT_BIN) --headless --path . --import; \
	$(GODOT_BIN) --headless --path . --export-release "$(PRESET)"

$(GODOT_BIN):
	@echo "==> Downloading Godot $(GODOT_TAG) (C#/mono)"
	mkdir -p $(GODOT_DIR)
	curl -L -o $(GODOT_ZIP) $(GODOT_URL)
	unzip -q -o $(GODOT_ZIP) -d $(GODOT_DIR)
	@rm -f $(GODOT_ZIP)

$(DOTNET_BIN):
	@echo "==> Installing .NET SDK 8"
	mkdir -p $(DOTNET_DIR)
	curl -L -o $(TOOLS_DIR)/dotnet-install.sh $(DOTNET_URL)
	chmod +x $(TOOLS_DIR)/dotnet-install.sh
	$(TOOLS_DIR)/dotnet-install.sh --channel 8.0 --install-dir $(DOTNET_DIR)

templates: $(TEMPL_DIR)/version.txt

$(TEMPL_DIR)/version.txt:
	@echo "==> Downloading export templates for $(GODOT_TAG)"
	@mkdir -p $(TEMPL_DIR) $(TOOLS_DIR)
	curl -L -o $(TEMPL_TPZ) $(TEMPL_URL)
	@tar -xf $(TEMPL_TPZ) -C $(TOOLS_DIR)
	@cp -r $(TOOLS_DIR)/templates/* $(TEMPL_DIR)/
	@rm -rf $(TOOLS_DIR)/templates $(TEMPL_TPZ)
	@echo "$(GODOT_VER).stable" > $(TEMPL_DIR)/version.txt