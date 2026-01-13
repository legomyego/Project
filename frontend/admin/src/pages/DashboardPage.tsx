// Dashboard Page - Main admin overview
// Shows statistics, recent activity, and quick access to management features
// This is the landing page after login

import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useNavigate } from 'react-router-dom'

/**
 * DashboardPage component
 * Displays admin dashboard with stats and navigation
 */
export function DashboardPage() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  /**
   * Handle logout
   * Clears auth and redirects to login
   */
  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <header className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-slate-900">Admin Dashboard</h1>
              <p className="text-sm text-slate-600">Welcome, {user?.username || user?.email}</p>
            </div>
            <Button variant="outline" onClick={handleLogout}>
              Logout
            </Button>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Stats grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Your Balance</CardDescription>
              <CardTitle className="text-3xl">{user?.balance || 0} pts</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-xs text-muted-foreground">
                Admin account balance
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Recipes</CardDescription>
              <CardTitle className="text-3xl">—</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-xs text-muted-foreground">
                Coming soon
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Active Users</CardDescription>
              <CardTitle className="text-3xl">—</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-xs text-muted-foreground">
                Coming soon
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Pending Trades</CardDescription>
              <CardTitle className="text-3xl">—</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-xs text-muted-foreground">
                Coming soon
              </p>
            </CardContent>
          </Card>
        </div>

        {/* Navigation cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/recipes')}>
            <CardHeader>
              <CardTitle>Recipe Management</CardTitle>
              <CardDescription>
                View, edit, and moderate recipes in the system
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary">Manage Recipes →</Button>
            </CardContent>
          </Card>

          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/users')}>
            <CardHeader>
              <CardTitle>User Management</CardTitle>
              <CardDescription>
                View users, manage accounts, and monitor activity
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary">Manage Users →</Button>
            </CardContent>
          </Card>

          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/trades')}>
            <CardHeader>
              <CardTitle>Trade Overview</CardTitle>
              <CardDescription>
                Monitor recipe trades and transaction history
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary">View Trades →</Button>
            </CardContent>
          </Card>

          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/analytics')}>
            <CardHeader>
              <CardTitle>Analytics</CardTitle>
              <CardDescription>
                View detailed statistics and reports
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary">View Analytics →</Button>
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  )
}
