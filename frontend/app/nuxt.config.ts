// Nuxt 3 configuration file
// https://nuxt.com/docs/api/configuration/nuxt-config

export default defineNuxtConfig({
  // Enable compatibility with Nuxt 4
  compatibilityDate: '2024-04-03',

  // Enable Nuxt DevTools for better DX
  devtools: { enabled: true },

  // TypeScript configuration
  typescript: {
    strict: true,
    typeCheck: false, // Disabled for faster dev server startup
  },

  // App configuration
  app: {
    head: {
      title: 'Recipes PWA',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Recipe trading PWA with points economy' },
      ],
    },
  },

  // Runtime config - accessible via useRuntimeConfig()
  // Public config is exposed to the client-side code
  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL || 'http://localhost:5010',
    },
  },

  // Enable PWA module
  modules: ['@vite-pwa/nuxt'],

  // PWA Configuration
  pwa: {
    // Register service worker type
    registerType: 'autoUpdate',

    // Manifest configuration
    manifest: {
      name: 'Recipes PWA',
      short_name: 'Recipes',
      description: 'Recipe trading app with points economy',
      theme_color: '#667eea',
      background_color: '#ffffff',
      display: 'standalone',
      start_url: '/',
      icons: [
        {
          src: '/icon-192x192.png',
          sizes: '192x192',
          type: 'image/png',
        },
        {
          src: '/icon-512x512.png',
          sizes: '512x512',
          type: 'image/png',
        },
        {
          src: '/icon-512x512.png',
          sizes: '512x512',
          type: 'image/png',
          purpose: 'any maskable',
        },
      ],
    },

    // Workbox configuration for caching
    workbox: {
      // Cache navigation requests (HTML pages)
      navigateFallback: '/',

      // Runtime caching strategies
      runtimeCaching: [
        {
          // Cache API responses with network-first strategy
          urlPattern: /^http:\/\/localhost:5010\/api\/.*/i,
          handler: 'NetworkFirst',
          options: {
            cacheName: 'api-cache',
            expiration: {
              maxEntries: 50,
              maxAgeSeconds: 60 * 60, // 1 hour
            },
            cacheableResponse: {
              statuses: [0, 200],
            },
          },
        },
        {
          // Cache images with cache-first strategy
          urlPattern: /\.(?:png|jpg|jpeg|svg|gif)$/,
          handler: 'CacheFirst',
          options: {
            cacheName: 'image-cache',
            expiration: {
              maxEntries: 100,
              maxAgeSeconds: 60 * 60 * 24 * 30, // 30 days
            },
          },
        },
      ],
    },

    // Development options
    devOptions: {
      enabled: true, // Enable PWA in development
      type: 'module',
    },
  },
})
