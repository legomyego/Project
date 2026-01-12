# Recipe PWA - Points Economy Application

Modern PWA application where users earn, buy, and exchange points to purchase recipes. Built with .NET 9, Vue 3/Nuxt 3, and React microfrontend architecture.

## Tech Stack

### Backend
- **.NET 9** with Minimal APIs
- **Entity Framework Core 9** with PostgreSQL
- **JWT Authentication** (httpOnly cookies)
- Memory caching + Output caching

### Frontend - Main App
- **Nuxt 3 + Vue 3** with TypeScript
- **PWA** support via `@vite-pwa/nuxt`
- Service Worker for offline capabilities

### Frontend - Admin Panel
- **React 18 + Vite** with TypeScript
- **shadcn/ui** components
- **TanStack Query** for data fetching
- **TanStack Table** for data tables
- **React Hook Form** for forms

## Prerequisites

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop/)

## Project Structure

```
/
├── backend/
│   └── RecipesApi/          # .NET 9 API
├── frontend/
│   ├── app/                 # Nuxt 3 main application
│   └── admin/               # React admin panel
├── docker-compose.yml       # PostgreSQL setup
└── CLAUDE.md               # Detailed project documentation
```

## Getting Started

### 1. Start Database

```bash
# Start PostgreSQL container
docker-compose up -d

# Verify container is running
docker ps
```

### 2. Run Backend API

```bash
cd backend/RecipesApi
dotnet restore
dotnet run

# API will be available at:
# http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### 3. Run Nuxt App (Main Frontend)

```bash
cd frontend/app
npm install
npm run dev

# App will be available at:
# http://localhost:3000
```

### 4. Run React Admin Panel

```bash
cd frontend/admin
npm install
npm run dev

# Admin panel will be available at:
# http://localhost:5173
```

## Database Connection

**Connection String (Development):**
```
Host=localhost;Port=5432;Database=recipes_db;Username=recipes_user;Password=dev_password
```

**Credentials:**
- Host: `localhost:5432`
- Database: `recipes_db`
- User: `recipes_user`
- Password: `dev_password` ⚠️ Development only!

## Authentication

Authentication uses **JWT tokens stored in httpOnly cookies** for security:

- Tokens are automatically sent with requests
- Protected against XSS attacks
- 60-minute expiration (configurable)
- CORS configured to allow credentials

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login (returns httpOnly cookie)
- `POST /api/auth/logout` - Logout (clears cookie)

### Users
- `GET /api/users/me` - Get current user profile + balance

### Recipes
- `GET /api/recipes` - List all recipes
- `GET /api/recipes/{id}` - Get recipe details

## Development Workflow

### Database Migrations

Migrations are applied **automatically** when the API starts. To create new migrations:

```bash
cd backend/RecipesApi
dotnet ef migrations add MigrationName
# Restart API to apply
```

### Code Style

- Detailed **English comments** explaining what code does and why
- Follow conventions in [CLAUDE.md](./CLAUDE.md)

## Environment Variables

### Backend
Configure in `backend/RecipesApi/appsettings.Development.json` (not committed):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=recipes_db;Username=recipes_user;Password=dev_password"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-min-32-characters",
    "Issuer": "RecipesAPI",
    "ExpirationMinutes": 60
  }
}
```

### Nuxt App
Create `frontend/app/.env.development`:

```env
NUXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

### React Admin
Create `frontend/admin/.env.development.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

## Troubleshooting

### PostgreSQL Connection Failed

```bash
# Check if container is running
docker ps

# View container logs
docker logs recipes-postgres

# Restart container
docker-compose restart postgres
```

### .NET Build Errors

```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Node.js Module Errors

```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

## Project Documentation

For detailed architecture, phases, and learning goals, see [CLAUDE.md](./CLAUDE.md).

## License

MIT
