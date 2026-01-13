// Custom hook for handling logout across the admin panel
// Consolidates logout logic to avoid duplication in placeholder pages

import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/AuthContext'

/**
 * Custom hook that provides a unified logout handler
 * Logs out the user and redirects to the login page
 */
export const useLogout = () => {
  const navigate = useNavigate()
  const { logout } = useAuth()

  /**
   * Handle logout action
   * Calls the auth context logout method and navigates to login page
   */
  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return { handleLogout }
}
