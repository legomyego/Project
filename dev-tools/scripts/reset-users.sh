#!/bin/bash

# Script to reset users - keeps only the specified admin user
# Usage: ./reset-users.sh <admin-email> <admin-username> <admin-password>

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

ADMIN_EMAIL="${1:-myadmin@recipes.local}"
ADMIN_USERNAME="${2:-myadmin}"
ADMIN_PASSWORD="${3:-admin123}"
API_URL="${API_URL:-http://localhost:5010}"

echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Reset Users - Keep Only Admin                             ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${YELLOW}⚠️  WARNING: This will delete the database and recreate it!${NC}"
echo -e "${YELLOW}   Only the admin user will remain.${NC}"
echo ""
echo -e "${YELLOW}Admin to keep:${NC}"
echo -e "   Email:    ${GREEN}$ADMIN_EMAIL${NC}"
echo -e "   Username: ${GREEN}$ADMIN_USERNAME${NC}"
echo -e "   Password: ${GREEN}$ADMIN_PASSWORD${NC}"
echo ""
read -p "Continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo -e "${RED}Cancelled${NC}"
    exit 1
fi

echo ""
echo -e "${BLUE}▶ Step 1: Dropping database...${NC}"

cd backend/RecipesApi

# Drop database using EF Core
dotnet ef database drop --force > /dev/null 2>&1
echo -e "${GREEN}✓ Database dropped${NC}"

echo ""
echo -e "${BLUE}▶ Step 2: Recreating database with migrations...${NC}"

# Apply migrations to recreate database
dotnet ef database update > /dev/null 2>&1
echo -e "${GREEN}✓ Database recreated${NC}"

echo ""
echo -e "${BLUE}▶ Step 3: Creating admin user...${NC}"

cd ../..

# Wait for API to be ready (if running)
sleep 2

# Create admin user
./create-admin.sh "$ADMIN_EMAIL" "$ADMIN_USERNAME" "$ADMIN_PASSWORD" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Admin user created${NC}"
else
    echo -e "${RED}✗ Failed to create admin user${NC}"
    echo -e "${YELLOW}Make sure API is running: npm run dev${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  ✓ Database reset complete!                                 ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}Fresh database with only admin user:${NC}"
echo -e "   Email:    ${GREEN}$ADMIN_EMAIL${NC}"
echo -e "   Username: ${GREEN}$ADMIN_USERNAME${NC}"
echo -e "   Password: ${GREEN}$ADMIN_PASSWORD${NC}"
echo ""
echo -e "${BLUE}Access:${NC}"
echo -e "   Main App:    ${GREEN}http://recipes.local${NC} or ${GREEN}http://localhost:3000${NC}"
echo -e "   Admin Panel: ${GREEN}http://admin.recipes.local${NC} or ${GREEN}http://localhost:5173${NC}"
echo ""
