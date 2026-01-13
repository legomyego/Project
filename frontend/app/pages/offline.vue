<template>
  <div class="offline-page">
    <div class="offline-content">
      <div class="offline-icon">📡</div>
      <h1>You're Offline</h1>
      <p>It looks like you've lost your internet connection.</p>
      <p class="description">
        Don't worry! You can still view your cached recipes and data.
        The app will automatically reconnect when you're back online.
      </p>

      <div class="actions">
        <button @click="retry" class="btn-retry">
          Try Again
        </button>
        <NuxtLink to="/my-recipes" class="btn-secondary">
          View My Recipes
        </NuxtLink>
      </div>

      <div class="status">
        <span :class="{ online: isOnline, offline: !isOnline }">
          {{ isOnline ? '🟢 Back Online!' : '🔴 Offline' }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Offline fallback page shown when user has no internet connection
// PWA service worker will cache this page for offline access

definePageMeta({
  layout: false, // No layout needed for offline page
})

// Track online/offline status
const isOnline = ref(navigator.onLine)

// Update status when connection changes
const updateOnlineStatus = () => {
  isOnline.value = navigator.onLine
  if (isOnline.value) {
    // Reload page when back online
    setTimeout(() => {
      window.location.reload()
    }, 1000)
  }
}

// Listen for online/offline events
onMounted(() => {
  window.addEventListener('online', updateOnlineStatus)
  window.addEventListener('offline', updateOnlineStatus)
})

// Cleanup listeners
onUnmounted(() => {
  window.removeEventListener('online', updateOnlineStatus)
  window.removeEventListener('offline', updateOnlineStatus)
})

// Retry connection
const retry = () => {
  window.location.reload()
}
</script>

<style scoped>
.offline-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

.offline-content {
  max-width: 500px;
  text-align: center;
  background: white;
  padding: 3rem 2rem;
  border-radius: 1.5rem;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.offline-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.1);
    opacity: 0.8;
  }
}

h1 {
  margin: 0 0 1rem 0;
  font-size: 2rem;
  color: #333;
}

p {
  margin: 0 0 1rem 0;
  color: #666;
  font-size: 1.1rem;
}

.description {
  font-size: 0.95rem;
  color: #888;
  line-height: 1.6;
  margin-bottom: 2rem;
}

.actions {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 2rem;
}

.btn-retry {
  padding: 1rem 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 0.75rem;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.btn-retry:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.4);
}

.btn-secondary {
  padding: 1rem 2rem;
  background: white;
  color: #667eea;
  border: 2px solid #667eea;
  border-radius: 0.75rem;
  font-size: 1rem;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.2s;
}

.btn-secondary:hover {
  background: #f5f7ff;
  transform: translateY(-2px);
}

.status {
  padding: 0.75rem;
  background: #f5f7fa;
  border-radius: 0.5rem;
  font-weight: 600;
}

.status .online {
  color: #28a745;
}

.status .offline {
  color: #dc3545;
}

@media (max-width: 600px) {
  .offline-content {
    padding: 2rem 1.5rem;
  }

  h1 {
    font-size: 1.5rem;
  }

  .offline-icon {
    font-size: 3rem;
  }
}
</style>
