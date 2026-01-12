<template>
  <div class="recipes-page">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1>Recipe Catalog</h1>
        <NuxtLink to="/dashboard" class="btn-back">← Dashboard</NuxtLink>
      </div>
    </header>

    <!-- Main content -->
    <div class="content">
      <!-- Loading state -->
      <div v-if="isLoading" class="loading">
        <p>Loading recipes...</p>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="error-card">
        <p>{{ error }}</p>
        <button @click="loadRecipes" class="btn-retry">Try Again</button>
      </div>

      <!-- Recipes grid -->
      <div v-else>
        <div v-if="recipes.length === 0" class="empty-state">
          <p>No recipes available yet</p>
        </div>

        <div v-else class="recipes-grid">
          <div
            v-for="recipe in recipes"
            :key="recipe.id"
            class="recipe-card"
          >
            <!-- Recipe info -->
            <div class="recipe-header">
              <h3>{{ recipe.title }}</h3>
              <div class="recipe-meta">
                <span class="views">👁️ {{ recipe.views }} views</span>
                <span class="author">By {{ recipe.author.email }}</span>
              </div>
            </div>

            <p class="recipe-description">
              {{ recipe.description || 'No description available' }}
            </p>

            <div class="recipe-footer">
              <div class="price">{{ recipe.price }} points</div>
              <button
                @click="handleBuyRecipe(recipe)"
                class="btn-buy"
                :disabled="isBuying === recipe.id || isOwnRecipe(recipe)"
              >
                <span v-if="isBuying === recipe.id">Buying...</span>
                <span v-else-if="isOwnRecipe(recipe)">Your Recipe</span>
                <span v-else>Buy Recipe</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="pagination && pagination.totalPages > 1" class="pagination">
          <button
            @click="changePage(pagination.currentPage - 1)"
            :disabled="pagination.currentPage === 1"
            class="btn-page"
          >
            ← Previous
          </button>

          <span class="page-info">
            Page {{ pagination.currentPage }} of {{ pagination.totalPages }}
          </span>

          <button
            @click="changePage(pagination.currentPage + 1)"
            :disabled="pagination.currentPage === pagination.totalPages"
            class="btn-page"
          >
            Next →
          </button>
        </div>
      </div>
    </div>

    <!-- Purchase result modal (simple version) -->
    <div v-if="purchaseResult" class="modal-overlay" @click="purchaseResult = null">
      <div class="modal" @click.stop>
        <div v-if="purchaseResult.success" class="success-modal">
          <div class="modal-icon">✅</div>
          <h3>Purchase Successful!</h3>
          <p>You purchased <strong>{{ purchaseResult.recipeName }}</strong></p>
          <p>New balance: <strong>{{ purchaseResult.newBalance }} points</strong></p>
          <button @click="purchaseResult = null" class="btn-close">Close</button>
        </div>

        <div v-else class="error-modal">
          <div class="modal-icon">❌</div>
          <h3>Purchase Failed</h3>
          <p>{{ purchaseResult.error }}</p>
          <button @click="purchaseResult = null" class="btn-close">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Recipes catalog page - browse and purchase recipes

definePageMeta({
  middleware: 'auth'
})

// Use composables
const { user } = useAuth()
const { getRecipes, buyRecipe } = useRecipes()

// State
const recipes = ref<any[]>([])
const pagination = ref<any>(null)
const isLoading = ref(false)
const error = ref('')
const isBuying = ref<string | null>(null)
const purchaseResult = ref<any>(null)

// Load recipes on mount
onMounted(() => {
  loadRecipes()
})

/**
 * Load recipes from API
 */
const loadRecipes = async (page: number = 1) => {
  isLoading.value = true
  error.value = ''

  const result = await getRecipes(page, 12) // 12 recipes per page

  if (result.success && result.data) {
    recipes.value = result.data.recipes
    pagination.value = result.data.pagination
  } else {
    error.value = result.error || 'Failed to load recipes'
  }

  isLoading.value = false
}

/**
 * Change page
 */
const changePage = (page: number) => {
  loadRecipes(page)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

/**
 * Check if recipe belongs to current user
 */
const isOwnRecipe = (recipe: any) => {
  return user.value && recipe.author.id === user.value.id
}

/**
 * Handle recipe purchase
 */
const handleBuyRecipe = async (recipe: any) => {
  if (!user.value) return

  // Check if user has enough balance
  if (user.value.balance < recipe.price) {
    purchaseResult.value = {
      success: false,
      error: `Insufficient balance. You need ${recipe.price - user.value.balance} more points.`
    }
    return
  }

  isBuying.value = recipe.id

  const result = await buyRecipe(recipe.id)

  if (result.success && result.data) {
    purchaseResult.value = {
      success: true,
      recipeName: recipe.title,
      newBalance: result.data.newBalance
    }
  } else {
    purchaseResult.value = {
      success: false,
      error: result.error || 'Purchase failed'
    }
  }

  isBuying.value = null
}
</script>

<style scoped>
.recipes-page {
  min-height: 100vh;
  background: #f5f7fa;
}

/* Header */
.page-header {
  background: white;
  padding: 1.5rem 2rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-header h1 {
  margin: 0;
  font-size: 1.75rem;
  color: #333;
}

.btn-back {
  padding: 0.5rem 1rem;
  background: #667eea;
  color: white;
  text-decoration: none;
  border-radius: 0.5rem;
  font-weight: 600;
  transition: background 0.2s;
}

.btn-back:hover {
  background: #5568d3;
}

/* Content */
.content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

/* Loading and error states */
.loading,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #666;
}

.error-card {
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  text-align: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.error-card p {
  color: #c33;
  margin-bottom: 1rem;
}

.btn-retry {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
}

/* Recipes grid */
.recipes-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

/* Recipe card */
.recipe-card {
  background: white;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  transition: transform 0.2s;
}

.recipe-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 12px rgba(0, 0, 0, 0.15);
}

.recipe-header h3 {
  margin: 0 0 0.5rem 0;
  color: #333;
  font-size: 1.25rem;
}

.recipe-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.85rem;
  color: #666;
  margin-bottom: 1rem;
}

.recipe-description {
  flex: 1;
  color: #666;
  line-height: 1.5;
  margin: 0 0 1rem 0;
}

.recipe-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 1rem;
  border-top: 1px solid #e0e0e0;
}

.price {
  font-size: 1.25rem;
  font-weight: 700;
  color: #667eea;
}

.btn-buy {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  transition: transform 0.2s;
}

.btn-buy:hover:not(:disabled) {
  transform: translateY(-2px);
}

.btn-buy:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background: #999;
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 2rem;
}

.btn-page {
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.btn-page:hover:not(:disabled) {
  background: #667eea;
  color: white;
  border-color: #667eea;
}

.btn-page:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  color: #666;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  max-width: 400px;
  width: 90%;
  text-align: center;
}

.modal-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.success-modal h3 {
  color: #28a745;
  margin: 0 0 1rem 0;
}

.error-modal h3 {
  color: #dc3545;
  margin: 0 0 1rem 0;
}

.modal p {
  margin: 0.5rem 0;
  color: #666;
}

.btn-close {
  margin-top: 1.5rem;
  padding: 0.75rem 2rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
}

/* Responsive */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    gap: 1rem;
  }

  .recipes-grid {
    grid-template-columns: 1fr;
  }

  .content {
    padding: 1rem;
  }
}
</style>
