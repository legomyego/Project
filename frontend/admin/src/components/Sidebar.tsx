import { Link, useLocation } from 'react-router-dom'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  BookOpen,
  CreditCard,
  Users,
  ArrowLeftRight,
  BarChart3,
  ExternalLink,
} from 'lucide-react'

// Sidebar component for admin panel navigation
// Provides quick access to all admin pages without returning to dashboard
export function Sidebar() {
  const location = useLocation()

  // Navigation items with icons and paths
  const navItems = [
    {
      title: 'Dashboard',
      href: '/dashboard',
      icon: LayoutDashboard,
    },
    {
      title: 'Recipes',
      href: '/recipes',
      icon: BookOpen,
    },
    {
      title: 'Subscriptions',
      href: '/subscriptions',
      icon: CreditCard,
    },
    {
      title: 'Users',
      href: '/users',
      icon: Users,
    },
    {
      title: 'Trades',
      href: '/trades',
      icon: ArrowLeftRight,
    },
    {
      title: 'Analytics',
      href: '/analytics',
      icon: BarChart3,
    },
  ]

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 border-r bg-white">
      {/* Sidebar header with app name */}
      <div className="flex h-16 items-center border-b px-6">
        <h1 className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
          Recipe Admin
        </h1>
      </div>

      {/* Navigation menu */}
      <nav className="flex flex-col gap-1 p-4">
        {navItems.map((item) => {
          // Check if current path matches this nav item
          const isActive = location.pathname === item.href
          const Icon = item.icon

          return (
            <Link
              key={item.href}
              to={item.href}
              className={cn(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-indigo-50 text-indigo-600'
                  : 'text-gray-700 hover:bg-gray-100'
              )}
            >
              <Icon className="h-5 w-5" />
              {item.title}
            </Link>
          )
        })}
      </nav>

      {/* Link to main app at the bottom */}
      <div className="absolute bottom-0 left-0 right-0 border-t p-4">
        <a
          href="http://recipes.local"
          className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 transition-colors"
        >
          <ExternalLink className="h-5 w-5" />
          Go to Main App
        </a>
      </div>
    </aside>
  )
}
