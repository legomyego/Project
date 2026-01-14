# Recipes PWA - Full-Stack Application

Progressive Web App for buying, selling, and trading recipes with points economy and subscription system.

## 🚀 Quick Start

> **⚠️ Важно:** После клонирования проекта создай админа: `./create-admin.sh`
>
> Пароли хранятся в `.credentials` (не коммитится в git)

### Prerequisites
- [Docker](https://www.docker.com/get-started) and Docker Compose
- OR: [.NET 9](https://dotnet.microsoft.com/download), [Node.js 20+](https://nodejs.org/), [PostgreSQL 16](https://www.postgresql.org/download/)

### Option 1: Docker (Recommended)

```bash
# Clone the repository
git clone <your-repo-url>
cd recipes-app

# Copy environment file and configure
cp .env.example .env
# Edit .env and set JWT_SECRET and passwords

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Access the application
# Main app: http://localhost
# Admin panel: http://localhost/admin
# API: http://localhost/api
# API Swagger: http://localhost:5010/swagger
```

### Option 2: Manual Development Setup

**1. Start PostgreSQL**
```bash
docker-compose up -d db
# Or use local PostgreSQL installation
```

**2. Backend (.NET API)**
```bash
cd backend/RecipesApi

# Update connection string in appsettings.Development.json
dotnet ef database update  # Apply migrations
dotnet run                 # Starts on http://localhost:5010
```

**3. Frontend App (Nuxt)**
```bash
cd frontend/app
npm install
npm run dev  # Starts on http://localhost:3000
```

**4. Frontend Admin (React)**
```bash
cd frontend/admin
npm install
npm run dev  # Starts on http://localhost:5173
```

## 📚 Tech Stack

### Backend
- **.NET 9** - Minimal APIs
- **Entity Framework Core 9** - ORM
- **PostgreSQL 16** - Database
- **JWT Authentication** - Security

### Frontend - Main App
- **Vue 3 + Nuxt 3** - Framework
- **TypeScript** - Type safety
- **PWA** - Installable, offline-capable
- **Tailwind CSS** - Styling

### Frontend - Admin Panel
- **React 19 + Vite** - Framework
- **TypeScript** - Type safety
- **shadcn/ui** - UI components
- **TanStack Table & Query** - Data management
- **React Hook Form** - Form handling

### Infrastructure
- **Docker** - Containerization
- **Nginx** - Reverse proxy
- **GitHub Actions** - CI/CD

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────┐
│              Nginx Reverse Proxy                │
│  http://localhost                               │
└────────┬──────────────┬──────────────┬──────────┘
         │              │              │
    /api/*         /admin/*          /*
         │              │              │
    ┌────▼────┐    ┌────▼────┐   ┌────▼────┐
    │ .NET 9  │    │  React  │   │  Nuxt 3 │
    │   API   │    │  Admin  │   │   App   │
    │  :5010  │    │   :80   │   │  :3000  │
    └────┬────┘    └─────────┘   └─────────┘
         │
    ┌────▼────────┐
    │ PostgreSQL  │
    │   :5432     │
    └─────────────┘
```

## 🔑 Key Features

### User Features (Main App)
- ✅ User registration & authentication
- ✅ Points economy (top-up, purchase, trade)
- ✅ Recipe catalog with search & pagination
- ✅ Recipe purchasing with points
- ✅ Recipe trading between users
- ✅ Username system
- ✅ Subscription system (day/3-day/week plans)
- ✅ PWA support (installable, offline mode)
- ✅ Personal dashboard with balance & transactions

### Admin Features (Admin Panel)
- ✅ Admin authentication (IsAdmin flag)
- ✅ Recipe management (create, edit, delete)
- ✅ Subscription management (full CRUD)
- ✅ Recipe assignment to subscriptions
- ✅ TanStack Table with sorting & pagination
- ✅ Two-way navigation (admin ↔ main app)

### Technical Features
- ✅ JWT authentication (shared across frontends)
- ✅ Database transactions for consistency
- ✅ EF Core migrations
- ✅ CORS configuration
- ✅ Docker containerization
- ✅ Nginx reverse proxy
- ✅ GitHub Actions CI/CD
- ✅ Health check endpoints
- ✅ Gzip compression
- ✅ Security headers

## 📖 API Documentation

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/logout` - Logout user
- `GET /api/users/me` - Get current user profile

### Recipes
- `GET /api/recipes` - List all recipes (paginated)
- `GET /api/recipes/{id}` - Get recipe by ID
- `GET /api/recipes/popular` - Get popular recipes (cached)
- `GET /api/recipes/my` - Get owned recipes
- `POST /api/recipes` - Create recipe
- `PUT /api/recipes/{id}` - Update recipe (admin/author only)
- `DELETE /api/recipes/{id}` - Delete recipe (admin/author only)
- `POST /api/recipes/{id}/buy` - Purchase recipe

### Subscriptions
- `GET /api/subscriptions` - List all subscriptions
- `GET /api/subscriptions/{id}` - Get subscription details
- `POST /api/subscriptions` - Create subscription (admin only)
- `PUT /api/subscriptions/{id}` - Update subscription (admin only)
- `DELETE /api/subscriptions/{id}` - Deactivate subscription (admin only)
- `POST /api/subscriptions/{id}/buy` - Purchase subscription
- `GET /api/subscriptions/my` - Get active subscriptions
- `POST /api/subscriptions/{id}/recipes` - Add recipes (admin only)
- `DELETE /api/subscriptions/{id}/recipes/{recipeId}` - Remove recipe (admin only)

### Points & Transactions
- `POST /api/points/topup` - Add points to balance
- `GET /api/points/transactions` - Get transaction history

### Trading
- `POST /api/trades/offer` - Create trade offer
- `GET /api/trades/incoming` - Get incoming offers
- `GET /api/trades/outgoing` - Get outgoing offers
- `POST /api/trades/{id}/accept` - Accept trade
- `POST /api/trades/{id}/decline` - Decline trade
- `POST /api/trades/{id}/cancel` - Cancel trade

### Users
- `GET /api/users/search?username={username}` - Search users by username

**Swagger UI**: http://localhost:5010/swagger (development mode)

## 🛠️ Development

### Database Migrations

```bash
cd backend/RecipesApi

# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Building for Production

```bash
# Build all Docker images
docker-compose build

# Build specific service
docker-compose build api
docker-compose build app
docker-compose build admin

# Push to container registry (GitHub Container Registry)
docker tag recipes-api ghcr.io/username/recipes-api:latest
docker push ghcr.io/username/recipes-api:latest
```

### Running Tests

```bash
# Backend tests
cd backend/RecipesApi
dotnet test

# Frontend App tests
cd frontend/app
npm test

# Frontend Admin tests
cd frontend/admin
npm test
```

## 🚢 Deployment

### VPS Deployment (Docker Compose)

```bash
# On your server
git clone <repo>
cd recipes-app

# Configure environment
cp .env.example .env
nano .env  # Set production secrets

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Update application
git pull
docker-compose build
docker-compose up -d
```

### Cloud Platforms

**Railway**
```bash
railway up
```

**Fly.io**
```bash
fly deploy
```

**Manual Docker Registry**
```bash
# Tag images
docker tag recipes-api your-registry/recipes-api:latest

# Push to registry
docker push your-registry/recipes-api:latest

# Pull and run on server
docker pull your-registry/recipes-api:latest
docker run -d -p 5010:5010 your-registry/recipes-api:latest
```

## 🔒 Security

- **JWT Authentication**: HTTP-only cookies for security
- **Password Hashing**: BCrypt with salt
- **SQL Injection**: Parameterized queries via EF Core
- **XSS Protection**: Security headers configured
- **CORS**: Configured for specific origins
- **Rate Limiting**: TODO - Phase 5
- **HTTPS**: Configure nginx SSL in production

## 📝 Environment Variables

See [.env.example](.env.example) for all available options.

**Critical for Production:**
- `JWT_SECRET` - Generate with `openssl rand -base64 64`
- `POSTGRES_PASSWORD` - Strong database password
- API URLs should point to production domain

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License

This project is for educational purposes.

## 📞 Support

For questions or issues, please open an issue on GitHub.

---

**Powered by**: .NET 9, Vue 3, React 19, PostgreSQL, Docker
**Built with**: Claude Code ❤️
