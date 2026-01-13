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
                <span class="author">By {{ item.recipe.author.username }}</span>
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

    <!-- Trade Modal -->
    <div v-if="showTradeModal" class="modal-overlay" @click="closeTradeModal">
      <div class="modal trade-modal" @click.stop>
        <div class="modal-header">
          <h3>Create Trade Offer</h3>
          <button @click="closeTradeModal" class="btn-close-x">✕</button>
        </div>

        <div class="modal-body">
          <!-- Your recipe (selected) -->
          <div class="trade-section">
            <h4>You Offer</h4>
            <div class="recipe-preview">
              <div class="recipe-name">{{ selectedRecipeForTrade?.title }}</div>
              <div class="recipe-price">{{ selectedRecipeForTrade?.price }} points</div>
            </div>
          </div>

          <div class="trade-arrow">⇄</div>

          <!-- Username search -->
          <div class="trade-section">
            <h4>Trade With</h4>
            <div class="username-search">
              <input
                v-model="searchUsername"
                type="text"
                placeholder="Enter username..."
                class="username-input"
                @input="onUsernameInput"
              />
              <button
                @click="searchUser"
                :disabled="!searchUsername || isSearching"
                class="btn-search"
              >
                {{ isSearching ? 'Searching...' : 'Search' }}
              </button>
            </div>

            <!-- Show found user -->
            <div v-if="foundUser" class="found-user">
              ✓ Found: <strong>{{ foundUser.username }}</strong>
            </div>
          </div>

          <!-- Select recipe to request -->
          <div v-if="foundUser" class="trade-section">
            <h4>You Want</h4>

            <!-- Loading state -->
            <div v-if="isLoadingAvailable" class="loading-small">Loading recipes...</div>

            <!-- Recipe selector -->
            <div v-else-if="availableRecipes.length > 0" class="recipe-selector">
              <select v-model="selectedRequestedRecipeId" class="recipe-select">
                <option value="">-- Select a recipe --</option>
                <option
                  v-for="recipe in availableRecipes"
                  :key="recipe.id"
                  :value="recipe.id"
                >
                  {{ recipe.title }} ({{ recipe.price }} points)
                </option>
              </select>
            </div>

            <!-- No recipes available -->
            <div v-else class="no-recipes">
              <p>This user has no recipes available for trading</p>
            </div>
          </div>

          <!-- Error message -->
          <div v-if="tradeError" class="error-message">{{ tradeError }}</div>
        </div>

        <div class="modal-footer">
          <button @click="closeTradeModal" class="btn-secondary">Cancel</button>
          <button
            @click="submitTradeOffer"
            :disabled="!selectedRequestedRecipeId || isSubmitting"
            class="btn-primary"
          >
            {{ isSubmitting ? 'Creating...' : 'Create Offer' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Success Modal -->
    <div v-if="showSuccessModal" class="modal-overlay" @click="showSuccessModal = false">
      <div class="modal success-modal" @click.stop>
        <div class="success-icon">✅</div>
        <h3>Trade Offer Created!</h3>
        <p>Your trade offer has been sent. Check the Trades page to see the status.</p>
        <button @click="showSuccessModal = false" class="btn-primary">Close</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// My Recipes page - shows recipes owned by the user (via purchase or trade)

definePageMeta({
  middleware: 'auth',
  ssr: false
})

// Use composables
const { getMyRecipes, createTradeOffer, searchUserByUsername } = useTrades()

// State - My Recipes
const recipes = ref<any[]>([])
const pagination = ref<any>(null)
const isLoading = ref(false)
const error = ref('')

// State - Trade Modal
const showTradeModal = ref(false)
const selectedRecipeForTrade = ref<any>(null)
const searchUsername = ref('')
const foundUser = ref<any>(null)
const isSearching = ref(false)
const availableRecipes = ref<any[]>([])
const isLoadingAvailable = ref(false)
const selectedRequestedRecipeId = ref('')
const tradeError = ref('')
const isSubmitting = ref(false)
const showSuccessModal = ref(false)

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
const openTradeModal = async (recipe: any) => {
  selectedRecipeForTrade.value = recipe
  showTradeModal.value = true
  tradeError.value = ''
  selectedRequestedRecipeId.value = ''
  searchUsername.value = ''
  foundUser.value = null
  availableRecipes.value = []
}

/**
 * Search for a user by username
 */
const searchUser = async () => {
  if (!searchUsername.value.trim()) {
    return
  }

  isSearching.value = true
  isLoadingAvailable.value = true
  tradeError.value = ''
  availableRecipes.value = []
  foundUser.value = null

  const result = await searchUserByUsername(searchUsername.value.trim())

  if (result.success && result.data) {
    foundUser.value = result.data.user
    availableRecipes.value = result.data.recipes
  } else {
    tradeError.value = result.error || 'User not found'
  }

  isSearching.value = false
  isLoadingAvailable.value = false
}

/**
 * Handle username input change
 * Clear results when username changes
 */
const onUsernameInput = () => {
  foundUser.value = null
  availableRecipes.value = []
  selectedRequestedRecipeId.value = ''
  tradeError.value = ''
}

/**
 * Close trade modal
 */
const closeTradeModal = () => {
  showTradeModal.value = false
  selectedRecipeForTrade.value = null
  selectedRequestedRecipeId.value = ''
  searchUsername.value = ''
  foundUser.value = null
  availableRecipes.value = []
  tradeError.value = ''
}

/**
 * Submit trade offer
 */
const submitTradeOffer = async () => {
  if (!selectedRecipeForTrade.value || !selectedRequestedRecipeId.value || !foundUser.value) {
    return
  }

  isSubmitting.value = true
  tradeError.value = ''

  const result = await createTradeOffer(
    selectedRecipeForTrade.value.id,
    foundUser.value.id,
    selectedRequestedRecipeId.value
  )

  if (result.success) {
    showTradeModal.value = false
    showSuccessModal.value = true
  } else {
    tradeError.value = result.error || 'Failed to create trade offer'
  }

  isSubmitting.value = false
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
  position: relative;
  z-index: 1001;
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

/* Trade Modal Styles */
.trade-modal {
  max-width: 600px;
  padding: 0;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h3 {
  margin: 0;
}

.btn-close-x {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #999;
  padding: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-close-x:hover {
  color: #333;
}

.modal-body {
  padding: 1.5rem;
}

.trade-section {
  margin-bottom: 1rem;
}

.trade-section h4 {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
  color: #999;
  text-transform: uppercase;
}

.recipe-preview {
  background: #f5f7fa;
  padding: 1rem;
  border-radius: 0.5rem;
}

.recipe-name {
  font-weight: 600;
  color: #333;
  margin-bottom: 0.25rem;
}

.recipe-price {
  color: #667eea;
  font-size: 0.9rem;
}

.trade-arrow {
  text-align: center;
  font-size: 2rem;
  color: #667eea;
  margin: 1rem 0;
}

.username-search {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.username-input {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  font-size: 1rem;
}

.username-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.btn-search {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  white-space: nowrap;
}

.btn-search:hover:not(:disabled) {
  background: #5568d3;
}

.btn-search:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.found-user {
  margin-top: 0.75rem;
  padding: 0.75rem;
  background: #e8f5e9;
  color: #2e7d32;
  border-radius: 0.5rem;
  font-size: 0.9rem;
}

.recipe-selector {
  margin-top: 0.5rem;
}

.recipe-select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  font-size: 1rem;
  background: white;
  cursor: pointer;
  position: relative;
  z-index: 1;
  pointer-events: auto;
}

.recipe-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.loading-small {
  text-align: center;
  padding: 1rem;
  color: #999;
}

.no-recipes {
  text-align: center;
  padding: 1rem;
  color: #999;
  background: #f5f7fa;
  border-radius: 0.5rem;
}

.error-message {
  background: #fee;
  color: #c33;
  padding: 0.75rem;
  border-radius: 0.5rem;
  margin-top: 1rem;
  font-size: 0.9rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.btn-secondary {
  padding: 0.75rem 1.5rem;
  background: #e0e0e0;
  color: #333;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-secondary:hover {
  background: #d0d0d0;
}

.btn-primary {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-primary:hover:not(:disabled) {
  background: #5568d3;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Success Modal */
.success-modal {
  text-align: center;
  padding: 2rem;
}

.success-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.success-modal h3 {
  color: #28a745;
  margin-bottom: 1rem;
}

.success-modal p {
  margin-bottom: 1.5rem;
  color: #666;
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
