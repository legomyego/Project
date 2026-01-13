// Global middleware that runs on every navigation
// Loads user session once on app initialization
// Prefix "00." ensures it runs before other middleware

let isInitialized = false

export default defineNuxtRouteMiddleware(async (to, from) => {
  // Only initialize once on client side
  if (process.client && !isInitialized) {
    const { fetchUser } = useAuth()

    // Try to restore user from cookie
    await fetchUser()

    isInitialized = true
  }
})
