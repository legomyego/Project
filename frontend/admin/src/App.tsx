// Main App component for the React Admin Panel
// Handles routing and authentication-based navigation

import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './contexts/AuthContext'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { RecipesPage } from './pages/RecipesPage'
import { UsersPage } from './pages/UsersPage'
import { TradesPage } from './pages/TradesPage'
import { AnalyticsPage } from './pages/AnalyticsPage'

/**
 * ProtectedRoute component
 * Redirects to login if user is not authenticated
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

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
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
      {/* Public route - Login */}
      <Route path="/login" element={<LoginPage />} />

      {/* Protected routes - require authentication */}
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
