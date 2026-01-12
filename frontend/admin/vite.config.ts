import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// Vite configuration for React admin panel
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  // Path resolution for @ alias (allows imports like "@/lib/utils")
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
})
