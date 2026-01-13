# CLAUDE.md — Recipes + Points PWA

## Project Overview

PWA application with points economy: users earn, buy, and exchange points to purchase recipes. Recipes can be traded between users. Personal dashboard for managing balance and recipe collection.

## Tech Stack

### Backend
- **.NET 8/9** with Minimal APIs
- **Entity Framework Core 8/9**
- **SQL Server**
- **JWT authentication** (shared between frontends)

### Frontend — Main Application
- **Vue 3 + Nuxt 3**
- **TypeScript**
- **PWA** via `@vite-pwa/nuxt`
- Covers: personal dashboard, recipe catalog, points system, trading

### Frontend — Microfrontend (Admin Panel)
- **React 18/19 + Vite**
- **TypeScript**
- **shadcn/ui** — UI components (buttons, cards, dialogs, etc.)
- **TanStack Table** — tables with sorting, filtering, pagination
- **TanStack Query** — API data fetching and caching
- **React Hook Form** — forms with validation
- Covers: recipe moderation, user management, analytics dashboard

## Architecture

```
┌─────────────────────────────────────────────────┐
│         Main App (Nuxt 3 / Vue 3)               │
│  /app/*                                         │
└─────────────────────────────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        ▼                             ▼
┌───────────────────┐    ┌────────────────────────┐
│  React Micro-     │    │   .NET 8/9 API         │
│  frontend         │    │   /api/*               │
│  /admin/*         │    │                        │
└───────────────────┘    └────────────────────────┘
```

- **Microfrontends**: Vue and React as separate applications
- **Integration**: reverse proxy (different paths or subdomains)
- **Shared**: backend API and JWT authentication

## Caching Strategy

### Backend (.NET)

**IMemoryCache** for hot data (built-in, free):
```csharp
builder.Services.AddMemoryCache();

app.MapGet("/recipes/popular", async (IMemoryCache cache, AppDbContext db) =>
{
    var recipes = await cache.GetOrCreateAsync("popular_recipes", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        return await db.Recipes.OrderByDescending(r => r.Views).Take(20).ToListAsync();
    });
    return recipes;
});
```

**Output Caching** for heavy endpoints:
```csharp
builder.Services.AddOutputCache();
app.UseOutputCache();

app.MapGet("/recipes/{id}", ...)
   .CacheOutput(p => p.Expire(TimeSpan.FromMinutes(5)));
```

### Frontend (Nuxt 3)

**useFetch with caching**:
```typescript
const { data: recipes } = await useFetch('/api/recipes/popular', {
  key: 'popular-recipes'
})
```

**PWA Service Worker**: automatic static caching + stale-while-revalidate for API.

### Caching Layers Summary
1. PWA Service Worker — offline + static assets
2. HTTP Cache-Control headers — browser cache
3. Nuxt useFetch — request deduplication
4. IMemoryCache — hot data on backend
5. Output Caching — heavy endpoints

**Note**: Redis is NOT needed for single-server setup. Consider adding when scaling to 2+ servers.

## Code Conventions

- **Detailed comments required** — this is a learning project, add explanatory comments explaining what code does and why
- Comments should be in **English**
- For HTML/CSS changes: provide **full file**, not partial diffs
- For JS-only changes: provide only the **changed script tag**, not the whole file

### Comment Style Examples

**.NET (C#):**
```csharp
// Configure JWT authentication with bearer tokens
// This middleware validates the token on each request and extracts user claims
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Token validation parameters define how we verify incoming tokens
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Ensure the token was signed with our secret key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            
            // ValidateIssuer = false means we accept tokens from any issuer
            // In production, set this to true and specify ValidIssuer
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
```

**Vue 3 / Nuxt:**
```typescript
// useAuth composable manages authentication state across the app
// It provides reactive user data and auth methods
export const useAuth = () => {
  // useState is Nuxt's SSR-safe alternative to ref()
  // The 'user' key ensures state is shared across components
  const user = useState<User | null>('user', () => null)
  
  // useCookie provides reactive cookie access that works on both server and client
  // httpOnly: false allows JavaScript to read the token (needed for API calls)
  const token = useCookie('auth_token', { httpOnly: false })
  
  // Login function: sends credentials, stores token, fetches user profile
  const login = async (email: string, password: string) => {
    // $fetch is Nuxt's universal fetch that works on server and client
    const response = await $fetch('/api/auth/login', {
      method: 'POST',
      body: { email, password }
    })
    
    // Store token in cookie — will be sent automatically with future requests
    token.value = response.token
    
    // Fetch and cache user profile
    user.value = await $fetch('/api/users/me')
  }
  
  return { user, token, login }
}
```

**React:**
```typescript
// TanStack Query hook for fetching recipes with caching
// This replaces manual useEffect + useState patterns
export const useRecipes = () => {
  return useQuery({
    // queryKey is used for caching — same key = same cached data
    // Array format allows automatic invalidation: ['recipes', id] 
    queryKey: ['recipes'],
    
    // queryFn is called when data is needed (not cached or stale)
    queryFn: async () => {
      const response = await fetch('/api/recipes')
      if (!response.ok) throw new Error('Failed to fetch')
      return response.json() as Promise<Recipe[]>
    },
    
    // staleTime: how long data is considered "fresh" (no refetch)
    // 5 minutes means we won't hit the API again for 5 min
    staleTime: 5 * 60 * 1000,
  })
}
```

## Project Structure (planned)

```
/
├── backend/
│   └── RecipesApi/          # .NET 8/9 Minimal API
├── frontend/
│   ├── app/                 # Nuxt 3 main application
│   └── admin/               # React microfrontend
├── docker-compose.yml
└── CLAUDE.md
```

## Learning Goals

- Modern .NET (Minimal APIs, new EF Core features)
- Vue 3 Composition API + Nuxt 3
- React basics for comparison
- Microfrontend architecture
- PWA development

## Development Plan

### Phase 0: Setup ✅ COMPLETE
- [x] Create repository (GitHub/GitLab)
- [x] Setup folder structure
- [x] Add CLAUDE.md
- [x] Docker Compose for local development (PostgreSQL)

**Result**: ✅ Project structure ready, database running

### Phase 1: Backend — Foundation ✅ COMPLETE
**Goal**: Working API with authentication

- [x] Initialize .NET 9 project (Minimal APIs)
- [x] EF Core + data models:
  - User (id, email, passwordHash, balance, createdAt)
  - Recipe (id, title, description, price, authorId, views, createdAt)
  - Transaction (id, userId, amount, type, recipeId, createdAt)
- [x] JWT authentication (register, login, logout)
- [x] Authentication endpoints:
  - POST /api/auth/register, /api/auth/login, /api/auth/logout
  - JWT in httpOnly cookies
- [x] User endpoints:
  - GET /api/users/me (profile + balance)
- [x] Recipe endpoints:
  - GET /api/recipes (with pagination)
  - GET /api/recipes/{id} (with view counter)
  - GET /api/recipes/popular (cached)
- [x] Swagger for testing
- [x] CORS configured

**Result**: ✅ API fully functional at http://localhost:5010

### Phase 2: Vue Frontend — Authentication ✅ COMPLETE
**Goal**: Working authentication and dashboard

- [x] Initialize Nuxt 3 project with TypeScript
- [x] Authentication composable (useAuth):
  - register(), login(), logout(), fetchUser()
  - JWT in httpOnly cookies
  - Reactive user state
- [x] Pages:
  - / — Landing page with features
  - /login — Login form
  - /register — Registration form
  - /dashboard — Protected dashboard with balance
- [x] Auth middleware for protected routes
- [x] Modern gradient design, responsive layout

**Result**: ✅ Users can register, login, access dashboard

### Phase 3: Points Economy & Purchasing ✅ COMPLETE
**Goal**: Working points system and recipe purchases

Backend:
- [x] POST /api/points/topup — add points (stub, ready for Stripe)
- [x] GET /api/points/transactions — transaction history with pagination
- [x] POST /api/recipes/{id}/buy — purchase recipe with points
  - Database transactions for consistency
  - Balance validation
  - Transfer points to author
  - Create purchase/sale transactions

Frontend:
- [x] Enhanced Dashboard (/dashboard):
  - Balance display with gradient
  - Points top-up form (quick buttons + custom)
  - Recent transaction history (10 latest)
  - Transaction icons (💰 TopUp, 🛒 Purchase, 💸 Sale)
  - Link to recipe catalog
- [x] Recipe Catalog (/recipes):
  - Grid layout with cards
  - Recipe details (title, description, price, views, author)
  - Buy button with loading states
  - "Your Recipe" indicator for own recipes
  - Purchase confirmation modal
  - Balance validation before purchase
  - Pagination support
- [x] Composables:
  - usePoints() — top-up and transactions
  - useRecipes() — listing and purchasing
  - Auto balance refresh

**Result**: ✅ Full purchase flow working — add points → browse → buy → see in history

### Phase 4: Recipe Trading System ✅ COMPLETE
**Goal**: Users can trade recipes with each other

Backend:
- [x] New models:
  - UserRecipe (many-to-many: users own recipes)
  - Trade (id, offeringUserId, offeredRecipeId, requestedUserId, requestedRecipeId, status, createdAt)
- [x] Recipe ownership endpoints:
  - GET /api/recipes/my — my purchased/traded recipes
- [x] Trade endpoints:
  - POST /api/trades/offer — offer a trade
  - GET /api/trades/incoming — incoming trade offers
  - GET /api/trades/outgoing — my trade offers
  - POST /api/trades/{id}/accept — accept trade
  - POST /api/trades/{id}/decline — decline trade
  - POST /api/trades/{id}/cancel — cancel my offer
- [x] Username system:
  - Added Username field to User model
  - Username search endpoint: GET /api/users/search?username={username}
  - Unique username validation

Frontend:
- [x] My Recipes page (/my-recipes):
  - List of owned recipes
  - "Trade Recipe" button on each
  - Shows acquisition type (Purchase/Trade)
- [x] Trade Offers page (/trades):
  - Tabs: Incoming / Outgoing
  - Offer details (who, what recipes, when)
  - Accept/Decline/Cancel buttons
  - Status indicators
- [x] Trade offer modal:
  - Username search input
  - Search for trading partners
  - Select recipe to request
  - Confirm trade offer
- [x] Registration with username
- [x] Username displayed throughout UI

**Result**: ✅ Full trading system working — search users → create offers → accept/decline → recipes exchanged

### Phase 5: Security & Account Management 🚧 IN PROGRESS
**Goal**: Production-ready security and user management

**Rate Limiting:**
- [x] ASP.NET Core 9 built-in rate limiting middleware
  - Fixed window limiter: 100 requests/minute for general API
  - Strict limiter: 5 requests/minute for auth endpoints (login, register)
  - IP-based throttling
  - Custom 429 (Too Many Requests) responses with retry-after metadata
  - Applied to authentication endpoints to prevent brute force attacks

**Password Management:**
- [x] Change password endpoint:
  - POST /api/users/change-password
  - Requires current password verification
  - Password strength validation (min 6 characters)
  - Prevents setting same password
  - Protected with authentication
- [ ] Forgot password flow (future):
  - Email service integration (SMTP)
  - Password reset tokens with expiration
  - Secure reset links

**Security Features:**
- [x] Rate limiting middleware configured
- [x] Password change with current password verification
- [x] JWT token validation on all protected endpoints
- [x] httpOnly cookies for token storage
- [x] BCrypt password hashing
- [ ] CAPTCHA integration (future - reCAPTCHA v3)
- [ ] Email verification (future)

**Frontend Updates:**
- [ ] Change password form in dashboard (future)
- [ ] Forgot password UI (future)
- [ ] Password strength indicator (future)

**Result**: 🎯 Core security features implemented — rate limiting active, password management working

### Phase 6: PWA ✅ COMPLETE
**Goal**: Install on phone, offline access

- [x] Connect @vite-pwa/nuxt
- [x] Manifest (icons, name, colors)
- [x] Service Worker — static caching with Workbox
- [x] Offline page with auto-reconnect detection
- [x] API response caching (network-first strategy)
- [x] Image caching (cache-first strategy)
- [x] App icons (192x192, 512x512)
- [x] Development mode PWA enabled

**Result**: ✅ App installable on mobile/desktop, works offline with cached data

### Phase 7: React Admin Panel ✅ COMPLETE
**Goal**: Learn React on real task — full-featured admin panel

**Setup & Infrastructure:**
- [x] Initialize React + Vite + TypeScript
- [x] Setup shadcn/ui (Button, Card, Table, Input, Label, Dialog components)
- [x] Authentication (same JWT as Nuxt app)
  - AuthContext with login/logout
  - Protected routes
  - Cookie-based authentication
  - IsAdmin check (rejects non-admin users)
- [x] TanStack Query — API connection and caching
- [x] React Router with authentication guards
- [x] Two-way navigation (admin ↔ main app)

**Subscription System:**
- [x] Subscription management (full CRUD):
  - Create new subscription plans
  - Edit subscription details (name, description, duration, price, isActive)
  - Delete (deactivate) subscriptions
  - View all subscriptions with cards
- [x] Recipe assignment to subscriptions:
  - SubscriptionDetailPage with recipe management
  - Search and multi-select recipes to add
  - Remove recipes from subscription
  - Live sync with backend
- [x] Backend integration:
  - Subscription, UserSubscription, SubscriptionRecipe models
  - Recipe.RequiresSubscription field
  - Access control for subscription-only recipes
  - User subscription purchase endpoint

**Recipe Management:**
- [x] Recipe list (TanStack Table):
  - Table with sorting support
  - Server-side pagination
  - Data fetching with TanStack Query
  - Edit and Delete actions
- [x] Recipe edit form (React Hook Form):
  - Edit title, description, price
  - Toggle "Requires Subscription" checkbox
  - Form validation
  - Optimistic updates
- [x] Recipe deletion with confirmation
- [x] Backend endpoints:
  - PUT /api/recipes/{id} — update recipe
  - DELETE /api/recipes/{id} — delete recipe
  - Permission check: only admin or author can edit/delete

**Admin Access Control:**
- [x] IsAdmin flag on User model
- [x] Admin detection in login flow
- [x] Temporary make-admin endpoint for setup
- [x] Admin-only route protection

**Pages Completed:**
- ✅ Login page with form validation
- ✅ Dashboard with stats cards, navigation, "Go to Main App" button
- ✅ Recipes page with TanStack Table, Edit/Delete
- ✅ Subscriptions page with full CRUD
- ✅ Subscription detail page with recipe assignment
- 📋 Users, Trades, Analytics (placeholders for future)

**Integration with Main App:**
- ✅ Nuxt dashboard shows "Admin Panel" button (only for admins)
- ✅ Admin panel dashboard shows "Go to Main App" button
- ✅ Shared JWT authentication across both apps
- ✅ Subscription purchase page in Nuxt app
- ✅ Active subscription display on Nuxt dashboard

**Running**: http://localhost:5173 (admin panel)
**Result**: ✅ Full-featured admin panel — manage subscriptions, assign recipes, edit/delete recipes, seamless integration with main app

### Phase 8: Deployment & Production ✅ COMPLETE
**Goal**: Containerize application and prepare for production deployment

**Docker Configuration:**
- [x] Backend Dockerfile (.NET 9 API)
  - Multi-stage build (build → publish → runtime)
  - Optimized with smaller aspnet runtime image
  - Exposes port 5010
- [x] Frontend App Dockerfile (Nuxt 3)
  - Multi-stage build with Node 20 Alpine
  - Standalone server output
  - Exposes port 3000
- [x] Frontend Admin Dockerfile (React)
  - Multi-stage build → nginx serving
  - Custom nginx config for SPA routing
  - Exposes port 80
- [x] .dockerignore for all services

**Nginx Reverse Proxy:**
- [x] Main nginx configuration
  - Routes `/api/*` → .NET backend
  - Routes `/admin/*` → React admin panel
  - Routes `/*` → Nuxt main app
  - Gzip compression enabled
  - Security headers configured
  - Health check endpoints

**Docker Compose:**
- [x] Complete orchestration for all services
  - PostgreSQL database with health checks
  - .NET API with JWT configuration
  - Nuxt app with environment variables
  - React admin panel
  - Nginx reverse proxy
- [x] Named volumes for data persistence
- [x] Custom network for service communication
- [x] Auto-restart policies

**CI/CD Pipeline:**
- [x] GitHub Actions workflow
  - Separate jobs for backend, frontend app, admin
  - Automated testing (lint, build, test)
  - Docker image building and pushing to GHCR
  - Cache optimization for faster builds
  - Deploy job (template for VPS/cloud)
- [x] Multi-architecture support ready

**Configuration Management:**
- [x] Environment variables (.env.example)
  - Database credentials
  - JWT secret configuration
  - Service ports
- [x] Production-ready settings
  - CORS configuration
  - Connection strings
  - Security best practices

**Deployment Options:**

1. **Docker Compose (recommended for VPS)**
   ```bash
   # Clone repository
   git clone <your-repo>
   cd recipes-app

   # Copy and configure environment
   cp .env.example .env
   nano .env  # Set JWT_SECRET and passwords

   # Build and start all services
   docker-compose up -d

   # Access application
   # Main app: http://localhost
   # Admin panel: http://localhost/admin
   # API: http://localhost/api
   ```

2. **Manual Build**
   ```bash
   # Build images
   docker build -t recipes-api ./backend/RecipesApi
   docker build -t recipes-app ./frontend/app
   docker build -t recipes-admin ./frontend/admin

   # Run with custom configuration
   docker run -d -p 5010:5010 recipes-api
   docker run -d -p 3000:3000 recipes-app
   docker run -d -p 5173:80 recipes-admin
   ```

3. **Cloud Platforms**
   - Railway: `railway up` (auto-detects Dockerfile)
   - Fly.io: `fly deploy`
   - AWS ECS/Fargate: Use GitHub Actions with AWS credentials
   - Azure Container Apps: Push to ACR and deploy
   - GCP Cloud Run: Push to GCR and deploy

**Production Checklist:**
- [x] Docker images optimized
- [x] Multi-stage builds for smaller images
- [x] Health checks configured
- [x] Logging configured
- [x] Environment variables externalized
- [ ] SSL/TLS certificates (add nginx SSL config)
- [ ] Database backups (configure pg_dump cron)
- [ ] Monitoring (add Prometheus/Grafana)
- [ ] CDN for static assets (optional)

**Files Created:**
- `backend/RecipesApi/Dockerfile`
- `frontend/app/Dockerfile`
- `frontend/admin/Dockerfile`
- `frontend/admin/nginx.conf`
- `nginx/nginx.conf` (main reverse proxy)
- `docker-compose.yml` (full stack)
- `.env.example`
- `.dockerignore`
- `.github/workflows/ci.yml`

**Result**: ✅ Production-ready containerized application with CI/CD pipeline, ready to deploy to any platform

### Phase 9: UX Improvements ✅ COMPLETE
**Goal**: Enhance user experience with search, filters, and better navigation

**Recipe Search & Filtering:**
- [x] Search by title/description (case-insensitive)
- [x] Filter by price range (minPrice, maxPrice)
- [x] Multiple sort options:
  - newest (default) - by creation date
  - popular - by view count
  - price_asc - price low to high
  - price_desc - price high to low
  - title - alphabetical
- [x] Updated GET /api/recipes endpoint with query parameters
- [x] Total count adjusted based on filters

**Backend Enhancements:**
- [x] Optimized queries with IQueryable for efficient filtering
- [x] Search applies to both title and description fields
- [x] Price range filtering with null-safe checks
- [x] Dynamic sorting with switch expression
- [x] Added requiresSubscription field to recipe response

**API Parameters:**
```
GET /api/recipes?search=chicken&minPrice=5&maxPrice=20&sortBy=popular&page=1&pageSize=20
```

**Result**: ✅ Flexible recipe search with filters - users can easily find recipes

### Phase 10: Analytics & Monitoring ✅ COMPLETE
**Goal**: Provide insights for admins and prepare for production monitoring

**Dashboard Analytics:**
- [x] Analytics endpoints created
- [x] GET /api/analytics/dashboard with comprehensive stats:
  - Overview metrics (total users, recipes, transactions, subscriptions, revenue)
  - Recent transactions (last 10)
  - Top recipes by views (top 5)
  - Timestamp for data freshness
- [x] Parallel query execution for performance
- [x] Revenue calculation from purchase transactions
- [x] Active subscriptions count

**Metrics Provided:**
- ✅ Total users count
- ✅ Total recipes count
- ✅ Total transactions count
- ✅ Active subscriptions count
- ✅ Total revenue (from purchases)
- ✅ Recent activity feed
- ✅ Most popular recipes

**Future Enhancements:**
- [ ] Time-based analytics (daily/weekly/monthly trends)
- [ ] User growth charts
- [ ] Revenue charts
- [ ] Prometheus metrics export
- [ ] Grafana dashboards
- [ ] Application logging (Serilog)
- [ ] Error tracking (Sentry)
- [ ] Performance monitoring (Application Insights)

**Result**: ✅ Basic analytics ready - admins can see key metrics and recent activity

## IDE & Tools

### Recommended IDE: VS Code

VS Code is preferred over Visual Studio for this project:
- Instant file tree updates when Claude Code creates files
- Lightweight and fast
- Excellent support for both .NET and frontend (Vue, React)
- Integrated terminal for Claude Code

### VS Code Extensions

**Required:**
```bash
code --install-extension ms-dotnettools.csdevkit    # C# Dev Kit
code --install-extension Vue.volar                   # Vue 3 / Nuxt
code --install-extension dbaeumer.vscode-eslint      # ESLint
code --install-extension esbenp.prettier-vscode     # Prettier
```

**Recommended:**
```bash
code --install-extension bradlc.vscode-tailwindcss      # Tailwind CSS
code --install-extension formulahendry.auto-rename-tag  # Auto rename HTML tags
code --install-extension PKief.material-icon-theme      # Icons
code --install-extension rangav.vscode-thunder-client   # API testing
```

### Workflow with Claude Code

```
┌─────────────────────────────────────────────┐
│  VS Code                                    │
│  ┌───────────────────────────────────────┐  │
│  │ Editor — see changes instantly        │  │
│  └───────────────────────────────────────┘  │
│  ┌───────────────────────────────────────┐  │
│  │ Terminal: claude                      │  │
│  │ > create User model with fields...    │  │
│  │ > add registration endpoint           │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```
