<template>
  <div>
    <!-- Loading Screen - shows until all resources are loaded -->
    <!-- The .loaded class is added after all resources finish loading -->
    <div class="loading-screen" :class="{ loaded: !isLoading }">
      <div class="loader-container">
        <!-- Animated Logo/Icon -->
        <div class="loader-icon">
          <svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
            <circle cx="50" cy="50" r="45" class="loader-circle" />
            <path d="M50 20 L65 40 L50 35 L35 40 Z" class="loader-chef-hat" />
            <circle cx="50" cy="55" r="20" class="loader-plate" />
          </svg>
        </div>

        <!-- Loading Text -->
        <div class="loader-text">
          <h2>Recipes PWA</h2>
          <div class="loader-dots">
            <span>.</span><span>.</span><span>.</span>
          </div>
        </div>

        <!-- Progress Bar -->
        <div class="loader-progress">
          <div class="loader-progress-bar" :style="{ width: loadingProgress + '%' }"></div>
        </div>

        <p class="loader-subtitle">Loading delicious recipes</p>
      </div>
    </div>

    <!-- Main App Content -->
    <NuxtPage />
  </div>
</template>

<script setup lang="ts">
// Root component with loading screen
// Shows loader until all resources are loaded (images, fonts, styles)

const isLoading = ref(true)
const loadingProgress = ref(0)

onMounted(() => {
  // Hide initial HTML loader (instant loader before Vue initialization)
  const initialLoader = document.getElementById('initial-loader')
  if (initialLoader) {
    initialLoader.classList.add('hidden')
    setTimeout(() => {
      initialLoader.remove()
    }, 300)
  }

  // Simulate loading progress
  const progressInterval = setInterval(() => {
    if (loadingProgress.value < 90) {
      loadingProgress.value += Math.random() * 15
    }
  }, 100)

  // Wait for all resources to load
  // This includes images, fonts, stylesheets, etc.
  if (document.readyState === 'complete') {
    // Page already loaded
    finishLoading(progressInterval)
  } else {
    // Wait for load event
    window.addEventListener('load', () => {
      finishLoading(progressInterval)
    })
  }
})

function finishLoading(interval: ReturnType<typeof setInterval>) {
  // Complete the progress bar
  loadingProgress.value = 100
  clearInterval(interval)

  // Hide loader with smooth fade out after brief delay
  setTimeout(() => {
    isLoading.value = false
  }, 300)
}
</script>

<style scoped>
/* Loading Screen Styles */
.loading-screen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  opacity: 1;
  visibility: visible;
  transition: opacity 0.3s ease-out, visibility 0.3s ease-out;
}

/* Hide loader when loaded */
.loading-screen.loaded {
  opacity: 0;
  visibility: hidden;
  pointer-events: none;
}

.loader-container {
  text-align: center;
  color: white;
}

/* Animated Icon */
.loader-icon {
  width: 120px;
  height: 120px;
  margin: 0 auto 2rem;
  animation: float 3s ease-in-out infinite;
}

.loader-icon svg {
  width: 100%;
  height: 100%;
  filter: drop-shadow(0 10px 30px rgba(0, 0, 0, 0.3));
}

.loader-circle {
  fill: none;
  stroke: white;
  stroke-width: 3;
  stroke-dasharray: 283;
  stroke-dashoffset: 283;
  animation: drawCircle 2s ease-in-out infinite;
}

.loader-chef-hat {
  fill: white;
  opacity: 0;
  animation: fadeInOut 2s ease-in-out infinite;
}

.loader-plate {
  fill: white;
  opacity: 0.8;
  animation: pulse 2s ease-in-out infinite;
}

/* Loading Text */
.loader-text h2 {
  font-size: 2rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
  letter-spacing: 1px;
}

.loader-dots {
  display: inline-block;
  margin-left: 0.5rem;
}

.loader-dots span {
  animation: blink 1.4s infinite;
  font-size: 2rem;
  line-height: 1;
}

.loader-dots span:nth-child(2) {
  animation-delay: 0.2s;
}

.loader-dots span:nth-child(3) {
  animation-delay: 0.4s;
}

/* Progress Bar */
.loader-progress {
  width: 300px;
  height: 4px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 2px;
  margin: 2rem auto 1rem;
  overflow: hidden;
}

.loader-progress-bar {
  height: 100%;
  background: white;
  border-radius: 2px;
  transition: width 0.3s ease;
  box-shadow: 0 0 10px rgba(255, 255, 255, 0.5);
}

.loader-subtitle {
  font-size: 0.9rem;
  opacity: 0.9;
  margin-top: 1rem;
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-20px);
  }
}

@keyframes drawCircle {
  0% {
    stroke-dashoffset: 283;
  }
  50% {
    stroke-dashoffset: 0;
  }
  100% {
    stroke-dashoffset: -283;
  }
}

@keyframes fadeInOut {
  0%, 100% {
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 0.8;
  }
  50% {
    transform: scale(1.1);
    opacity: 1;
  }
}

@keyframes blink {
  0%, 100% {
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
}

/* Responsive */
@media (max-width: 640px) {
  .loader-icon {
    width: 100px;
    height: 100px;
  }

  .loader-text h2 {
    font-size: 1.5rem;
  }

  .loader-progress {
    width: 250px;
  }
}
</style>
