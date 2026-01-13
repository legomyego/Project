#!/bin/bash

# Development startup script
# Starts all services in development mode with live reload
# Each service runs in a separate terminal tab/window

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting Recipe PWA Development Environment${NC}"
echo ""

# Check if PostgreSQL is running
if ! pg_isready -h localhost -p 5432 > /dev/null 2>&1; then
    echo -e "${YELLOW}⚠️  PostgreSQL is not running on localhost:5432${NC}"
    echo "Please start PostgreSQL first:"
    echo "  - Using Docker: docker-compose up db -d"
    echo "  - Or install locally and start the service"
    exit 1
fi

echo -e "${GREEN}✓ PostgreSQL is running${NC}"
echo ""

# Function to open a new terminal tab and run a command (macOS)
run_in_tab() {
    local title=$1
    local command=$2
    osascript -e "tell application \"Terminal\"
        do script \"cd $(pwd) && echo -e '\\033[1;34m$title\\033[0m' && $command\"
        set custom title of front window to \"$title\"
    end tell" > /dev/null 2>&1
}

# Start Backend API
echo -e "${BLUE}▶ Starting .NET Backend API on http://localhost:5010${NC}"
run_in_tab "Backend API" "cd backend/RecipesApi && dotnet watch run"

# Wait a bit for backend to initialize
sleep 2

# Start Nuxt Frontend
echo -e "${BLUE}▶ Starting Nuxt App on http://localhost:3000${NC}"
run_in_tab "Nuxt App" "cd frontend/app && npm run dev"

# Start React Admin
echo -e "${BLUE}▶ Starting React Admin on http://localhost:5173${NC}"
run_in_tab "React Admin" "cd frontend/admin && npm run dev"

echo ""
echo -e "${GREEN}✓ All services starting!${NC}"
echo ""
echo "Services will be available at:"
echo "  - Main App:    ${BLUE}http://localhost:3000${NC}"
echo "  - Admin Panel: ${BLUE}http://localhost:5173${NC}"
echo "  - API:         ${BLUE}http://localhost:5010${NC}"
echo "  - API Docs:    ${BLUE}http://localhost:5010/swagger${NC}"
echo ""
echo "To stop all services: Close the terminal tabs or press Ctrl+C in each"
