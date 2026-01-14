// Authentication composable for managing user auth state
// Provides reactive user data and authentication methods
// State is shared across all components using this composable

// User type definition
interface User {
  id: string
  email: string
  username: string
  balance: number
  isAdmin: boolean
  createdAt: string
}

// Login request payload
interface LoginRequest {
  email: string
  password: string
}

// Registration request payload
interface RegisterRequest {
  email: string
  username: string
  password: string
}

// API response for successful auth
interface AuthResponse {
  id: string
  email: string
  username: string
  balance: number
  isAdmin: boolean
  createdAt: string
  message: string
}

export const useAuth = () => {
  // useState creates a shared reactive state across the app
  // The 'user' key ensures the same state is used everywhere
  const user = useState<User | null>('user', () => null)

  // Track loading state for auth operations
  const isLoading = useState<boolean>('auth-loading', () => false)

  // Get API base URL from runtime config
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBaseUrl || 'http://localhost:5010'

  /**
   * Register a new user account
   * Creates user and automatically logs them in
   */
  const register = async (email: string, username: string, password: string) => {
    isLoading.value = true

    try {
      // $fetch is Nuxt's universal fetch that works on both server and client
      // credentials: 'include' ensures cookies are sent/received
      const response = await $fetch<AuthResponse>(`${apiBase}/api/auth/register`, {
        method: 'POST',
        body: { email, username, password },
        credentials: 'include', // Important: allows cookies to be set
      })

      // Store user data in reactive state
      user.value = {
        id: response.id,
        email: response.email,
        username: response.username,
        balance: response.balance,
        isAdmin: response.isAdmin,
        createdAt: response.createdAt,
      }

      return { success: true, user: user.value }
    } catch (error: any) {
      // Handle different error types
      const errorMessage = error.data?.error || error.message || 'Registration failed'
      return { success: false, error: errorMessage }
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Login with existing credentials
   * Sets JWT token in httpOnly cookie
   */
  const login = async (email: string, password: string) => {
    isLoading.value = true

    try {
      const response = await $fetch<AuthResponse>(`${apiBase}/api/auth/login`, {
        method: 'POST',
        body: { email, password },
        credentials: 'include', // Allows cookies to be set
      })

      // Store user data
      user.value = {
        id: response.id,
        email: response.email,
        username: response.username,
        balance: response.balance,
        isAdmin: response.isAdmin,
        createdAt: response.createdAt,
      }

      return { success: true, user: user.value }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Login failed'
      return { success: false, error: errorMessage }
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Logout current user
   * Clears cookie and local state
   */
  const logout = async () => {
    try {
      await $fetch(`${apiBase}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      })

      // Clear user state
      user.value = null

      return { success: true }
    } catch (error: any) {
      // Even if API call fails, clear local state
      user.value = null
      return { success: true }
    }
  }

  /**
   * Fetch current user profile
   * Requires valid JWT token in cookie
   */
  const fetchUser = async () => {
    // Prevent multiple simultaneous fetches
    if (isLoading.value) {
      return { success: false }
    }

    isLoading.value = true

    try {
      const response = await $fetch<User>(`${apiBase}/api/users/me`, {
        credentials: 'include',
      })

      user.value = response
      return { success: true, user: response }
    } catch (error: any) {
      // Token invalid or expired - clear user
      user.value = null
      return { success: false }
    } finally {
      isLoading.value = false
    }
  }

  // Computed property to check if user is authenticated
  const isAuthenticated = computed(() => user.value !== null)

  return {
    user: readonly(user), // Make user read-only outside this composable
    isLoading: readonly(isLoading),
    isAuthenticated,
    register,
    login,
    logout,
    fetchUser,
  }
}
