// Authentication Context for React Admin Panel
// Manages user authentication state across the application
// Provides login, logout, and user data to all components

import React, { createContext, useContext, useState, useEffect } from 'react'
import { api, type User } from '@/lib/api'

/**
 * AuthContext value interface
 * Defines what data and methods are available to components
 */
interface AuthContextType {
  user: User | null
  isLoading: boolean
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
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
        // Try to get current user from backend
        // If auth cookie is valid, this will return user data
        const userData = await api.auth.getCurrentUser()

        // Only set user if they have admin privileges
        if (userData.isAdmin) {
          setUser(userData)
        } else {
          // User is authenticated but not an admin - log them out
          await api.auth.logout()
          setUser(null)
        }
      } catch (error) {
        // If request fails, user is not authenticated
        // This is normal - it just means no valid cookie exists
        setUser(null)
      } finally {
        // Always stop loading after check completes
        setIsLoading(false)
      }
    }

    loadUser()
  }, [])

  /**
   * Login function
   * Calls API to authenticate and stores user data
   * Only allows login if user has admin privileges
   *
   * @throws Error if login fails or user is not an admin
   */
  const login = async (email: string, password: string) => {
    try {
      // Call login API - this sets the auth cookie
      const response = await api.auth.login({ email, password })

      // Check if user is an admin
      if (!response.isAdmin) {
        // User is not an admin - clear cookie and reject
        await api.auth.logout()
        throw new Error('Access denied. Admin privileges required.')
      }

      // Store user data in state
      setUser({
        id: response.id,
        email: response.email,
        username: response.username,
        balance: response.balance,
        isAdmin: response.isAdmin,
        createdAt: response.createdAt,
      })
    } catch (error) {
      // Re-throw error so calling component can handle it
      throw error
    }
  }

  /**
   * Logout function
   * Calls API to clear auth cookie and resets user state
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
    login,
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
 * const { user, login, logout } = useAuth()
 */
export function useAuth() {
  const context = useContext(AuthContext)

  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }

  return context
}
