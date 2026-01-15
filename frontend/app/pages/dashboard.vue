<template>
  <div class="dashboard">
    <!-- Header with user info and logout -->
    <header class="dashboard-header">
      <h1>Dashboard</h1>
      <div class="header-actions">
        <button v-if="user?.isAdmin" @click="goToAdminPanel" class="btn-admin">
          Admin Panel
        </button>
        <button @click="handleLogout" class="btn-logout">Logout</button>
      </div>
    </header>

    <!-- Loading state -->
    <div v-if="!user" class="loading">
      <p>Loading...</p>
    </div>

    <!-- Main content -->
    <div v-else class="content">
      <!-- Balance Card -->
      <div class="card">
        <h2>Your Balance</h2>
        <div class="balance-display">
          <div class="balance-amount">{{ user.balance }} points</div>
        </div>

        <!-- Top-up form -->
        <div class="topup-section">
          <h3>Add Points</h3>
          <div v-if="topupError" class="error-message">{{ topupError }}</div>
          <div v-if="topupSuccess" class="success-message">{{ topupSuccess }}</div>

          <form @submit.prevent="handleTopUp" class="topup-form">
            <div class="amount-buttons">
              <button
                type="button"
                v-for="amount in [10, 50, 100, 500]"
                :key="amount"
                @click="topupAmount = amount"
                :class="{ active: topupAmount === amount }"
                class="amount-btn"
              >
                {{ amount }} points
              </button>
            </div>

            <div class="custom-amount">
              <input
                v-model.number="topupAmount"
                type="number"
                min="1"
                placeholder="Or enter custom amount"
                :disabled="isTopupLoading"
              />
            </div>

            <button type="submit" class="btn-primary" :disabled="isTopupLoading || !topupAmount">
              {{ isTopupLoading ? 'Processing...' : `Add ${topupAmount || 0} Points` }}
            </button>
          </form>
        </div>
      </div>

      <!-- Active Subscription Card -->
      <div v-if="activeSubscription" class="card subscription-card">
        <div class="subscription-header">
          <h3>✨ Active Subscription</h3>
          <span class="badge-active">Active</span>
        </div>
        <div class="subscription-details">
          <div class="subscription-name">{{ activeSubscription.subscription.name }}</div>
          <div class="subscription-info">
            <div class="info-item">
              <span class="info-label">Days Remaining:</span>
              <span class="info-value">{{ activeSubscription.daysRemaining }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Expires:</span>
              <span class="info-value">{{ formatDate(activeSubscription.endDate) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Links -->
      <div class="quick-links">
        <NuxtLink to="/subscriptions" class="link-card subscription-link">
          <div class="link-icon">⭐</div>
          <div class="link-text">
            <h3>Subscriptions</h3>
            <p>Get premium recipe access</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/recipes" class="link-card">
          <div class="link-icon">📖</div>
          <div class="link-text">
            <h3>Browse Recipes</h3>
            <p>Discover and purchase recipes</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/my-recipes" class="link-card">
          <div class="link-icon">🍳</div>
          <div class="link-text">
            <h3>My Recipes</h3>
            <p>View your owned recipes and trade</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/trades" class="link-card">
          <div class="link-icon">🔄</div>
          <div class="link-text">
            <h3>Trade Offers</h3>
            <p>Manage incoming and outgoing trades</p>
          </div>
        </NuxtLink>
      </div>

      <!-- Transaction History -->
      <div class="card transactions-card">
        <h3>Recent Transactions</h3>

        <div v-if="isLoadingTransactions" class="loading-small">
          Loading transactions...
        </div>

        <div v-else-if="transactions.length === 0" class="empty-state">
          <p>No transactions yet</p>
        </div>

        <div v-else class="transactions-list">
          <div
            v-for="transaction in transactions"
            :key="transaction.id"
            class="transaction-item"
            :class="getTransactionClass(transaction.type)"
          >
            <div class="transaction-main">
              <div class="transaction-type">
                {{ getTransactionIcon(transaction.type) }}
                {{ getTransactionLabel(transaction.type) }}
              </div>
              <div v-if="transaction.recipe" class="transaction-recipe">
                {{ transaction.recipe.title }}
              </div>
            </div>

            <div class="transaction-details">
              <div class="transaction-amount" :class="getAmountClass(transaction.amount)">
                {{ transaction.amount > 0 ? '+' : '' }}{{ transaction.amount }} points
              </div>
              <div class="transaction-date">
                {{ formatDate(transaction.createdAt) }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Dashboard page - user's personal area with balance, top-up, and transactions

definePageMeta({
  middleware: 'auth',
  ssr: false // Disable SSR for protected pages to avoid cookie issues
})

// Use composables
const { user, logout } = useAuth()
const { topUp, getTransactions } = usePoints()
const router = useRouter()

// Get runtime config for admin URL
const config = useRuntimeConfig()

// Computed admin URL with token for SSO
// Gets token from cookie and passes it as URL parameter

// Top-up form state
const topupAmount = ref<number | null>(null)
const isTopupLoading = ref(false)
const topupError = ref('')
const topupSuccess = ref('')

// Transactions state
const transactions = ref<any[]>([])
const isLoadingTransactions = ref(false)

// Active subscription state
const activeSubscription = ref<any>(null)

// Load transactions and active subscription on mount
onMounted(async () => {
  await loadTransactions()
  await loadActiveSubscription()
})

/**
 * Load user's active subscription
 */
const loadActiveSubscription = async () => {
  try {
    const response = await $fetch('/api/subscriptions/my')
    // Get the first active subscription (if any)
    if (Array.isArray(response) && response.length > 0) {
      activeSubscription.value = response[0]
    }
  } catch (err) {
    console.error('Failed to load active subscription:', err)
  }
}

/**
 * Load user's transaction history
 */
const loadTransactions = async () => {
  isLoadingTransactions.value = true
  const result = await getTransactions(1, 10) // First 10 transactions

  if (result.success && result.data) {
    transactions.value = result.data.transactions
  }

  isLoadingTransactions.value = false
}

/**
 * Handle points top-up
 */
const handleTopUp = async () => {
  if (!topupAmount.value || topupAmount.value <= 0) return

  topupError.value = ''
  topupSuccess.value = ''
  isTopupLoading.value = true

  const result = await topUp(topupAmount.value)

  if (result.success) {
    topupSuccess.value = `Successfully added ${topupAmount.value} points!`
    topupAmount.value = null

    // Reload transactions to show new top-up
    await loadTransactions()

    // Clear success message after 3 seconds
    setTimeout(() => {
      topupSuccess.value = ''
    }, 3000)
  } else {
    topupError.value = result.error || 'Failed to add points'
  }

  isTopupLoading.value = false
}

/**
 * Navigate to admin panel with token for SSO
 */
const goToAdminPanel = () => {
  const baseUrl = config.public.adminUrl || 'http://localhost:5173'
  const token = useCookie('auth_token')

  if (token.value) {
    window.location.href = `${baseUrl}?token=${token.value}`
  } else {
    window.location.href = baseUrl
  }
}

/**
 * Handle logout
 */
const handleLogout = async () => {
  await logout()
  await router.push('/login')
}

/**
 * Format date string
 */
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

/**
 * Get transaction type icon
 */
const getTransactionIcon = (type: string) => {
  const icons: Record<string, string> = {
    TopUp: '💰',
    Purchase: '🛒',
    Sale: '💸',
  }
  return icons[type] || '•'
}

/**
 * Get transaction type label
 */
const getTransactionLabel = (type: string) => {
  const labels: Record<string, string> = {
    TopUp: 'Points Added',
    Purchase: 'Recipe Purchased',
    Sale: 'Recipe Sold',
  }
  return labels[type] || type
}

/**
 * Get CSS class for transaction type
 */
const getTransactionClass = (type: string) => {
  return `transaction-${type.toLowerCase()}`
}

/**
 * Get CSS class for amount (positive/negative)
 */
const getAmountClass = (amount: number) => {
  return amount > 0 ? 'amount-positive' : 'amount-negative'
}
</script>

<style scoped>
/* Dashboard container */
.dashboard {
  min-height: 100vh;
  background: #f5f7fa;
}

/* Header */
.dashboard-header {
  background: white;
  padding: 1.5rem 2rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.dashboard-header h1 {
  margin: 0;
  font-size: 1.75rem;
  color: #333;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.btn-admin {
  padding: 0.5rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  transition: transform 0.2s;
  display: inline-block;
}

.btn-admin:hover {
  transform: translateY(-2px);
}

.btn-logout {
  padding: 0.5rem 1.5rem;
  background: #dc3545;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-logout:hover {
  background: #c82333;
}

/* Loading states */
.loading,
.loading-small {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.loading-small {
  padding: 1rem;
  font-size: 0.9rem;
}

/* Main content */
.content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

/* Card component */
.card {
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.card h2 {
  margin: 0 0 1.5rem 0;
  color: #333;
  font-size: 1.5rem;
}

.card h3 {
  margin: 2rem 0 1rem 0;
  color: #333;
  font-size: 1.2rem;
}

/* Balance display */
.balance-display {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
  border-radius: 0.75rem;
  text-align: center;
  margin-bottom: 2rem;
}

.balance-amount {
  color: white;
  font-size: 3rem;
  font-weight: 700;
}

/* Top-up section */
.topup-section {
  border-top: 1px solid #e0e0e0;
  padding-top: 2rem;
}

.topup-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.amount-buttons {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.5rem;
}

.amount-btn {
  padding: 0.75rem;
  background: #f5f5f5;
  border: 2px solid #e0e0e0;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.amount-btn:hover {
  background: #e8e8e8;
}

.amount-btn.active {
  background: #667eea;
  color: white;
  border-color: #667eea;
}

.custom-amount input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  font-size: 1rem;
  box-sizing: border-box;
}

.custom-amount input:focus {
  outline: none;
  border-color: #667eea;
}

.btn-primary {
  padding: 0.875rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Messages */
.error-message {
  background: #fee;
  color: #c33;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  border: 1px solid #fcc;
}

.success-message {
  background: #efe;
  color: #2a2;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  border: 1px solid #cfc;
}

/* Subscription Card */
.subscription-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  margin-bottom: 2rem;
}

.subscription-card h3 {
  color: white;
  margin: 0;
}

.subscription-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.badge-active {
  background: rgba(255, 255, 255, 0.3);
  padding: 0.375rem 0.875rem;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 600;
}

.subscription-name {
  font-size: 1.5rem;
  font-weight: 700;
  margin-bottom: 1rem;
}

.subscription-info {
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.875rem;
  opacity: 0.9;
}

.info-value {
  font-size: 1.125rem;
  font-weight: 600;
}

/* Quick links */
.quick-links {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.link-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  background: white;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  text-decoration: none;
  transition: transform 0.2s;
}

.link-card:hover {
  transform: translateY(-2px);
}

.subscription-link {
  background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
  border: 2px solid #f59e0b;
}

.link-icon {
  font-size: 3rem;
}

.link-text h3 {
  margin: 0 0 0.25rem 0;
  color: #333;
}

.link-text p {
  margin: 0;
  color: #666;
  font-size: 0.9rem;
}

/* Transactions */
.transactions-card h3 {
  margin-top: 0;
}

.transactions-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.transaction-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 0.5rem;
  border-left: 4px solid #ddd;
}

.transaction-topup {
  border-left-color: #28a745;
}

.transaction-purchase {
  border-left-color: #dc3545;
}

.transaction-sale {
  border-left-color: #ffc107;
}

.transaction-main {
  flex: 1;
}

.transaction-type {
  font-weight: 600;
  color: #333;
  margin-bottom: 0.25rem;
}

.transaction-recipe {
  font-size: 0.9rem;
  color: #666;
}

.transaction-details {
  text-align: right;
}

.transaction-amount {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 0.25rem;
}

.amount-positive {
  color: #28a745;
}

.amount-negative {
  color: #dc3545;
}

.transaction-date {
  font-size: 0.85rem;
  color: #999;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: #999;
}

/* Responsive */
@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    gap: 1rem;
  }

  .content {
    padding: 1rem;
  }

  .balance-amount {
    font-size: 2rem;
  }

  .amount-buttons {
    grid-template-columns: repeat(2, 1fr);
  }

  .transaction-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .transaction-details {
    text-align: left;
    width: 100%;
    display: flex;
    justify-content: space-between;
  }
}
</style>
