<template>
  <!-- Subscriptions Page - User can purchase subscription plans -->
  <div class="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50">
    <!-- Header -->
    <header class="bg-white/80 backdrop-blur-sm border-b border-gray-200 sticky top-0 z-10">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
              Subscription Plans
            </h1>
            <p class="text-sm text-gray-600 mt-1">
              Get access to premium recipes
            </p>
          </div>
          <div class="flex gap-2">
            <NuxtLink to="/dashboard">
              <button class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                ← Dashboard
              </button>
            </NuxtLink>
            <button
              @click="logout"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <!-- Current Balance -->
      <div class="mb-8 p-6 bg-white rounded-xl shadow-sm border border-gray-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">Your Balance</p>
            <p class="text-3xl font-bold text-gray-900">{{ userBalance }} pts</p>
          </div>
          <NuxtLink to="/dashboard">
            <button class="px-4 py-2 text-sm font-medium text-indigo-600 bg-indigo-50 rounded-lg hover:bg-indigo-100 transition-colors">
              Top Up Balance →
            </button>
          </NuxtLink>
        </div>
      </div>

      <!-- Active Subscriptions -->
      <div v-if="activeSubscriptions && activeSubscriptions.length > 0" class="mb-8">
        <h2 class="text-xl font-bold text-gray-900 mb-4">Your Active Subscriptions</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="userSub in activeSubscriptions"
            :key="userSub.id"
            class="p-6 bg-gradient-to-br from-green-50 to-emerald-50 rounded-xl border-2 border-green-200"
          >
            <div class="flex items-start justify-between mb-4">
              <div>
                <h3 class="text-lg font-bold text-gray-900">{{ userSub.subscription.name }}</h3>
                <p class="text-sm text-gray-600 mt-1">{{ userSub.subscription.description }}</p>
              </div>
              <span class="px-3 py-1 text-xs font-semibold text-green-700 bg-green-100 rounded-full">
                Active
              </span>
            </div>
            <div class="space-y-2 text-sm">
              <div class="flex justify-between">
                <span class="text-gray-600">Days Remaining:</span>
                <span class="font-semibold text-gray-900">{{ userSub.daysRemaining }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Expires:</span>
                <span class="font-semibold text-gray-900">{{ formatDate(userSub.endDate) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Available Subscription Plans -->
      <div>
        <h2 class="text-xl font-bold text-gray-900 mb-4">Available Plans</h2>

        <!-- Loading State -->
        <div v-if="pending" class="text-center py-12">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
          <p class="mt-4 text-gray-600">Loading subscriptions...</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="text-center py-12">
          <p class="text-red-600">Error loading subscriptions: {{ error.message }}</p>
        </div>

        <!-- Subscription Cards -->
        <div v-else-if="subscriptions && subscriptions.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="subscription in subscriptions"
            :key="subscription.id"
            class="p-6 bg-white rounded-xl shadow-sm border border-gray-200 hover:shadow-lg transition-all duration-200 flex flex-col"
          >
            <!-- Subscription Header -->
            <div class="mb-4">
              <h3 class="text-xl font-bold text-gray-900">{{ subscription.name }}</h3>
              <p class="text-sm text-gray-600 mt-2">{{ subscription.description || 'Premium recipe access' }}</p>
            </div>

            <!-- Subscription Details -->
            <div class="space-y-3 mb-6 flex-grow">
              <div class="flex items-center gap-2 text-sm">
                <span class="text-gray-600">Duration:</span>
                <span class="font-semibold text-gray-900">{{ subscription.durationDays }} days</span>
              </div>
              <div class="flex items-center gap-2 text-sm">
                <span class="text-gray-600">Recipes:</span>
                <span class="font-semibold text-gray-900">{{ subscription.recipeCount || 0 }} premium recipes</span>
              </div>
            </div>

            <!-- Price and Buy Button -->
            <div class="border-t pt-4 mt-auto">
              <div class="flex items-end justify-between mb-4">
                <div>
                  <p class="text-sm text-gray-600">Price</p>
                  <p class="text-3xl font-bold text-indigo-600">{{ subscription.price }} pts</p>
                </div>
              </div>
              <button
                @click="buySubscription(subscription)"
                :disabled="purchasing === subscription.id || userBalance < subscription.price"
                class="w-full py-3 px-4 text-sm font-semibold text-white bg-gradient-to-r from-indigo-600 to-purple-600 rounded-lg hover:from-indigo-700 hover:to-purple-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
              >
                <span v-if="purchasing === subscription.id">Purchasing...</span>
                <span v-else-if="userBalance < subscription.price">Insufficient Balance</span>
                <span v-else>Purchase Plan</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="text-center py-12">
          <p class="text-gray-600">No subscription plans available at the moment.</p>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
// Subscriptions page - allows users to view and purchase subscription plans
// Subscriptions grant access to premium recipes for a limited time

// Define authentication requirement
// This middleware redirects unauthenticated users to login page
definePageMeta({
  middleware: 'auth'
})

// ==================== COMPOSABLES ====================

// Get authentication state and user data
const { user, logout: authLogout } = useAuth()

// ==================== STATE ====================

// Track which subscription is currently being purchased
const purchasing = ref<string | null>(null)

// ==================== API CALLS ====================

/**
 * Fetch all available subscription plans
 * useFetch automatically handles caching and SSR
 */
const { data: subscriptions, pending, error } = await useFetch('/api/subscriptions', {
  key: 'available-subscriptions'
})

/**
 * Fetch user's active subscriptions
 * Only fetch if user is authenticated
 */
const { data: activeSubscriptions, refresh: refreshActiveSubscriptions } = await useFetch('/api/subscriptions/my', {
  key: 'my-active-subscriptions'
})

// ==================== COMPUTED ====================

/**
 * Get user's current balance
 * Returns 0 if user is not loaded
 */
const userBalance = computed(() => {
  return user.value?.balance || 0
})

// ==================== METHODS ====================

/**
 * Handle logout
 * Call auth logout and redirect to login page
 */
async function logout() {
  await authLogout()
  navigateTo('/login')
}

/**
 * Purchase a subscription plan
 * Deducts points from user balance and activates subscription
 */
async function buySubscription(subscription: any) {
  // Validate balance
  if (userBalance.value < subscription.price) {
    alert('Insufficient balance. Please top up your account.')
    return
  }

  // Confirm purchase
  if (!confirm(`Purchase ${subscription.name} for ${subscription.price} points?`)) {
    return
  }

  // Set purchasing state
  purchasing.value = subscription.id

  try {
    // Call API to purchase subscription
    const response = await $fetch(`/api/subscriptions/${subscription.id}/buy`, {
      method: 'POST'
    })

    // Show success message
    alert(`Subscription purchased successfully! Expires: ${formatDate((response as any).subscription.endDate)}`)

    // Refresh user data to update balance
    const { fetchUser } = useAuth()
    await fetchUser()

    // Refresh active subscriptions
    await refreshActiveSubscriptions()
  } catch (err: any) {
    // Show error message
    const errorMessage = err.data?.error || err.message || 'Failed to purchase subscription'
    alert(`Error: ${errorMessage}`)
  } finally {
    // Clear purchasing state
    purchasing.value = null
  }
}

/**
 * Format date for display
 * Converts ISO date string to readable format
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// ==================== HEAD MANAGEMENT ====================

// Set page title and meta tags
useHead({
  title: 'Subscriptions - Recipe PWA',
  meta: [
    { name: 'description', content: 'Purchase subscription plans for premium recipe access' }
  ]
})
</script>
