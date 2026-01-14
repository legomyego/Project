#!/bin/bash

# Script to create an admin user
# Usage: ./create-admin.sh [email] [username] [password]

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

# Default values
EMAIL="${1:-admin@recipes.local}"
USERNAME="${2:-admin}"
PASSWORD="${3:-admin123}"

API_URL="${API_URL:-http://localhost:5010}"

echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Creating Admin User                                        ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if API is running
echo -e "${YELLOW}▶ Checking API connection...${NC}"
if ! curl -s "$API_URL/api/health" > /dev/null 2>&1; then
    echo -e "${RED}✗ API is not running at $API_URL${NC}"
    echo -e "${YELLOW}  Please start the API first: npm run dev:api${NC}"
    exit 1
fi
echo -e "${GREEN}✓ API is running${NC}"
echo ""

# Register user
echo -e "${YELLOW}▶ Registering user...${NC}"
echo -e "   Email:    ${BLUE}$EMAIL${NC}"
echo -e "   Username: ${BLUE}$USERNAME${NC}"
echo -e "   Password: ${BLUE}$PASSWORD${NC}"
echo ""

REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

# Check if registration was successful
if echo "$REGISTER_RESPONSE" | grep -q "error"; then
    ERROR=$(echo "$REGISTER_RESPONSE" | jq -r '.error' 2>/dev/null || echo "$REGISTER_RESPONSE")
    echo -e "${RED}✗ Registration failed: $ERROR${NC}"

    # If user already exists, try to find their ID
    if echo "$ERROR" | grep -qi "already"; then
        echo ""
        echo -e "${YELLOW}User might already exist. Try logging in with these credentials:${NC}"
        echo -e "   Email:    ${BLUE}$EMAIL${NC}"
        echo -e "   Password: ${BLUE}$PASSWORD${NC}"
        echo ""
        echo -e "${YELLOW}If you know the user ID, you can make them admin with:${NC}"
        echo -e "   ${BLUE}curl -X POST $API_URL/api/admin/make-admin/USER_ID${NC}"
    fi
    exit 1
fi

USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.id')
echo -e "${GREEN}✓ User registered${NC}"
echo -e "   User ID: ${BLUE}$USER_ID${NC}"
echo ""

# Make admin
echo -e "${YELLOW}▶ Granting admin privileges...${NC}"
ADMIN_RESPONSE=$(curl -s -X POST "$API_URL/api/admin/make-admin/$USER_ID")

if echo "$ADMIN_RESPONSE" | grep -q "is now an admin"; then
    echo -e "${GREEN}✓ Admin privileges granted${NC}"
else
    echo -e "${RED}✗ Failed to grant admin privileges${NC}"
    echo "$ADMIN_RESPONSE" | jq 2>/dev/null || echo "$ADMIN_RESPONSE"
    exit 1
fi

echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  ✓ Admin user created successfully!                        ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}Login credentials:${NC}"
echo -e "   Email:    ${GREEN}$EMAIL${NC}"
echo -e "   Username: ${GREEN}$USERNAME${NC}"
echo -e "   Password: ${GREEN}$PASSWORD${NC}"
echo ""
echo -e "${BLUE}Access admin panel at:${NC}"
echo -e "   ${GREEN}http://localhost:5173${NC}"
echo -e "   ${GREEN}http://admin.recipes.local${NC} (if nginx is configured)"
echo ""
