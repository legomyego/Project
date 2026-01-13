#!/bin/bash

# Setup script for local domain configuration
# Configures recipes.local domain with nginx reverse proxy

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Setting up recipes.local domain for development           ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if running on macOS
if [[ "$OSTYPE" != "darwin"* ]]; then
    echo -e "${RED}✗ This script is designed for macOS${NC}"
    exit 1
fi

# Step 1: Check/Install nginx
echo -e "${BLUE}▶ Step 1: Checking nginx installation${NC}"
if ! command -v nginx &> /dev/null; then
    echo -e "${YELLOW}  nginx not found. Installing via Homebrew...${NC}"
    if ! command -v brew &> /dev/null; then
        echo -e "${RED}✗ Homebrew not found. Please install: https://brew.sh${NC}"
        exit 1
    fi
    brew install nginx
    echo -e "${GREEN}✓ nginx installed${NC}"
else
    echo -e "${GREEN}✓ nginx already installed${NC}"
fi
echo ""

# Step 2: Configure nginx
echo -e "${BLUE}▶ Step 2: Configuring nginx${NC}"
NGINX_CONF_DIR="/usr/local/etc/nginx/servers"
NGINX_CONF_FILE="$NGINX_CONF_DIR/recipes.conf"

# Create servers directory if it doesn't exist
if [ ! -d "$NGINX_CONF_DIR" ]; then
    echo -e "${YELLOW}  Creating nginx servers directory...${NC}"
    sudo mkdir -p "$NGINX_CONF_DIR"
fi

# Copy nginx configuration
echo -e "${YELLOW}  Copying nginx configuration...${NC}"
sudo cp nginx-local.conf "$NGINX_CONF_FILE"
echo -e "${GREEN}✓ nginx configured at $NGINX_CONF_FILE${NC}"
echo ""

# Step 3: Update /etc/hosts
echo -e "${BLUE}▶ Step 3: Updating /etc/hosts${NC}"
HOSTS_FILE="/etc/hosts"
HOSTS_ENTRIES="# Recipes PWA - Local Development
127.0.0.1       recipes.local
127.0.0.1       api.recipes.local
127.0.0.1       admin.recipes.local"

# Check if entries already exist
if grep -q "recipes.local" "$HOSTS_FILE"; then
    echo -e "${YELLOW}  entries already exist in /etc/hosts${NC}"
else
    echo -e "${YELLOW}  Adding entries to /etc/hosts (requires sudo)...${NC}"
    echo "$HOSTS_ENTRIES" | sudo tee -a "$HOSTS_FILE" > /dev/null
    echo -e "${GREEN}✓ /etc/hosts updated${NC}"
fi
echo ""

# Step 4: Test nginx configuration
echo -e "${BLUE}▶ Step 4: Testing nginx configuration${NC}"
if sudo nginx -t &> /dev/null; then
    echo -e "${GREEN}✓ nginx configuration is valid${NC}"
else
    echo -e "${RED}✗ nginx configuration has errors${NC}"
    sudo nginx -t
    exit 1
fi
echo ""

# Step 5: Start/Restart nginx
echo -e "${BLUE}▶ Step 5: Starting nginx${NC}"
if brew services list | grep nginx | grep started &> /dev/null; then
    echo -e "${YELLOW}  Restarting nginx...${NC}"
    brew services restart nginx
else
    echo -e "${YELLOW}  Starting nginx...${NC}"
    brew services start nginx
fi

# Wait for nginx to start
sleep 2

if brew services list | grep nginx | grep started &> /dev/null; then
    echo -e "${GREEN}✓ nginx is running${NC}"
else
    echo -e "${RED}✗ Failed to start nginx${NC}"
    exit 1
fi
echo ""

# Success message
echo -e "${GREEN}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  ✓ Setup complete!                                          ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}Your services are now accessible at:${NC}"
echo ""
echo -e "  ${GREEN}Main App:${NC}    http://recipes.local"
echo -e "  ${GREEN}Admin Panel:${NC} http://admin.recipes.local"
echo -e "  ${GREEN}API:${NC}         http://api.recipes.local"
echo -e "  ${GREEN}Swagger:${NC}     http://api.recipes.local/swagger"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Start your dev servers: ${BLUE}npm run dev${NC}"
echo -e "  2. Open ${BLUE}http://recipes.local${NC} in your browser"
echo ""
echo -e "${YELLOW}To stop nginx:${NC} ${BLUE}brew services stop nginx${NC}"
echo -e "${YELLOW}To view logs:${NC}  ${BLUE}tail -f /usr/local/var/log/nginx/*.log${NC}"
echo ""
