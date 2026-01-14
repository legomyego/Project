// Authentication middleware
// Protects routes that require user to be logged in
// Redirects to login page if user is not authenticated

export default defineNuxtRouteMiddleware(async (to, from) => {
  // CRITICAL: Only run on client side
  // Server doesn't have access to cookies or client state
  if (!process.client) {
    return
  }

  const { user, fetchUser, isLoading } = useAuth()

  // If user is not loaded yet and we're not currently loading, try to fetch
  if (!user.value && !isLoading.value) {
    await fetchUser()
  }

  // After fetch attempt, check if user exists
  if (!user.value) {
    return navigateTo('/login')
  }
})
