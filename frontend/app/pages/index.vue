<template>
  <div class="home-page">
    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-background"></div>
      <div class="container">
        <div class="hero-content animate-fade-in">
          <h1 class="hero-title">
            Discover & Trade
            <span class="gradient-text">Amazing Recipes</span>
          </h1>
          <p class="hero-description animate-fade-in stagger-1">
            Join our community of food lovers. Buy, sell, and trade unique recipes using our point system.
          </p>
          <div class="hero-actions animate-fade-in stagger-2">
            <NuxtLink to="/recipes" class="btn btn-primary btn-lg">
              <span class="btn-icon">🔍</span>
              Explore Recipes
            </NuxtLink>
            <NuxtLink v-if="!user" to="/register" class="btn btn-secondary btn-lg">
              <span class="btn-icon">✨</span>
              Get Started
            </NuxtLink>
            <NuxtLink v-else to="/dashboard" class="btn btn-secondary btn-lg">
              <span class="btn-icon">👤</span>
              Dashboard
            </NuxtLink>
          </div>
          <div class="hero-stats animate-fade-in stagger-3">
            <div class="stat">
              <div class="stat-number">1000+</div>
              <div class="stat-label">Recipes</div>
            </div>
            <div class="stat">
              <div class="stat-number">500+</div>
              <div class="stat-label">Users</div>
            </div>
            <div class="stat">
              <div class="stat-number">5000+</div>
              <div class="stat-label">Trades</div>
            </div>
          </div>
        </div>
        <div class="hero-illustration animate-float">
          <div class="recipe-card card-1">🍕</div>
          <div class="recipe-card card-2">🍰</div>
          <div class="recipe-card card-3">🍜</div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features">
      <div class="container">
        <h2 class="section-title">Why Choose Recipes PWA?</h2>
        <p class="section-description">Everything you need to discover and share amazing recipes</p>

        <div class="features-grid">
          <div class="feature-card animate-fade-in stagger-1 hover-lift">
            <div class="feature-icon">💎</div>
            <h3 class="feature-title">Points Economy</h3>
            <p class="feature-description">
              Earn points by sharing recipes, spend them to unlock exclusive content
            </p>
          </div>

          <div class="feature-card animate-fade-in stagger-2 hover-lift">
            <div class="feature-icon">🔄</div>
            <h3 class="feature-title">Recipe Trading</h3>
            <p class="feature-description">
              Trade recipes with other users and expand your collection
            </p>
          </div>

          <div class="feature-card animate-fade-in stagger-3 hover-lift">
            <div class="feature-icon">📱</div>
            <h3 class="feature-title">PWA Support</h3>
            <p class="feature-description">
              Install on your device and use offline - works like a native app
            </p>
          </div>

          <div class="feature-card animate-fade-in stagger-4 hover-lift">
            <div class="feature-icon">🎯</div>
            <h3 class="feature-title">Smart Search</h3>
            <p class="feature-description">
              Find exactly what you're looking for with advanced filters
            </p>
          </div>

          <div class="feature-card animate-fade-in stagger-1 hover-lift">
            <div class="feature-icon">💰</div>
            <h3 class="feature-title">Subscriptions</h3>
            <p class="feature-description">
              Get access to premium recipes with monthly subscriptions
            </p>
          </div>

          <div class="feature-card animate-fade-in stagger-2 hover-lift">
            <div class="feature-icon">🔒</div>
            <h3 class="feature-title">Secure & Private</h3>
            <p class="feature-description">
              Your data is safe with modern encryption and authentication
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta">
      <div class="container">
        <div class="cta-content animate-scale-in">
          <h2 class="cta-title">Ready to Start Cooking?</h2>
          <p class="cta-description">
            Join thousands of food lovers sharing and discovering amazing recipes
          </p>
          <div class="cta-actions">
            <NuxtLink v-if="!user" to="/register" class="btn btn-white btn-lg">
              Create Free Account
            </NuxtLink>
            <NuxtLink to="/recipes" class="btn btn-outline btn-lg">
              Browse Recipes
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
// Get current user from auth composable
const { user } = useAuth()

// Set page metadata
useHead({
  title: 'Home - Recipes PWA',
  meta: [
    { name: 'description', content: 'Discover, trade, and share amazing recipes with our community' }
  ]
})
</script>

<style scoped>
/* Hero Section */
.hero {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  padding: 4rem 0;
  overflow: hidden;
}

.hero-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  opacity: 0.05;
  z-index: -1;
}

.hero-background::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -20%;
  width: 100%;
  height: 100%;
  background: radial-gradient(circle, rgba(102, 126, 234, 0.3) 0%, transparent 70%);
  animation: pulse 4s ease-in-out infinite;
}

.hero .container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  align-items: center;
}

.hero-content {
  max-width: 600px;
}

.hero-title {
  font-size: clamp(2.5rem, 5vw, 4rem);
  font-weight: 800;
  line-height: 1.1;
  margin-bottom: 1.5rem;
}

.hero-description {
  font-size: 1.25rem;
  color: var(--text-secondary);
  margin-bottom: 2rem;
  line-height: 1.7;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 3rem;
}

.hero-stats {
  display: flex;
  gap: 3rem;
}

.stat {
  text-align: center;
}

.stat-number {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
  font-family: var(--font-display);
}

.stat-label {
  font-size: 0.875rem;
  color: var(--text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

/* Hero Illustration */
.hero-illustration {
  position: relative;
  height: 500px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.recipe-card {
  position: absolute;
  width: 120px;
  height: 120px;
  background: white;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 4rem;
  animation: float 3s ease-in-out infinite;
}

.card-1 {
  top: 10%;
  left: 10%;
  animation-delay: 0s;
}

.card-2 {
  top: 50%;
  right: 10%;
  animation-delay: 1s;
}

.card-3 {
  bottom: 10%;
  left: 30%;
  animation-delay: 2s;
}

/* Features Section */
.features {
  padding: 6rem 0;
  background: var(--bg-secondary);
}

.section-title {
  text-align: center;
  margin-bottom: 1rem;
}

.section-description {
  text-align: center;
  font-size: 1.125rem;
  color: var(--text-secondary);
  margin-bottom: 4rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
}

.feature-card {
  background: white;
  padding: 2rem;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow);
  text-align: center;
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature-title {
  font-size: 1.25rem;
  margin-bottom: 0.75rem;
}

.feature-description {
  color: var(--text-secondary);
  line-height: 1.6;
}

/* CTA Section */
.cta {
  padding: 6rem 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.cta-content {
  text-align: center;
  max-width: 700px;
  margin: 0 auto;
}

.cta-title {
  color: white;
  margin-bottom: 1rem;
}

.cta-description {
  font-size: 1.25rem;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 2rem;
}

.cta-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  border: none;
  text-decoration: none;
}

.btn-lg {
  padding: 1rem 2rem;
  font-size: 1.125rem;
}

.btn-primary {
  background: var(--gradient-primary);
  color: white;
  box-shadow: var(--shadow);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.btn-secondary {
  background: white;
  color: var(--primary);
  box-shadow: var(--shadow);
}

.btn-secondary:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.btn-white {
  background: white;
  color: var(--primary);
}

.btn-white:hover {
  background: rgba(255, 255, 255, 0.95);
}

.btn-outline {
  background: transparent;
  color: white;
  border: 2px solid white;
}

.btn-outline:hover {
  background: white;
  color: var(--primary);
}

.btn-icon {
  font-size: 1.2em;
}

/* Responsive */
@media (max-width: 768px) {
  .hero .container {
    grid-template-columns: 1fr;
  }

  .hero-illustration {
    height: 300px;
  }

  .recipe-card {
    width: 80px;
    height: 80px;
    font-size: 2.5rem;
  }

  .hero-stats {
    gap: 2rem;
  }

  .features-grid {
    grid-template-columns: 1fr;
  }

  .cta-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>
