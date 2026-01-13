# Recipe PWA - Admin Panel

React admin panel for managing recipes, users, and viewing analytics.

## Tech Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TanStack Query** - Data fetching and caching
- **TanStack Table** - Advanced table functionality
- **React Router** - Navigation
- **React Hook Form** - Form management
- **shadcn/ui** - UI component library
- **Tailwind CSS** - Styling

## Features

✅ **Authentication** - JWT-based auth (shared with Nuxt app)
✅ **Recipe Management** - View all recipes with sorting and pagination
✅ **Dashboard** - Overview with statistics and quick navigation
🚧 **User Management** - Coming soon
🚧 **Trade Monitoring** - Coming soon
🚧 **Analytics** - Coming soon

## Getting Started

### Prerequisites

- Node.js 18+
- Backend API running on http://localhost:5010

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The admin panel will be available at http://localhost:5173

### Login

Use your Recipe PWA account credentials:
- Email: testuser@example.com
- Password: password123

Or create a new account via the main Nuxt app.

## Project Structure

```
src/
├── components/
│   └── ui/           # shadcn/ui components
├── contexts/
│   └── AuthContext.tsx   # Authentication state management
├── lib/
│   ├── api.ts        # API client
│   └── utils.ts      # Utility functions
├── pages/
│   ├── LoginPage.tsx
│   ├── DashboardPage.tsx
│   ├── RecipesPage.tsx
│   ├── UsersPage.tsx
│   ├── TradesPage.tsx
│   └── AnalyticsPage.tsx
├── App.tsx           # Router and protected routes
└── main.tsx          # App entry point with providers
```

## Key Concepts

### TanStack Query

Used for server state management:
- Automatic caching and background refetching
- Loading and error states
- Pagination support

```typescript
const { data, isLoading, error } = useQuery({
  queryKey: ['recipes', currentPage],
  queryFn: () => api.recipes.getAll(currentPage, pageSize),
  staleTime: 5 * 60 * 1000, // 5 minutes
})
```

### TanStack Table

Advanced table with sorting, filtering, and pagination:

```typescript
const table = useReactTable({
  data: data?.recipes || [],
  columns,
  getCoreRowModel: getCoreRowModel(),
  getSortedRowModel: getSortedRowModel(),
  // ...
})
```

### Authentication

- JWT stored in httpOnly cookies
- AuthContext provides user state globally
- Protected routes redirect to login if not authenticated

## Development

### Build for Production

```bash
npm run build
```

### Lint

```bash
npm run lint
```

### Preview Production Build

```bash
npm run preview
```

## API Integration

The admin panel connects to the same .NET backend as the Nuxt app:

- Base URL: `http://localhost:5010`
- Authentication: JWT in httpOnly cookies
- All API calls include `credentials: 'include'`

## Next Steps

Phase 7 development roadmap:

1. ✅ Basic setup and authentication
2. ✅ Recipe management table
3. 🚧 Recipe edit form (React Hook Form)
4. 🚧 User management table
5. 🚧 Analytics dashboard with charts
6. 🚧 Trade moderation features

## Learning Resources

- [React Docs](https://react.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
- [TanStack Table](https://tanstack.com/table/latest)
- [shadcn/ui](https://ui.shadcn.com/)
- [Tailwind CSS](https://tailwindcss.com/)
