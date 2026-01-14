<template>
  <div class="auth-page">
    <!-- Background decoration -->
    <div class="auth-background"></div>

    <div class="auth-container">
      <!-- Left side - Branding -->
      <div class="auth-branding animate-fade-in">
        <div class="brand-icon">🍳</div>
        <h1 class="brand-title">Welcome Back!</h1>
        <p class="brand-description">
          Continue your culinary journey and discover amazing recipes from our community
        </p>
        <div class="brand-features">
          <div class="feature-item">
            <span class="feature-icon">✨</span>
            <span>Exclusive Recipes</span>
          </div>
          <div class="feature-item">
            <span class="feature-icon">💎</span>
            <span>Points Economy</span>
          </div>
          <div class="feature-item">
            <span class="feature-icon">🔄</span>
            <span>Recipe Trading</span>
          </div>
        </div>
      </div>

      <!-- Right side - Form -->
      <div class="auth-card animate-slide-in-right">
        <div class="auth-card-header">
          <h2>Sign In</h2>
          <p>Enter your credentials to access your account</p>
        </div>

        <!-- Error message display -->
        <div v-if="errorMessage" class="error-message animate-shake">
          <span class="error-icon">⚠️</span>
          {{ errorMessage }}
        </div>

        <!-- Login form -->
        <form @submit.prevent="handleLogin" class="auth-form">
          <div class="form-group">
            <label for="email">
              <span class="label-icon">📧</span>
              Email Address
            </label>
            <input
              id="email"
              v-model="email"
              type="email"
              required
              placeholder="your@email.com"
              :disabled="isLoading"
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label for="password">
              <span class="label-icon">🔒</span>
              Password
            </label>
            <input
              id="password"
              v-model="password"
              type="password"
              required
              placeholder="Enter your password"
              minlength="6"
              :disabled="isLoading"
              class="form-input"
            />
          </div>

          <button type="submit" class="btn btn-primary btn-large" :disabled="isLoading">
            <span v-if="isLoading" class="btn-spinner">⏳</span>
            <span v-else class="btn-icon">🚀</span>
            {{ isLoading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>

        <!-- Footer with register link -->
        <div class="auth-footer">
          <p class="footer-text">
            Don't have an account?
            <NuxtLink to="/register" class="footer-link">Create one now</NuxtLink>
          </p>
          <NuxtLink to="/" class="back-link">
            <span class="back-icon">←</span>
            Back to Home
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Login page component with modern design
// Allows existing users to authenticate with email and password

// Use auth composable for authentication logic
const { login, isLoading } = useAuth()

// Form data - reactive refs for two-way binding
const email = ref('')
const password = ref('')
const errorMessage = ref('')

// Router for navigation after successful login
const router = useRouter()
const route = useRoute()

// Get redirect URL from query parameter (e.g., from admin panel)
const redirectUrl = route.query.redirect as string | undefined

// Set page metadata
useHead({
  title: 'Sign In - Recipes PWA',
  meta: [
    { name: 'description', content: 'Sign in to your Recipes PWA account' }
  ]
})

/**
 * Handle login form submission
 * Calls auth API and redirects to dashboard or redirect URL on success
 */
const handleLogin = async () => {
  // Clear previous error
  errorMessage.value = ''

  // Call login method from useAuth composable
  const result = await login(email.value, password.value)

  if (result.success) {
    // Login successful - redirect to specified URL or dashboard
    if (redirectUrl) {
      // External redirect (e.g., to admin panel)
      window.location.href = redirectUrl
    } else {
      // Internal redirect to dashboard
      await router.push('/dashboard')
    }
  } else {
    // Login failed - show error message
    errorMessage.value = result.error || 'Login failed. Please try again.'
  }
}
</script>

<style scoped>
/* Main auth page container */
.auth-page {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: var(--bg-primary);
  overflow: hidden;
}

/* Animated background decoration */
.auth-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
  z-index: 0;
}

.auth-background::before {
  content: '';
  position: absolute;
  top: -50%;
  left: -50%;
  width: 100%;
  height: 100%;
  background: radial-gradient(circle, rgba(102, 126, 234, 0.1) 0%, transparent 70%);
  animation: pulse 4s ease-in-out infinite;
}

/* Container with two columns */
.auth-container {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  max-width: 1200px;
  width: 100%;
  align-items: center;
}

/* Left side - Branding section */
.auth-branding {
  text-align: center;
  color: var(--text-primary);
}

.brand-icon {
  font-size: 5rem;
  margin-bottom: 1.5rem;
  animation: float 3s ease-in-out infinite;
}

.brand-title {
  font-size: clamp(2rem, 4vw, 3rem);
  font-weight: 800;
  font-family: var(--font-display);
  margin-bottom: 1rem;
  background: var(--gradient-primary);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.brand-description {
  font-size: 1.125rem;
  color: var(--text-secondary);
  line-height: 1.7;
  margin-bottom: 2rem;
  max-width: 500px;
  margin-left: auto;
  margin-right: auto;
}

.brand-features {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  max-width: 400px;
  margin: 0 auto;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.5rem;
  background: white;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow);
  font-weight: 500;
  transition: all 0.3s ease;
}

.feature-item:hover {
  transform: translateX(10px);
  box-shadow: var(--shadow-lg);
}

.feature-icon {
  font-size: 1.5rem;
}

/* Right side - Form card */
.auth-card {
  background: white;
  padding: 3rem;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  width: 100%;
  max-width: 500px;
  margin-left: auto;
}

.auth-card-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-card-header h2 {
  font-size: 2rem;
  font-weight: 700;
  font-family: var(--font-display);
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.auth-card-header p {
  color: var(--text-secondary);
  font-size: 0.95rem;
}

/* Error message with shake animation */
.error-message {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--error-light);
  color: var(--error);
  padding: 1rem 1.25rem;
  border-radius: var(--radius);
  margin-bottom: 1.5rem;
  font-size: 0.9rem;
  border: 1px solid var(--error-border);
  animation: shake 0.5s ease;
}

.error-icon {
  font-size: 1.25rem;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-10px); }
  75% { transform: translateX(10px); }
}

/* Form styling */
.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-primary);
  font-weight: 600;
  font-size: 0.95rem;
}

.label-icon {
  font-size: 1.125rem;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1.125rem;
  border: 2px solid var(--border-light);
  border-radius: var(--radius);
  font-size: 1rem;
  font-family: var(--font-sans);
  transition: all 0.3s ease;
  background: var(--bg-secondary);
}

.form-input:focus {
  outline: none;
  border-color: var(--primary);
  background: white;
  box-shadow: 0 0 0 3px var(--primary-light);
}

.form-input:disabled {
  background: var(--bg-tertiary);
  cursor: not-allowed;
  opacity: 0.6;
}

.form-input::placeholder {
  color: var(--text-tertiary);
}

/* Button styling */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  font-size: 1rem;
  font-family: var(--font-sans);
  cursor: pointer;
  transition: all 0.3s ease;
  border: none;
  text-decoration: none;
}

.btn-large {
  padding: 1rem 2rem;
  font-size: 1.125rem;
}

.btn-primary {
  background: var(--gradient-primary);
  color: white;
  box-shadow: var(--shadow);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-icon,
.btn-spinner {
  font-size: 1.25em;
}

.btn-spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Footer section */
.auth-footer {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-light);
  text-align: center;
}

.footer-text {
  color: var(--text-secondary);
  font-size: 0.95rem;
  margin-bottom: 1rem;
}

.footer-link {
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s ease;
}

.footer-link:hover {
  color: var(--primary-dark);
  text-decoration: underline;
}

.back-link {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-tertiary);
  text-decoration: none;
  font-size: 0.9rem;
  transition: all 0.2s ease;
}

.back-link:hover {
  color: var(--primary);
  gap: 0.75rem;
}

.back-icon {
  transition: transform 0.2s ease;
}

.back-link:hover .back-icon {
  transform: translateX(-3px);
}

/* Responsive design */
@media (max-width: 968px) {
  .auth-container {
    grid-template-columns: 1fr;
    gap: 2rem;
  }

  .auth-branding {
    display: none;
  }

  .auth-card {
    margin: 0 auto;
  }
}

@media (max-width: 640px) {
  .auth-page {
    padding: 1rem;
  }

  .auth-card {
    padding: 2rem 1.5rem;
  }

  .auth-card-header h2 {
    font-size: 1.5rem;
  }

  .brand-features {
    gap: 0.75rem;
  }

  .feature-item {
    padding: 0.75rem 1rem;
  }
}
</style>
