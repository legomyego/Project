<template>
  <div class="container">
    <div class="hero">
      <h1>Welcome to Recipes PWA</h1>
      <p class="tagline">Trade recipes, earn points, build your collection</p>

      <div class="actions">
        <NuxtLink to="/login" class="btn btn-primary">
          Login
        </NuxtLink>
        <NuxtLink to="/register" class="btn btn-secondary">
          Get Started
        </NuxtLink>
      </div>

      <div class="features">
        <div class="feature">
          <div class="feature-icon">📖</div>
          <h3>Browse Recipes</h3>
          <p>Discover delicious recipes from our community</p>
        </div>
        <div class="feature">
          <div class="feature-icon">💰</div>
          <h3>Earn Points</h3>
          <p>Create recipes and earn points from others</p>
        </div>
        <div class="feature">
          <div class="feature-icon">🔄</div>
          <h3>Trade & Collect</h3>
          <p>Build your personal recipe collection</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Home page - landing page for non-authenticated users
// Shows welcome message and links to login/register

// Check if user is already logged in and redirect to dashboard
const { user, fetchUser } = useAuth()
const router = useRouter()

// On mount, check authentication status
onMounted(async () => {
  // Try to fetch user if not already loaded
  if (!user.value) {
    await fetchUser()
  }

  // If user is logged in, redirect to dashboard
  if (user.value) {
    await router.push('/dashboard')
  }
})
</script>

<style scoped>
/* Main container */
.container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

/* Hero section */
.hero {
  text-align: center;
  color: white;
  max-width: 900px;
}

/* Main heading */
h1 {
  font-size: 3.5rem;
  margin: 0 0 1rem 0;
  font-weight: 800;
}

/* Tagline */
.tagline {
  font-size: 1.25rem;
  margin: 0 0 3rem 0;
  opacity: 0.95;
}

/* Action buttons */
.actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 4rem;
}

/* Button base styles */
.btn {
  padding: 1rem 2rem;
  font-size: 1.1rem;
  font-weight: 600;
  border-radius: 0.5rem;
  text-decoration: none;
  transition: all 0.2s;
  display: inline-block;
}

/* Primary button (Login) */
.btn-primary {
  background: white;
  color: #667eea;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(255, 255, 255, 0.3);
}

/* Secondary button (Register) */
.btn-secondary {
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 2px solid white;
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: translateY(-2px);
}

/* Features grid */
.features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-top: 3rem;
}

/* Feature card */
.feature {
  background: rgba(255, 255, 255, 0.1);
  padding: 2rem;
  border-radius: 1rem;
  backdrop-filter: blur(10px);
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.5rem;
}

.feature p {
  margin: 0;
  opacity: 0.9;
}

/* Responsive design */
@media (max-width: 768px) {
  h1 {
    font-size: 2.5rem;
  }

  .tagline {
    font-size: 1rem;
  }

  .actions {
    flex-direction: column;
    max-width: 300px;
    margin: 0 auto 3rem;
  }

  .features {
    grid-template-columns: 1fr;
  }
}
</style>
