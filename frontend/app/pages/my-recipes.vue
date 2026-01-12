<template>
  <div class="my-recipes-page">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1>My Recipes</h1>
        <NuxtLink to="/dashboard" class="btn-back">← Dashboard</NuxtLink>
      </div>
    </header>

    <!-- Main content -->
    <div class="content">
      <!-- Loading state -->
      <div v-if="isLoading" class="loading">
        <p>Loading your recipes...</p>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="error-card">
        <p>{{ error }}</p>
        <button @click="loadRecipes" class="btn-retry">Try Again</button>
      </div>

      <!-- Recipes grid -->
      <div v-else>
        <div v-if="recipes.length === 0" class="empty-state">
          <p>You don't own any recipes yet</p>
          <NuxtLink to="/recipes" class="btn-browse">Browse Recipes</NuxtLink>
        </div>

        <div v-else class="recipes-grid">
          <div
            v-for="item in recipes"
            :key="item.recipe.id"
            class="recipe-card"
          >
            <!-- Recipe info -->
            <div class="recipe-header">
              <h3>{{ item.recipe.title }}</h3>
              <div class="recipe-meta">
                <span class="badge" :class="getBadgeClass(item.acquisitionType)">
                  {{ item.acquisitionType }}
                </span>
                <span class="author">By {{ item.recipe.author.email }}</span>
              </div>
            </div>

            <p class="recipe-description">
              {{ item.recipe.description || 'No description available' }}
            </p>

            <div class="recipe-footer">
              <div class="acquired-info">
                <span class="acquired-label">Acquired:</span>
                <span class="acquired-date">{{ formatDate(item.acquiredAt) }}</span>
              </div>
              <button
                @click="openTradeModal(item.recipe)"
                class="btn-trade"
              >
                Trade Recipe
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

    <!-- Trade Modal (placeholder for now) -->
    <div v-if="selectedRecipe" class="modal-overlay" @click="selectedRecipe = null">
      <div class="modal" @click.stop>
        <h3>Trade Recipe</h3>
        <p>Feature coming soon: Trade {{ selectedRecipe.title }} with other users</p>
        <button @click="selectedRecipe = null" class="btn-close">Close</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// My Recipes page - shows recipes owned by the user (via purchase or trade)

definePageMeta({
  middleware: 'auth'
})

// Use composables
const { getMyRecipes } = useTrades()

// State
const recipes = ref<any[]>([])
const pagination = ref<any>(null)
const isLoading = ref(false)
const error = ref('')
const selectedRecipe = ref<any>(null)

// Load recipes on mount
onMounted(() => {
  loadRecipes()
})

/**
 * Load user's owned recipes from API
 */
const loadRecipes = async (page: number = 1) => {
  isLoading.value = true
  error.value = ''

  const result = await getMyRecipes(page, 12) // 12 recipes per page

  if (result.success && result.data) {
    recipes.value = result.data.recipes
    pagination.value = result.data.pagination
  } else {
    error.value = result.error || 'Failed to load your recipes'
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
 * Format date string
 */
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

/**
 * Get badge CSS class based on acquisition type
 */
const getBadgeClass = (type: string) => {
  return type === 'Purchase' ? 'badge-purchase' : 'badge-trade'
}

/**
 * Open trade modal for selected recipe
 */
const openTradeModal = (recipe: any) => {
  selectedRecipe.value = recipe
  // TODO: Implement full trade modal with user selection
}
</script>

<style scoped>
.my-recipes-page {
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

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.btn-browse {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  text-decoration: none;
  border-radius: 0.5rem;
  font-weight: 600;
  transition: background 0.2s;
}

.btn-browse:hover {
  background: #5568d3;
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
  align-items: center;
}

.badge {
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-weight: 600;
  font-size: 0.75rem;
  text-transform: uppercase;
}

.badge-purchase {
  background: #e3f2fd;
  color: #1976d2;
}

.badge-trade {
  background: #fff3e0;
  color: #f57c00;
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

.acquired-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.acquired-label {
  font-size: 0.75rem;
  color: #999;
  text-transform: uppercase;
}

.acquired-date {
  font-size: 0.9rem;
  color: #666;
  font-weight: 600;
}

.btn-trade {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #f57c00 0%, #ff9800 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  transition: transform 0.2s;
}

.btn-trade:hover {
  transform: translateY(-2px);
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

.modal h3 {
  margin: 0 0 1rem 0;
  color: #333;
}

.modal p {
  margin: 0 0 1.5rem 0;
  color: #666;
}

.btn-close {
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

  .recipe-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .btn-trade {
    width: 100%;
  }
}
</style>
