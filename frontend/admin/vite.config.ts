import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// Vite configuration for React admin panel
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  // Development server configuration
  server: {
    // Allow access from admin.recipes.local domain (proxied through nginx)
    host: '0.0.0.0', // Listen on all interfaces
    port: 5173,
    allowedHosts: ['admin.recipes.local', 'localhost', '127.0.0.1'],
  },

  // Path resolution for @ alias (allows imports like "@/lib/utils")
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
})
