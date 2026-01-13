// Authentication middleware
// Protects routes that require user to be logged in
// Redirects to login page if user is not authenticated
// Note: Global middleware (00.init-auth.global.ts) loads user first

export default defineNuxtRouteMiddleware(async (to, from) => {
  // CRITICAL: Only run on client side
  // Server doesn't have access to cookies or client state
  if (!process.client) {
    return
  }

  const { user } = useAuth()

  // Simply check if user exists
  // Global middleware already tried to load user from cookie
  if (!user.value) {
    return navigateTo('/login')
  }
})
