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

### Phase 0: Setup (1-2 days)
- [ ] Create repository (GitHub/GitLab)
- [ ] Setup folder structure
- [ ] Add CLAUDE.md
- [ ] Docker Compose for local development (SQL Server)

### Phase 1: Backend — Foundation (1-2 weeks)
**Goal**: Working API with authentication

- [ ] Initialize .NET 8/9 project (Minimal APIs)
- [ ] EF Core + data models:
  - User (id, email, passwordHash, balance)
  - Recipe (id, title, description, price, authorId)
  - Transaction (id, userId, amount, type, createdAt)
- [ ] JWT authentication (register, login, refresh tokens)
- [ ] Basic endpoints:
  - POST /auth/register, /auth/login
  - GET /recipes, GET /recipes/{id}
  - GET /users/me (profile + balance)
- [ ] Swagger for testing

**Result**: API testable via Swagger/Postman

### Phase 2: Vue Frontend — Skeleton (1-2 weeks)
**Goal**: Working personal dashboard

- [ ] Initialize Nuxt 3 project
- [ ] Setup TypeScript, ESLint, Prettier
- [ ] Authentication:
  - Pages /login, /register
  - JWT storage (useAuth composable)
  - Middleware for protected routes
- [ ] Personal dashboard:
  - /dashboard — balance, recent transactions
  - /recipes — purchased recipes list
  - /profile — profile settings
- [ ] Basic navigation and layout

**Result**: Can login and see profile

### Phase 3: Core Functionality (2-3 weeks)
**Goal**: Working points economy

Backend:
- [ ] POST /points/purchase — buy points (stub without payment)
- [ ] POST /recipes/{id}/buy — purchase recipe with points
- [ ] GET /recipes/catalog — available recipes catalog
- [ ] Balance validation, transactions

Frontend:
- [ ] /catalog — browse and buy recipes
- [ ] /recipes/{id} — recipe page
- [ ] Balance top-up component
- [ ] Transaction history
- [ ] Toast notifications for success/errors

**Result**: Full cycle — top up points → buy recipe → see in collection

### Phase 4: PWA (3-5 days)
**Goal**: Install on phone, offline access

- [ ] Connect @vite-pwa/nuxt
- [ ] Manifest (icons, name, colors)
- [ ] Service Worker — static caching
- [ ] Offline page
- [ ] Cache purchased recipes for offline viewing

**Result**: App installable, recipes available offline

### Phase 5: React Admin Panel (2-3 weeks)
**Goal**: Learn React on real task

- [ ] Initialize React + Vite + TypeScript
- [ ] Setup shadcn/ui
- [ ] Authentication (same JWT)
- [ ] TanStack Query — API connection
- [ ] Recipe list (TanStack Table):
  - Sorting, filters, pagination
  - Moderation (approve/reject)
- [ ] Recipe edit form (React Hook Form)
- [ ] Stats dashboard

**Result**: Working admin panel + React understanding

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
