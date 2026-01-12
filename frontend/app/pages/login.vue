<template>
  <div class="container">
    <div class="auth-card">
      <h1>Login</h1>
      <p class="subtitle">Welcome back to Recipes PWA</p>

      <!-- Error message display -->
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <!-- Login form -->
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            placeholder="your@email.com"
            :disabled="isLoading"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            placeholder="Enter your password"
            minlength="6"
            :disabled="isLoading"
          />
        </div>

        <button type="submit" class="btn-primary" :disabled="isLoading">
          {{ isLoading ? 'Logging in...' : 'Login' }}
        </button>
      </form>

      <!-- Link to registration -->
      <p class="footer-text">
        Don't have an account?
        <NuxtLink to="/register" class="link">Register here</NuxtLink>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
// Login page component
// Allows existing users to authenticate with email and password

// Use auth composable for authentication logic
const { login, isLoading } = useAuth()

// Form data - reactive refs for two-way binding
const email = ref('')
const password = ref('')
const errorMessage = ref('')

// Router for navigation after successful login
const router = useRouter()

/**
 * Handle login form submission
 * Calls auth API and redirects to dashboard on success
 */
const handleLogin = async () => {
  // Clear previous error
  errorMessage.value = ''

  // Call login method from useAuth composable
  const result = await login(email.value, password.value)

  if (result.success) {
    // Login successful - redirect to dashboard
    await router.push('/dashboard')
  } else {
    // Login failed - show error message
    errorMessage.value = result.error || 'Login failed. Please try again.'
  }
}
</script>

<style scoped>
/* Container for centering the form */
.container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

/* Card containing the form */
.auth-card {
  background: white;
  padding: 3rem;
  border-radius: 1rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

/* Page title */
h1 {
  margin: 0 0 0.5rem 0;
  font-size: 2rem;
  color: #333;
  text-align: center;
}

/* Subtitle text */
.subtitle {
  margin: 0 0 2rem 0;
  color: #666;
  text-align: center;
}

/* Error message styling */
.error-message {
  background: #fee;
  color: #c33;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
  font-size: 0.9rem;
  border: 1px solid #fcc;
}

/* Form group (label + input) */
.form-group {
  margin-bottom: 1.5rem;
}

/* Form labels */
label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 500;
}

/* Input fields */
input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  font-size: 1rem;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

input:focus {
  outline: none;
  border-color: #667eea;
}

input:disabled {
  background: #f5f5f5;
  cursor: not-allowed;
}

/* Primary button */
.btn-primary {
  width: 100%;
  padding: 0.875rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Footer text with link */
.footer-text {
  margin-top: 1.5rem;
  text-align: center;
  color: #666;
  font-size: 0.9rem;
}

/* Link styling */
.link {
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
}

.link:hover {
  text-decoration: underline;
}
</style>
