# Project Resources

This folder contains project documentation and utility scripts.

## 📚 Documentation (`/docs`)

All project documentation files:

- **[DOMAINS.md](docs/DOMAINS.md)** - Local domain configuration with nginx
- **[LOCAL-DOMAIN.md](docs/LOCAL-DOMAIN.md)** - Additional domain setup info
- **[SCRIPTS.md](docs/SCRIPTS.md)** - Description of available scripts
- **[SETUP-SUMMARY.md](docs/SETUP-SUMMARY.md)** - Complete setup guide
- **[START.md](docs/START.md)** - Quick start instructions
- **[USAGE.md](docs/USAGE.md)** - Usage examples and workflows

## 🔧 Scripts (`/scripts`)

Utility scripts for development and administration:

### Development
- **[start-dev.sh](scripts/start-dev.sh)** - Start all services in dev mode (opens Terminal tabs)
- **[restart-dev.sh](scripts/restart-dev.sh)** - Restart all development services
- **[help.sh](scripts/help.sh)** - Show available commands and usage

### Administration
- **[create-admin.sh](scripts/create-admin.sh)** - Create admin user account
- **[reset-users.sh](scripts/reset-users.sh)** - Reset user database

### Configuration
- **[setup-local-domain.sh](scripts/setup-local-domain.sh)** - Setup local domain configuration
- **[update-hosts.sh](scripts/update-hosts.sh)** - Update /etc/hosts file

## Usage

All scripts should be run from the project root:

```bash
# Example: Start dev environment
./dev-tools/scripts/start-dev.sh

# Example: Create admin user
./dev-tools/scripts/create-admin.sh
```

## Structure

```
dev-tools/
├── docs/          # Documentation files (.md)
├── scripts/       # Utility scripts (.sh)
└── README.md      # This file
```
