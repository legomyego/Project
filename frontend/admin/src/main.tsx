// Main entry point for the React Admin Panel
// Sets up all providers: Router, TanStack Query, Auth

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from './contexts/AuthContext'
import './index.css'
import App from './App.tsx'

// Create TanStack Query client
// Handles API data caching and state management
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Retry failed requests once
      retry: 1,
      // Refetch on window focus for fresh data
      refetchOnWindowFocus: false,
      // Cache for 5 minutes by default
      staleTime: 5 * 60 * 1000,
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {/* Router provider - enables navigation */}
    <BrowserRouter>
      {/* TanStack Query provider - enables data fetching hooks */}
      <QueryClientProvider client={queryClient}>
        {/* Auth provider - enables authentication across app */}
        <AuthProvider>
          <App />
        </AuthProvider>
      </QueryClientProvider>
    </BrowserRouter>
  </StrictMode>,
)
