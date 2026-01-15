// Dashboard Page - Main admin overview with real-time statistics
// Shows live metrics, recent activity, and quick access to management features
// Uses TanStack Query for data fetching and auto-refresh

import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import {
  Users,
  BookOpen,
  DollarSign,
  CreditCard,
  TrendingUp,
  ArrowUpRight,
  ArrowDownRight,
  Activity
} from 'lucide-react'

/**
 * DashboardPage component
 * Displays admin dashboard with live stats and recent activity
 */
export function DashboardPage() {
  const { user } = useAuth()
  const navigate = useNavigate()

  // Fetch dashboard analytics with auto-refresh every 30 seconds
  const { data: analytics, isLoading } = useQuery({
    queryKey: ['analytics', 'dashboard'],
    queryFn: async () => {
      const response = await api.get('/analytics/dashboard')
      return response.data
    },
    refetchInterval: 30000, // Refresh every 30 seconds
    staleTime: 10000, // Consider data stale after 10 seconds
  })

  // Format currency for display
  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('en-US').format(num)
  }

  // Format relative time for activity feed
  const formatRelativeTime = (dateString: string) => {
    const date = new Date(dateString)
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    const diffMins = Math.floor(diffMs / 60000)
    const diffHours = Math.floor(diffMins / 60)
    const diffDays = Math.floor(diffHours / 24)

    if (diffMins < 1) return 'Just now'
    if (diffMins < 60) return `${diffMins}m ago`
    if (diffHours < 24) return `${diffHours}h ago`
    return `${diffDays}d ago`
  }

  // Get transaction icon and color based on type
  const getTransactionBadge = (type: string) => {
    switch (type) {
      case 'Purchase':
        return <Badge variant="destructive" className="text-xs">Purchase</Badge>
      case 'Sale':
        return <Badge variant="default" className="text-xs">Sale</Badge>
      case 'TopUp':
        return <Badge variant="secondary" className="text-xs">Top-up</Badge>
      default:
        return <Badge variant="outline" className="text-xs">{type}</Badge>
    }
  }

  return (
    <div className="p-8">
      {/* Page header with live indicator */}
      <div className="mb-8">
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold text-slate-900">Dashboard</h1>
          {!isLoading && (
            <div className="flex items-center gap-1.5 text-xs text-green-600">
              <Activity className="h-3 w-3 animate-pulse" />
              Live
            </div>
          )}
        </div>
        <p className="text-slate-600 mt-1">Welcome back, {user?.username || user?.email}</p>
      </div>

      {/* Real-time Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
        {/* Total Users */}
        <Card>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardDescription>Total Users</CardDescription>
              <Users className="h-4 w-4 text-muted-foreground" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoading ? '...' : formatNumber(analytics?.overview?.totalUsers || 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Registered accounts
            </p>
          </CardContent>
        </Card>

        {/* Total Recipes */}
        <Card>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardDescription>Total Recipes</CardDescription>
              <BookOpen className="h-4 w-4 text-muted-foreground" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoading ? '...' : formatNumber(analytics?.overview?.totalRecipes || 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Available recipes
            </p>
          </CardContent>
        </Card>

        {/* Total Revenue */}
        <Card>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardDescription>Total Revenue</CardDescription>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoading ? '...' : `${formatNumber(analytics?.overview?.totalRevenue || 0)} pts`}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              From purchases
            </p>
          </CardContent>
        </Card>

        {/* Active Subscriptions */}
        <Card>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardDescription>Subscriptions</CardDescription>
              <CreditCard className="h-4 w-4 text-muted-foreground" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoading ? '...' : formatNumber(analytics?.overview?.activeSubscriptions || 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Currently active
            </p>
          </CardContent>
        </Card>

        {/* Transactions */}
        <Card>
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardDescription>Transactions</CardDescription>
              <TrendingUp className="h-4 w-4 text-muted-foreground" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoading ? '...' : formatNumber(analytics?.overview?.totalTransactions || 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              All time
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Two Column Layout: Recent Activity + Top Recipes */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Recent Activity Feed */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
            <CardDescription>Latest transactions in the system</CardDescription>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="text-center py-8 text-muted-foreground">Loading...</div>
            ) : analytics?.recentTransactions?.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">No recent activity</div>
            ) : (
              <div className="space-y-3">
                {analytics?.recentTransactions?.slice(0, 8).map((transaction: any) => (
                  <div
                    key={transaction.id}
                    className="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/50 transition-colors"
                  >
                    <div className="flex items-center gap-3 flex-1 min-w-0">
                      <div className="flex-shrink-0">
                        {transaction.amount > 0 ? (
                          <ArrowUpRight className="h-4 w-4 text-green-600" />
                        ) : (
                          <ArrowDownRight className="h-4 w-4 text-red-600" />
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-medium truncate">
                            {transaction.user?.username || 'Unknown User'}
                          </p>
                          {getTransactionBadge(transaction.type)}
                        </div>
                        <p className="text-xs text-muted-foreground">
                          {formatRelativeTime(transaction.createdAt)}
                        </p>
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <p className={`text-sm font-semibold ${
                        transaction.amount > 0 ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {transaction.amount > 0 ? '+' : ''}{transaction.amount} pts
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Top Recipes */}
        <Card>
          <CardHeader>
            <CardTitle>Popular Recipes</CardTitle>
            <CardDescription>Most viewed recipes</CardDescription>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="text-center py-8 text-muted-foreground">Loading...</div>
            ) : analytics?.topRecipes?.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">No recipes yet</div>
            ) : (
              <div className="space-y-3">
                {analytics?.topRecipes?.map((recipe: any, index: number) => (
                  <div
                    key={recipe.id}
                    className="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/50 transition-colors"
                  >
                    <div className="flex items-center gap-3 flex-1 min-w-0">
                      <div className="flex-shrink-0 w-6 h-6 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white text-xs font-bold">
                        {index + 1}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">{recipe.title}</p>
                        <p className="text-xs text-muted-foreground">
                          by {recipe.author?.username || 'Unknown'}
                        </p>
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <p className="text-sm font-semibold">{formatNumber(recipe.views)}</p>
                      <p className="text-xs text-muted-foreground">views</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Quick Access Cards */}
      <div>
        <h2 className="text-xl font-bold text-slate-900 mb-4">Quick Access</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/recipes')}>
            <CardHeader>
              <CardTitle className="text-base">Recipe Management</CardTitle>
              <CardDescription className="text-xs">
                View, edit, and moderate recipes
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary" size="sm">Manage Recipes →</Button>
            </CardContent>
          </Card>

          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/subscriptions')}>
            <CardHeader>
              <CardTitle className="text-base">Subscriptions</CardTitle>
              <CardDescription className="text-xs">
                Manage subscription plans
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary" size="sm">Manage Plans →</Button>
            </CardContent>
          </Card>

          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => navigate('/analytics')}>
            <CardHeader>
              <CardTitle className="text-base">Analytics</CardTitle>
              <CardDescription className="text-xs">
                View detailed statistics
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button variant="secondary" size="sm">View Analytics →</Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
