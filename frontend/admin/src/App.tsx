// Main App component for the React Admin Panel
// Handles routing and authentication-based navigation

import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './contexts/AuthContext'
import { DashboardPage } from './pages/DashboardPage'
import { RecipesPage } from './pages/RecipesPage'
import { SubscriptionsPage } from './pages/SubscriptionsPage'
import { SubscriptionDetailPage } from './pages/SubscriptionDetailPage'
import { UsersPage } from './pages/UsersPage'
import { TradesPage } from './pages/TradesPage'
import { AnalyticsPage } from './pages/AnalyticsPage'

/**
 * ProtectedRoute component
 * Redirects to main app login if user is not authenticated or not admin
 */
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth()

  // Show loading while checking auth status
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-muted-foreground">Loading...</div>
      </div>
    )
  }

  // Redirect to main app login if not authenticated
  // User must login on the main app, then return to admin panel
  if (!isAuthenticated) {
    const mainAppUrl = import.meta.env.VITE_MAIN_APP_URL || 'http://localhost:3000'
    const adminUrl = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://admin.recipes.local'
    // Add redirect parameter so user returns to admin after login
    window.location.href = `${mainAppUrl}/login?redirect=${encodeURIComponent(adminUrl)}`
    return null
  }

  // Render children if authenticated
  return <>{children}</>
}

/**
 * App component - main router
 * Defines all routes and their authentication requirements
 */
function App() {
  return (
    <Routes>
      {/* All routes are protected - no separate login page */}
      {/* Users must login on the main app (recipes.local) first */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/recipes"
        element={
          <ProtectedRoute>
            <RecipesPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscriptions"
        element={
          <ProtectedRoute>
            <SubscriptionsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscriptions/:id"
        element={
          <ProtectedRoute>
            <SubscriptionDetailPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ProtectedRoute>
            <UsersPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/trades"
        element={
          <ProtectedRoute>
            <TradesPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/analytics"
        element={
          <ProtectedRoute>
            <AnalyticsPage />
          </ProtectedRoute>
        }
      />

      {/* Default route - redirect to dashboard */}
      <Route path="/" element={<Navigate to="/dashboard" replace />} />

      {/* 404 - redirect to dashboard */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}

export default App
