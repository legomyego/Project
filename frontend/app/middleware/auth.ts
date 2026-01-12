// Authentication middleware
// Protects routes that require user to be logged in
// Redirects to login page if user is not authenticated

export default defineNuxtRouteMiddleware(async (to, from) => {
  const { user, fetchUser } = useAuth()

  // If user is not loaded yet, try to fetch from API
  // This happens on page refresh or direct navigation
  if (!user.value) {
    await fetchUser()
  }

  // If still no user after fetch, redirect to login
  if (!user.value) {
    return navigateTo('/login')
  }
})
