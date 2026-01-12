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

  // Modules will be added here
  // modules: ['@vite-pwa/nuxt'],
})
