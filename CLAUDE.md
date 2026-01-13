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

### Phase 5: Security & Account Management 📋 PLANNED
**Goal**: Production-ready security and user management

Backend:
- [ ] Rate Limiting for DDoS protection:
  - ASP.NET built-in middleware
  - Configure limits per endpoint
  - IP-based throttling
- [ ] Password management:
  - POST /api/users/change-password — change password for authenticated user
  - POST /api/auth/forgot-password — send reset email
  - POST /api/auth/reset-password — reset with token
  - Email service (SMTP configuration)
- [ ] CAPTCHA integration:
  - Google reCAPTCHA v3 (invisible)
  - Verify on registration and login
  - Configurable threshold

Frontend:
- [ ] Change password form in dashboard/profile
- [ ] Forgot password flow on login page
- [ ] Reset password page with token validation
- [ ] CAPTCHA integration (invisible to users)

**Result**: Secure, production-ready authentication system

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

### Phase 6: Integration & Deploy (1 week)
- [ ] Nginx config (reverse proxy)
- [ ] Docker images for all services
- [ ] CI/CD (GitHub Actions)
- [ ] Deploy to VPS / Railway / Fly.io

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
