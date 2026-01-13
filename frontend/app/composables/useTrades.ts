// Trades composable for managing recipe trades between users
// Provides methods for offering, accepting, declining, and canceling trades

interface Trade {
  id: string
  offeringUser?: {
    id: string
    email: string
  }
  requestedUser?: {
    id: string
    email: string
  }
  offeredRecipe: {
    id: string
    title: string
    price: number
  }
  requestedRecipe: {
    id: string
    title: string
    price: number
  }
  status: string
  createdAt: string
  updatedAt?: string
}

interface TradesResponse {
  trades: Trade[]
  pagination: {
    currentPage: number
    pageSize: number
    totalCount: number
    totalPages: number
  }
}

export const useTrades = () => {
  // Get API base URL from runtime config
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBaseUrl || 'http://localhost:5010'

  /**
   * Search for a user by username and get their owned recipes
   * Used for finding trading partners
   */
  const searchUserByUsername = async (username: string) => {
    try {
      const response = await $fetch<any>(`${apiBase}/api/users/search`, {
        method: 'GET',
        params: { username },
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to find user'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Create a trade offer
   * Offer one of your recipes in exchange for another user's recipe
   */
  const createTradeOffer = async (
    offeredRecipeId: string,
    requestedUserId: string,
    requestedRecipeId: string
  ) => {
    try {
      const response = await $fetch<{
        message: string
        trade: Trade
      }>(`${apiBase}/api/trades/offer`, {
        method: 'POST',
        body: {
          offeredRecipeId,
          requestedUserId,
          requestedRecipeId,
        },
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to create trade offer'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Get incoming trade offers (offers you received from others)
   * These are offers where someone wants to trade for your recipe
   */
  const getIncomingTrades = async (page: number = 1, pageSize: number = 20) => {
    try {
      const response = await $fetch<TradesResponse>(
        `${apiBase}/api/trades/incoming?page=${page}&pageSize=${pageSize}`,
        {
          credentials: 'include',
        }
      )

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch incoming trades'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Get outgoing trade offers (offers you made to others)
   * These are offers where you want to trade for someone else's recipe
   */
  const getOutgoingTrades = async (page: number = 1, pageSize: number = 20) => {
    try {
      const response = await $fetch<TradesResponse>(
        `${apiBase}/api/trades/outgoing?page=${page}&pageSize=${pageSize}`,
        {
          credentials: 'include',
        }
      )

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch outgoing trades'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Accept a trade offer
   * Exchange recipes with the offering user
   */
  const acceptTrade = async (tradeId: string) => {
    try {
      const response = await $fetch<{
        message: string
        trade: {
          id: string
          offeredRecipe: { id: string; title: string }
          requestedRecipe: { id: string; title: string }
          status: string
          updatedAt: string
        }
      }>(`${apiBase}/api/trades/${tradeId}/accept`, {
        method: 'POST',
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to accept trade'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Decline a trade offer
   * Reject an offer made to you
   */
  const declineTrade = async (tradeId: string) => {
    try {
      const response = await $fetch<{
        message: string
        tradeId: string
        status: string
      }>(`${apiBase}/api/trades/${tradeId}/decline`, {
        method: 'POST',
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to decline trade'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Cancel a trade offer
   * Cancel an offer you made to someone else
   */
  const cancelTrade = async (tradeId: string) => {
    try {
      const response = await $fetch<{
        message: string
        tradeId: string
        status: string
      }>(`${apiBase}/api/trades/${tradeId}/cancel`, {
        method: 'POST',
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to cancel trade'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Get user's owned recipes (for creating trade offers)
   * Returns recipes the user can offer in trades
   */
  const getMyRecipes = async (page: number = 1, pageSize: number = 50) => {
    try {
      const response = await $fetch<{
        recipes: Array<{
          recipe: {
            id: string
            title: string
            description: string
            price: number
            views: number
            createdAt: string
            author: {
              id: string
              email: string
            }
          }
          acquiredAt: string
          acquisitionType: string
        }>
        pagination: {
          currentPage: number
          pageSize: number
          totalCount: number
          totalPages: number
        }
      }>(`${apiBase}/api/recipes/my?page=${page}&pageSize=${pageSize}`, {
        credentials: 'include',
      })

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch your recipes'
      return { success: false, error: errorMessage }
    }
  }

  return {
    searchUserByUsername,
    createTradeOffer,
    getIncomingTrades,
    getOutgoingTrades,
    acceptTrade,
    declineTrade,
    cancelTrade,
    getMyRecipes,
  }
}
