#!/bin/bash

# Script to cleanly restart all dev servers
# Kills old processes and starts fresh

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🔄 Restarting development servers...${NC}"
echo ""

# Step 1: Kill old processes
echo -e "${YELLOW}▶ Stopping old processes...${NC}"

# Kill node processes (Nuxt, React admin)
pkill -f "nuxt dev" 2>/dev/null && echo "  ✓ Stopped Nuxt" || echo "  - No Nuxt running"
pkill -f "vite.*5173" 2>/dev/null && echo "  ✓ Stopped React admin" || echo "  - No React admin running"
pkill -f "concurrently" 2>/dev/null && echo "  ✓ Stopped concurrently" || echo "  - No concurrently running"

# Kill dotnet processes (backend API)
pkill -f "dotnet.*RecipesApi" 2>/dev/null && echo "  ✓ Stopped .NET API" || echo "  - No .NET API running"

# Wait for processes to stop
sleep 1

# Check if ports are free
echo ""
echo -e "${YELLOW}▶ Checking ports...${NC}"

if lsof -i :3000 > /dev/null 2>&1; then
    echo -e "  ${RED}✗ Port 3000 still in use${NC}"
    echo "    Killing process on port 3000..."
    lsof -ti:3000 | xargs kill -9 2>/dev/null
else
    echo "  ✓ Port 3000 free"
fi

if lsof -i :5010 > /dev/null 2>&1; then
    echo -e "  ${RED}✗ Port 5010 still in use${NC}"
    echo "    Killing process on port 5010..."
    lsof -ti:5010 | xargs kill -9 2>/dev/null
else
    echo "  ✓ Port 5010 free"
fi

if lsof -i :5173 > /dev/null 2>&1; then
    echo -e "  ${RED}✗ Port 5173 still in use${NC}"
    echo "    Killing process on port 5173..."
    lsof -ti:5173 | xargs kill -9 2>/dev/null
else
    echo "  ✓ Port 5173 free"
fi

echo ""
echo -e "${GREEN}✓ All processes stopped${NC}"
echo ""
echo -e "${BLUE}🚀 Starting fresh dev servers...${NC}"
echo ""
echo -e "${YELLOW}Run: npm run dev${NC}"
echo ""
