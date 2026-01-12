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

  // Modules will be added here
  // modules: ['@vite-pwa/nuxt'],
})
