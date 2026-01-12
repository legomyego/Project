// Points composable for managing user points and transactions
// Provides methods for top-up and viewing transaction history

interface Transaction {
  id: string
  amount: number
  type: string
  createdAt: string
  recipe: {
    id: string
    title: string
    price: number
  } | null
}

interface TransactionsResponse {
  transactions: Transaction[]
  pagination: {
    currentPage: number
    pageSize: number
    totalCount: number
    totalPages: number
  }
}

export const usePoints = () => {
  // Get API base URL from runtime config
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBaseUrl || 'http://localhost:5010'

  // Get auth composable to update user balance
  const { user, fetchUser } = useAuth()

  /**
   * Top up user's points balance
   * In production, this would integrate with a payment provider
   */
  const topUp = async (amount: number) => {
    try {
      const response = await $fetch<{
        message: string
        amount: number
        newBalance: number
        transactionId: string
      }>(`${apiBase}/api/points/topup`, {
        method: 'POST',
        body: { amount },
        credentials: 'include',
      })

      // Update user balance in local state
      await fetchUser()

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Top-up failed'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Fetch user's transaction history
   */
  const getTransactions = async (page: number = 1, pageSize: number = 20) => {
    try {
      const response = await $fetch<TransactionsResponse>(
        `${apiBase}/api/points/transactions?page=${page}&pageSize=${pageSize}`,
        {
          credentials: 'include',
        }
      )

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch transactions'
      return { success: false, error: errorMessage }
    }
  }

  return {
    topUp,
    getTransactions,
  }
}
