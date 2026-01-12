<template>
  <div class="trades-page">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1>Trade Offers</h1>
        <NuxtLink to="/dashboard" class="btn-back">← Dashboard</NuxtLink>
      </div>
    </header>

    <!-- Main content -->
    <div class="content">
      <!-- Tab navigation -->
      <div class="tabs">
        <button
          @click="activeTab = 'incoming'"
          :class="{ active: activeTab === 'incoming' }"
          class="tab-btn"
        >
          Incoming Offers
          <span v-if="incomingTrades.length > 0" class="badge">{{ incomingTrades.length }}</span>
        </button>
        <button
          @click="activeTab = 'outgoing'"
          :class="{ active: activeTab === 'outgoing' }"
          class="tab-btn"
        >
          My Offers
        </button>
      </div>

      <!-- Incoming Trades Tab -->
      <div v-if="activeTab === 'incoming'">
        <!-- Loading state -->
        <div v-if="isLoadingIncoming" class="loading">
          <p>Loading incoming trades...</p>
        </div>

        <!-- Error state -->
        <div v-else-if="incomingError" class="error-card">
          <p>{{ incomingError }}</p>
          <button @click="loadIncomingTrades" class="btn-retry">Try Again</button>
        </div>

        <!-- Empty state -->
        <div v-else-if="incomingTrades.length === 0" class="empty-state">
          <p>No incoming trade offers</p>
        </div>

        <!-- Trades list -->
        <div v-else class="trades-list">
          <div
            v-for="trade in incomingTrades"
            :key="trade.id"
            class="trade-card"
          >
            <div class="trade-header">
              <span class="user-label">From: {{ trade.offeringUser?.email }}</span>
            </div>

            <div class="trade-content">
              <div class="trade-side">
                <h4>They Offer</h4>
                <div class="recipe-info">
                  <span class="recipe-title">{{ trade.offeredRecipe.title }}</span>
                  <span class="recipe-price">{{ trade.offeredRecipe.price }} points</span>
                </div>
              </div>

              <div class="trade-arrow">⇄</div>

              <div class="trade-side">
                <h4>They Want</h4>
                <div class="recipe-info">
                  <span class="recipe-title">{{ trade.requestedRecipe.title }}</span>
                  <span class="recipe-price">{{ trade.requestedRecipe.price }} points</span>
                </div>
              </div>
            </div>

            <div class="trade-footer">
              <span class="trade-date">{{ formatDate(trade.createdAt) }}</span>
              <div class="trade-actions">
                <button
                  @click="handleAcceptTrade(trade.id)"
                  :disabled="isProcessing === trade.id"
                  class="btn-accept"
                >
                  {{ isProcessing === trade.id ? 'Processing...' : 'Accept' }}
                </button>
                <button
                  @click="handleDeclineTrade(trade.id)"
                  :disabled="isProcessing === trade.id"
                  class="btn-decline"
                >
                  Decline
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Outgoing Trades Tab -->
      <div v-if="activeTab === 'outgoing'">
        <!-- Loading state -->
        <div v-if="isLoadingOutgoing" class="loading">
          <p>Loading your offers...</p>
        </div>

        <!-- Error state -->
        <div v-else-if="outgoingError" class="error-card">
          <p>{{ outgoingError }}</p>
          <button @click="loadOutgoingTrades" class="btn-retry">Try Again</button>
        </div>

        <!-- Empty state -->
        <div v-else-if="outgoingTrades.length === 0" class="empty-state">
          <p>You haven't made any trade offers yet</p>
          <NuxtLink to="/my-recipes" class="btn-browse">Go to My Recipes</NuxtLink>
        </div>

        <!-- Trades list -->
        <div v-else class="trades-list">
          <div
            v-for="trade in outgoingTrades"
            :key="trade.id"
            class="trade-card"
            :class="`status-${trade.status.toLowerCase()}`"
          >
            <div class="trade-header">
              <span class="user-label">To: {{ trade.requestedUser?.email }}</span>
              <span class="status-badge" :class="`status-${trade.status.toLowerCase()}`">
                {{ trade.status }}
              </span>
            </div>

            <div class="trade-content">
              <div class="trade-side">
                <h4>You Offer</h4>
                <div class="recipe-info">
                  <span class="recipe-title">{{ trade.offeredRecipe.title }}</span>
                  <span class="recipe-price">{{ trade.offeredRecipe.price }} points</span>
                </div>
              </div>

              <div class="trade-arrow">⇄</div>

              <div class="trade-side">
                <h4>You Want</h4>
                <div class="recipe-info">
                  <span class="recipe-title">{{ trade.requestedRecipe.title }}</span>
                  <span class="recipe-price">{{ trade.requestedRecipe.price }} points</span>
                </div>
              </div>
            </div>

            <div class="trade-footer">
              <span class="trade-date">{{ formatDate(trade.createdAt) }}</span>
              <button
                v-if="trade.status === 'Pending'"
                @click="handleCancelTrade(trade.id)"
                :disabled="isProcessing === trade.id"
                class="btn-cancel"
              >
                {{ isProcessing === trade.id ? 'Cancelling...' : 'Cancel Offer' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Result modal -->
    <div v-if="resultMessage" class="modal-overlay" @click="resultMessage = null">
      <div class="modal" @click.stop>
        <div :class="resultSuccess ? 'success-modal' : 'error-modal'">
          <div class="modal-icon">{{ resultSuccess ? '✅' : '❌' }}</div>
          <h3>{{ resultSuccess ? 'Success!' : 'Error' }}</h3>
          <p>{{ resultMessage }}</p>
          <button @click="closeResultModal" class="btn-close">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Trades page - manage incoming and outgoing recipe trade offers

definePageMeta({
  middleware: 'auth'
})

// Use composables
const { getIncomingTrades, getOutgoingTrades, acceptTrade, declineTrade, cancelTrade } = useTrades()

// State
const activeTab = ref<'incoming' | 'outgoing'>('incoming')
const incomingTrades = ref<any[]>([])
const outgoingTrades = ref<any[]>([])
const isLoadingIncoming = ref(false)
const isLoadingOutgoing = ref(false)
const incomingError = ref('')
const outgoingError = ref('')
const isProcessing = ref<string | null>(null)
const resultMessage = ref('')
const resultSuccess = ref(false)

// Load trades on mount
onMounted(() => {
  loadIncomingTrades()
  loadOutgoingTrades()
})

/**
 * Load incoming trade offers
 */
const loadIncomingTrades = async () => {
  isLoadingIncoming.value = true
  incomingError.value = ''

  const result = await getIncomingTrades(1, 50)

  if (result.success && result.data) {
    incomingTrades.value = result.data.trades
  } else {
    incomingError.value = result.error || 'Failed to load incoming trades'
  }

  isLoadingIncoming.value = false
}

/**
 * Load outgoing trade offers
 */
const loadOutgoingTrades = async () => {
  isLoadingOutgoing.value = true
  outgoingError.value = ''

  const result = await getOutgoingTrades(1, 50)

  if (result.success && result.data) {
    outgoingTrades.value = result.data.trades
  } else {
    outgoingError.value = result.error || 'Failed to load your offers'
  }

  isLoadingOutgoing.value = false
}

/**
 * Accept a trade offer
 */
const handleAcceptTrade = async (tradeId: string) => {
  isProcessing.value = tradeId

  const result = await acceptTrade(tradeId)

  if (result.success) {
    resultSuccess.value = true
    resultMessage.value = 'Trade accepted! Recipes exchanged successfully.'

    // Reload both lists
    await loadIncomingTrades()
    await loadOutgoingTrades()
  } else {
    resultSuccess.value = false
    resultMessage.value = result.error || 'Failed to accept trade'
  }

  isProcessing.value = null
}

/**
 * Decline a trade offer
 */
const handleDeclineTrade = async (tradeId: string) => {
  isProcessing.value = tradeId

  const result = await declineTrade(tradeId)

  if (result.success) {
    resultSuccess.value = true
    resultMessage.value = 'Trade offer declined.'

    // Remove from incoming list
    incomingTrades.value = incomingTrades.value.filter(t => t.id !== tradeId)
  } else {
    resultSuccess.value = false
    resultMessage.value = result.error || 'Failed to decline trade'
  }

  isProcessing.value = null
}

/**
 * Cancel your own trade offer
 */
const handleCancelTrade = async (tradeId: string) => {
  isProcessing.value = tradeId

  const result = await cancelTrade(tradeId)

  if (result.success) {
    resultSuccess.value = true
    resultMessage.value = 'Trade offer cancelled.'

    // Reload outgoing list
    await loadOutgoingTrades()
  } else {
    resultSuccess.value = false
    resultMessage.value = result.error || 'Failed to cancel trade'
  }

  isProcessing.value = null
}

/**
 * Close result modal
 */
const closeResultModal = () => {
  resultMessage.value = ''
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
</script>

<style scoped>
.trades-page {
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

/* Tabs */
.tabs {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid #e0e0e0;
}

.tab-btn {
  padding: 1rem 1.5rem;
  background: transparent;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  color: #666;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.tab-btn:hover {
  color: #667eea;
}

.tab-btn.active {
  color: #667eea;
  border-bottom-color: #667eea;
}

.tab-btn .badge {
  background: #667eea;
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 1rem;
  font-size: 0.75rem;
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

/* Trades list */
.trades-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* Trade card */
.trade-card {
  background: white;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  border-left: 4px solid #667eea;
}

.trade-card.status-accepted {
  border-left-color: #28a745;
  opacity: 0.7;
}

.trade-card.status-declined {
  border-left-color: #dc3545;
  opacity: 0.7;
}

.trade-card.status-cancelled {
  border-left-color: #999;
  opacity: 0.7;
}

.trade-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.user-label {
  font-weight: 600;
  color: #333;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 0.25rem;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
}

.status-badge.status-pending {
  background: #fff3cd;
  color: #856404;
}

.status-badge.status-accepted {
  background: #d4edda;
  color: #155724;
}

.status-badge.status-declined {
  background: #f8d7da;
  color: #721c24;
}

.status-badge.status-cancelled {
  background: #e2e3e5;
  color: #383d41;
}

.trade-content {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  gap: 1.5rem;
  align-items: center;
  margin-bottom: 1rem;
}

.trade-side h4 {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
  color: #999;
  text-transform: uppercase;
}

.recipe-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.recipe-title {
  font-weight: 600;
  color: #333;
}

.recipe-price {
  font-size: 0.9rem;
  color: #667eea;
}

.trade-arrow {
  font-size: 2rem;
  color: #667eea;
}

.trade-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #e0e0e0;
}

.trade-date {
  font-size: 0.85rem;
  color: #999;
}

.trade-actions {
  display: flex;
  gap: 0.5rem;
}

.btn-accept,
.btn-decline,
.btn-cancel {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
  transition: transform 0.2s;
}

.btn-accept {
  background: #28a745;
  color: white;
}

.btn-accept:hover:not(:disabled) {
  transform: translateY(-2px);
}

.btn-decline {
  background: #dc3545;
  color: white;
}

.btn-decline:hover:not(:disabled) {
  transform: translateY(-2px);
}

.btn-cancel {
  background: #999;
  color: white;
}

.btn-cancel:hover:not(:disabled) {
  transform: translateY(-2px);
}

.btn-accept:disabled,
.btn-decline:disabled,
.btn-cancel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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
  margin: 0.5rem 0 1.5rem 0;
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

  .content {
    padding: 1rem;
  }

  .trade-content {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .trade-arrow {
    transform: rotate(90deg);
    text-align: center;
  }

  .trade-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .trade-actions {
    width: 100%;
  }

  .btn-accept,
  .btn-decline {
    flex: 1;
  }
}
</style>
