// API client for making requests to the backend
// Handles authentication cookies and provides typed API methods

import Cookies from 'js-cookie'

// Base API URL - same backend as Nuxt app
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5010'

/**
 * Type definitions for API requests and responses
 */

// Authentication
export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  username: string
  password: string
}

export interface AuthResponse {
  id: string
  email: string
  username: string
  balance: number
  createdAt: string
  message: string
}

export interface User {
  id: string
  email: string
  username: string
  balance: number
  createdAt: string
}

// Recipes
export interface Recipe {
  id: string
  title: string
  description: string
  price: number
  views: number
  createdAt: string
  author: {
    id: string
    username: string
  }
}

export interface RecipesResponse {
  recipes: Recipe[]
  pagination: {
    currentPage: number
    totalPages: number
    pageSize: number
    totalCount: number
  }
}

// Trades
export interface Trade {
  id: string
  status: string
  createdAt: string
  offeringUser: {
    id: string
    username: string
  }
  offeredRecipe: {
    id: string
    title: string
    price: number
  }
  requestedUser: {
    id: string
    username: string
  }
  requestedRecipe: {
    id: string
    title: string
    price: number
  }
}

/**
 * Generic API request helper
 * Automatically includes credentials for cookie-based auth
 */
async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    credentials: 'include', // Include cookies in requests
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  // Check if response is OK (status 200-299)
  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: 'Request failed' }))
    throw new Error(error.error || `HTTP ${response.status}`)
  }

  return response.json()
}

/**
 * API client object with all API methods
 */
export const api = {
  // Authentication endpoints
  auth: {
    /**
     * Login with email and password
     * Sets auth cookie on success
     */
    login: async (data: LoginRequest): Promise<AuthResponse> => {
      return apiRequest<AuthResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(data),
      })
    },

    /**
     * Register new user account
     * Sets auth cookie on success
     */
    register: async (data: RegisterRequest): Promise<AuthResponse> => {
      return apiRequest<AuthResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      })
    },

    /**
     * Logout current user
     * Clears auth cookie
     */
    logout: async (): Promise<{ message: string }> => {
      const result = await apiRequest<{ message: string }>('/api/auth/logout', {
        method: 'POST',
      })
      // Clear cookie on client side as well
      Cookies.remove('auth_token')
      return result
    },

    /**
     * Get current authenticated user profile
     */
    getCurrentUser: async (): Promise<User> => {
      return apiRequest<User>('/api/users/me')
    },
  },

  // Recipe endpoints
  recipes: {
    /**
     * Get all recipes with pagination
     */
    getAll: async (page: number = 1, pageSize: number = 20): Promise<RecipesResponse> => {
      return apiRequest<RecipesResponse>(`/api/recipes?page=${page}&pageSize=${pageSize}`)
    },

    /**
     * Get single recipe by ID
     */
    getById: async (id: string): Promise<Recipe> => {
      return apiRequest<Recipe>(`/api/recipes/${id}`)
    },

    /**
     * Get popular recipes (cached)
     */
    getPopular: async (): Promise<Recipe[]> => {
      return apiRequest<Recipe[]>('/api/recipes/popular')
    },

    /**
     * Get user's owned recipes
     */
    getMyRecipes: async (): Promise<{ recipes: any[], pagination: any }> => {
      return apiRequest('/api/recipes/my')
    },
  },

  // Trade endpoints
  trades: {
    /**
     * Get incoming trade offers
     */
    getIncoming: async (): Promise<Trade[]> => {
      return apiRequest<Trade[]>('/api/trades/incoming')
    },

    /**
     * Get outgoing trade offers
     */
    getOutgoing: async (): Promise<Trade[]> => {
      return apiRequest<Trade[]>('/api/trades/outgoing')
    },

    /**
     * Create new trade offer
     */
    create: async (offeredRecipeId: string, requestedUserId: string, requestedRecipeId: string) => {
      return apiRequest('/api/trades/offer', {
        method: 'POST',
        body: JSON.stringify({
          offeredRecipeId,
          requestedUserId,
          requestedRecipeId,
        }),
      })
    },

    /**
     * Accept trade offer
     */
    accept: async (tradeId: string) => {
      return apiRequest(`/api/trades/${tradeId}/accept`, {
        method: 'POST',
      })
    },

    /**
     * Decline trade offer
     */
    decline: async (tradeId: string) => {
      return apiRequest(`/api/trades/${tradeId}/decline`, {
        method: 'POST',
      })
    },

    /**
     * Cancel outgoing trade offer
     */
    cancel: async (tradeId: string) => {
      return apiRequest(`/api/trades/${tradeId}/cancel`, {
        method: 'POST',
      })
    },
  },

  // Points/Transactions endpoints
  points: {
    /**
     * Top up user points balance
     */
    topup: async (amount: number) => {
      return apiRequest('/api/points/topup', {
        method: 'POST',
        body: JSON.stringify({ amount }),
      })
    },

    /**
     * Get transaction history
     */
    getTransactions: async (page: number = 1, pageSize: number = 10) => {
      return apiRequest(`/api/points/transactions?page=${page}&pageSize=${pageSize}`)
    },
  },
}
