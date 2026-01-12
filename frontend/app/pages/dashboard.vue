<template>
  <div class="dashboard">
    <!-- Header with user info and logout -->
    <header class="dashboard-header">
      <h1>Dashboard</h1>
      <button @click="handleLogout" class="btn-logout">Logout</button>
    </header>

    <!-- Loading state -->
    <div v-if="!user" class="loading">
      <p>Loading...</p>
    </div>

    <!-- User profile card -->
    <div v-else class="content">
      <div class="card">
        <h2>Welcome, {{ user.email }}!</h2>

        <div class="balance-section">
          <div class="balance-label">Your Balance</div>
          <div class="balance-amount">{{ user.balance }} points</div>
        </div>

        <div class="user-info">
          <div class="info-row">
            <span class="info-label">User ID:</span>
            <span class="info-value">{{ user.id }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Email:</span>
            <span class="info-value">{{ user.email }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Member since:</span>
            <span class="info-value">{{ formatDate(user.createdAt) }}</span>
          </div>
        </div>
      </div>

      <!-- Quick actions -->
      <div class="actions">
        <h3>Quick Actions</h3>
        <div class="action-buttons">
          <button class="btn-action" disabled>
            Browse Recipes
            <small>Coming soon</small>
          </button>
          <button class="btn-action" disabled>
            Top Up Points
            <small>Coming soon</small>
          </button>
          <button class="btn-action" disabled>
            My Recipes
            <small>Coming soon</small>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Dashboard page - user's personal area
// Shows balance, profile info, and quick actions
// Protected route - requires authentication

// Define page meta for authentication requirement
definePageMeta({
  middleware: 'auth' // This will be created next
})

// Use auth composable to access user data and logout
const { user, logout } = useAuth()

// Router for navigation after logout
const router = useRouter()

/**
 * Format ISO date string to readable format
 */
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

/**
 * Handle logout button click
 * Clears auth and redirects to login
 */
const handleLogout = async () => {
  await logout()
  await router.push('/login')
}
</script>

<style scoped>
/* Dashboard container */
.dashboard {
  min-height: 100vh;
  background: #f5f7fa;
}

/* Header with title and logout button */
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

/* Logout button */
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

/* Loading state */
.loading {
  text-align: center;
  padding: 4rem 2rem;
  color: #666;
  font-size: 1.1rem;
}

/* Main content area */
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

/* Balance display section */
.balance-section {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
  border-radius: 0.75rem;
  text-align: center;
  margin-bottom: 2rem;
}

.balance-label {
  color: rgba(255, 255, 255, 0.9);
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.balance-amount {
  color: white;
  font-size: 3rem;
  font-weight: 700;
}

/* User information rows */
.user-info {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.75rem;
  background: #f8f9fa;
  border-radius: 0.5rem;
}

.info-label {
  color: #666;
  font-weight: 600;
}

.info-value {
  color: #333;
  font-family: monospace;
  font-size: 0.9rem;
}

/* Quick actions section */
.actions h3 {
  margin: 0 0 1rem 0;
  color: #333;
}

.action-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

/* Action button */
.btn-action {
  padding: 1.5rem;
  background: white;
  border: 2px solid #e0e0e0;
  border-radius: 0.75rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.btn-action:not(:disabled):hover {
  border-color: #667eea;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
}

.btn-action:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.btn-action small {
  color: #999;
  font-size: 0.75rem;
  font-weight: normal;
}

/* Responsive design */
@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }

  .content {
    padding: 1rem;
  }

  .balance-amount {
    font-size: 2rem;
  }

  .action-buttons {
    grid-template-columns: 1fr;
  }
}
</style>
