// Authentication Context for React Admin Panel
// Manages user authentication state across the application
// Provides login, logout, and user data to all components

import React, { createContext, useContext, useState, useEffect } from 'react'
import { api, type User } from '@/lib/api'

/**
 * AuthContext value interface
 * Defines what data and methods are available to components
 * Note: No login method - users must login on the main app
 */
interface AuthContextType {
  user: User | null
  isLoading: boolean
  isAuthenticated: boolean
  logout: () => Promise<void>
}

// Create context with undefined initial value
// We'll provide the real value in AuthProvider
const AuthContext = createContext<AuthContextType | undefined>(undefined)

/**
 * AuthProvider component
 * Wraps the app and provides authentication state to all children
 *
 * Usage:
 * <AuthProvider>
 *   <App />
 * </AuthProvider>
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  // User state - null means not logged in, User object means logged in
  const [user, setUser] = useState<User | null>(null)

  // Loading state - true while checking authentication status
  const [isLoading, setIsLoading] = useState(true)

  // Derived state - user is authenticated if user object exists
  const isAuthenticated = user !== null

  /**
   * Load user data on mount
   * Checks if user has valid auth cookie and admin privileges
   */
  useEffect(() => {
    const loadUser = async () => {
      try {
        console.log('[Admin Auth] Starting auth check...')
        console.log('[Admin Auth] Current URL:', window.location.href)

        // Check if token is passed in URL (for SSO from main app)
        const urlParams = new URLSearchParams(window.location.search)
        const tokenFromUrl = urlParams.get('token')

        console.log('[Admin Auth] Token from URL:', tokenFromUrl ? 'YES (length: ' + tokenFromUrl.length + ')' : 'NO')

        if (tokenFromUrl) {
          console.log('[Admin Auth] Setting token as cookie...')
          // Set token as cookie for future requests
          document.cookie = `auth_token=${tokenFromUrl}; path=/; domain=.recipes.local; max-age=${7 * 24 * 60 * 60}`
          console.log('[Admin Auth] Cookie set, document.cookie:', document.cookie)

          // Remove token from URL for security
          window.history.replaceState({}, '', window.location.pathname)
          console.log('[Admin Auth] Token removed from URL')
        }

        // Try to get current user from backend
        // If auth cookie is valid, this will return user data
        console.log('[Admin Auth] Fetching user data from API...')
        const userData = await api.auth.getCurrentUser()
        console.log('[Admin Auth] User data received:', { email: userData.email, isAdmin: userData.isAdmin })

        // Only set user if they have admin privileges
        if (userData.isAdmin) {
          console.log('[Admin Auth] User is admin - setting user state')
          setUser(userData)
        } else {
          console.log('[Admin Auth] User is NOT admin - logging out')
          // User is authenticated but not an admin - log them out
          await api.auth.logout()
          setUser(null)
        }
      } catch (error) {
        console.error('[Admin Auth] Error during auth check:', error)
        // If request fails, user is not authenticated
        // This is normal - it just means no valid cookie exists
        setUser(null)
      } finally {
        console.log('[Admin Auth] Auth check complete')
        // Always stop loading after check completes
        setIsLoading(false)
      }
    }

    loadUser()
  }, [])

  /**
   * Logout function
   * Calls API to clear auth cookie and resets user state
   * After logout, redirects to main app
   */
  const logout = async () => {
    try {
      // Call logout API - this clears the auth cookie
      await api.auth.logout()
    } catch (error) {
      // Even if API call fails, clear local state
      console.error('Logout error:', error)
    } finally {
      // Always clear user state on logout
      setUser(null)
    }
  }

  // Provide authentication state and methods to all children
  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

/**
 * useAuth hook
 * Provides easy access to authentication context
 *
 * @throws Error if used outside AuthProvider
 *
 * Usage:
 * const { user, logout } = useAuth()
 */
export function useAuth() {
  const context = useContext(AuthContext)

  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }

  return context
}
