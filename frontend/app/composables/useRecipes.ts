// Recipes composable for browsing and purchasing recipes
// Provides methods for listing recipes and buying them with points

interface Recipe {
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

interface RecipesResponse {
  recipes: Recipe[]
  pagination: {
    currentPage: number
    pageSize: number
    totalCount: number
    totalPages: number
  }
}

export const useRecipes = () => {
  // Get API base URL from runtime config
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBaseUrl || 'http://localhost:5010'

  // Get auth composable to update user balance after purchase
  const { fetchUser } = useAuth()

  /**
   * Fetch list of all recipes with pagination
   */
  const getRecipes = async (page: number = 1, pageSize: number = 50) => {
    try {
      const response = await $fetch<RecipesResponse>(
        `${apiBase}/api/recipes?page=${page}&pageSize=${pageSize}`
      )

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch recipes'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Fetch a single recipe by ID
   */
  const getRecipeById = async (id: string) => {
    try {
      const response = await $fetch<Recipe>(`${apiBase}/api/recipes/${id}`)

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Failed to fetch recipe'
      return { success: false, error: errorMessage }
    }
  }

  /**
   * Purchase a recipe with points
   * Requires authentication
   */
  const buyRecipe = async (id: string) => {
    try {
      const response = await $fetch<{
        message: string
        recipe: {
          id: string
          title: string
          price: number
        }
        newBalance: number
        transactionId: string
      }>(`${apiBase}/api/recipes/${id}/buy`, {
        method: 'POST',
        credentials: 'include',
      })

      // Update user balance in local state
      await fetchUser()

      return { success: true, data: response }
    } catch (error: any) {
      const errorMessage = error.data?.error || error.message || 'Purchase failed'
      return { success: false, error: errorMessage }
    }
  }

  return {
    getRecipes,
    getRecipeById,
    buyRecipe,
  }
}
