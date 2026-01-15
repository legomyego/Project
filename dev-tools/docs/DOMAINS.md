# Local Domain Configuration

This project uses local domains for development instead of ports for better organization and production-like setup.

## Configured Domains

- **Main App**: http://recipes.local
- **Admin Panel**: http://admin.recipes.local
- **API**: http://api.recipes.local (optional, you can also use recipes.local/api)

## How It Works

1. **nginx** acts as reverse proxy and routes requests:
   - `recipes.local` → Nuxt dev server (localhost:3000)
   - `admin.recipes.local` → Vite dev server (localhost:5173)
   - `*.recipes.local/api/*` → .NET API (localhost:5010)

2. **DNS**: `/etc/hosts` file resolves `*.recipes.local` to `127.0.0.1`

3. **Environment variables**: Each frontend knows its API base URL through `.env` files

## Setup (Already Done ✅)

```bash
# 1. Install nginx (if not installed)
brew install nginx

# 2. Add domains to /etc/hosts (already added)
echo "127.0.0.1  recipes.local" | sudo tee -a /etc/hosts
echo "127.0.0.1  api.recipes.local" | sudo tee -a /etc/hosts
echo "127.0.0.1  admin.recipes.local" | sudo tee -a /etc/hosts

# 3. nginx config is in /opt/homebrew/etc/nginx/servers/recipes.conf

# 4. Start nginx
brew services start nginx
```

## Usage

### Start Development Services

```bash
# Option 1: Use npm script (starts all 3 services)
npm run dev

# Option 2: Manual start
cd backend/RecipesApi && dotnet watch run &
cd frontend/app && npm run dev &
cd frontend/admin && npm run dev &
```

### Access Applications

- Main App: **http://recipes.local**
- Admin Panel: **http://admin.recipes.local**
- API Health: **http://recipes.local/api/health**
- Swagger: **http://api.recipes.local/swagger** (direct API access)

### Restart nginx

```bash
# If you change nginx config
brew services restart nginx

# Check nginx status
brew services list | grep nginx
```

## Troubleshooting

### Domains not working

```bash
# Check if nginx is running
brew services list | grep nginx

# Test nginx config
nginx -t

# Restart nginx
brew services restart nginx

# Check if domain resolves
ping recipes.local
```

### 502 Bad Gateway

This means nginx is running but can't reach the backend services. Check:

```bash
# Are services running?
ps aux | grep -E "(dotnet|npm)" | grep -v grep

# Check ports
lsof -i :3000  # Nuxt
lsof -i :5173  # Admin
lsof -i :5010  # API
```

### Clear browser cache

If you see old localhost URLs, clear browser cache or use incognito mode.

## Benefits of Using Domains

1. **Production-like setup** - same domain structure as production
2. **Better cookie handling** - cookies work correctly across subdomains
3. **No CORS issues** - proper domain-based CORS configuration
4. **Cleaner URLs** - easier to remember than ports
5. **SSO works properly** - shared authentication between apps

## Switching Back to Localhost

If you prefer localhost ports:

```bash
# Stop nginx
brew services stop nginx

# Access directly:
# - http://localhost:3000 (Nuxt)
# - http://localhost:5173 (Admin)
# - http://localhost:5010 (API)
```

Note: You'll need to update environment variables in `.env` files to use localhost URLs.
