#!/bin/bash

# Add recipes.local entries to /etc/hosts
# This script needs to be run with sudo

HOSTS_ENTRIES="# Recipes PWA - Local Development
127.0.0.1       recipes.local
127.0.0.1       api.recipes.local
127.0.0.1       admin.recipes.local"

# Check if entries already exist
if grep -q "recipes.local" /etc/hosts; then
    echo "✓ recipes.local entries already exist in /etc/hosts"
else
    echo "Adding entries to /etc/hosts..."
    echo "$HOSTS_ENTRIES" >> /etc/hosts
    echo "✓ /etc/hosts updated"
fi
