// Analytics Page - Advanced analytics with charts
// Shows user growth, revenue trends, and transaction analytics
// Uses TanStack Query for data fetching and Recharts for visualization

import { useQuery } from '@tanstack/react-query'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { api } from '@/lib/api'
import { useState } from 'react'
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'

// Period type for analytics
type Period = 'daily' | 'weekly' | 'monthly'

/**
 * AnalyticsPage component
 * Displays comprehensive analytics with interactive charts
 */
export function AnalyticsPage() {
  // State for selected time periods
  const [userGrowthPeriod, setUserGrowthPeriod] = useState<Period>('daily')
  const [revenuePeriod, setRevenuePeriod] = useState<Period>('daily')
  const [transactionPeriod, setTransactionPeriod] = useState<Period>('daily')

  // Fetch user growth analytics
  const { data: userGrowth, isLoading: isLoadingUserGrowth } = useQuery({
    queryKey: ['analytics', 'user-growth', userGrowthPeriod],
    queryFn: async () => {
      const response = await api.get(`/analytics/user-growth?period=${userGrowthPeriod}&days=30`)
      return response.data
    },
  })

  // Fetch revenue analytics
  const { data: revenue, isLoading: isLoadingRevenue } = useQuery({
    queryKey: ['analytics', 'revenue', revenuePeriod],
    queryFn: async () => {
      const response = await api.get(`/analytics/revenue?period=${revenuePeriod}&days=30`)
      return response.data
    },
  })

  // Fetch transaction trends
  const { data: transactions, isLoading: isLoadingTransactions } = useQuery({
    queryKey: ['analytics', 'transactions', transactionPeriod],
    queryFn: async () => {
      const response = await api.get(`/analytics/transactions?period=${transactionPeriod}&days=30`)
      return response.data
    },
  })

  // Period selector component
  const PeriodSelector = ({
    value,
    onChange,
  }: {
    value: Period
    onChange: (period: Period) => void
  }) => (
    <div className="flex gap-2">
      {(['daily', 'weekly', 'monthly'] as Period[]).map((period) => (
        <Button
          key={period}
          variant={value === period ? 'default' : 'outline'}
          size="sm"
          onClick={() => onChange(period)}
          className="capitalize"
        >
          {period}
        </Button>
      ))}
    </div>
  )

  return (
    <div className="p-8">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900">Analytics</h1>
        <p className="text-slate-600 mt-1">Detailed insights and trends</p>
      </div>

      {/* User Growth Chart */}
      <Card className="mb-8">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>User Growth</CardTitle>
              <CardDescription>New user registrations over time</CardDescription>
            </div>
            <PeriodSelector value={userGrowthPeriod} onChange={setUserGrowthPeriod} />
          </div>
        </CardHeader>
        <CardContent>
          {isLoadingUserGrowth ? (
            <div className="h-80 flex items-center justify-center text-muted-foreground">
              Loading...
            </div>
          ) : (
            <>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={userGrowth?.data || []}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="count"
                    name="New Users"
                    stroke="#6366f1"
                    strokeWidth={2}
                  />
                  <Line
                    type="monotone"
                    dataKey="cumulative"
                    name="Total Users"
                    stroke="#8b5cf6"
                    strokeWidth={2}
                  />
                </LineChart>
              </ResponsiveContainer>
              <div className="mt-4 text-sm text-muted-foreground">
                Total new users: <span className="font-semibold">{userGrowth?.totalNewUsers}</span>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {/* Revenue Chart */}
      <Card className="mb-8">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Revenue</CardTitle>
              <CardDescription>Revenue from purchases and subscriptions</CardDescription>
            </div>
            <PeriodSelector value={revenuePeriod} onChange={setRevenuePeriod} />
          </div>
        </CardHeader>
        <CardContent>
          {isLoadingRevenue ? (
            <div className="h-80 flex items-center justify-center text-muted-foreground">
              Loading...
            </div>
          ) : (
            <>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={revenue?.data || []}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="revenue" name="Revenue" fill="#10b981" />
                </BarChart>
              </ResponsiveContainer>
              <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Total Revenue:</span>{' '}
                  <span className="font-semibold">{revenue?.totalRevenue} pts</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Transactions:</span>{' '}
                  <span className="font-semibold">{revenue?.totalTransactions}</span>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {/* Transaction Trends Chart */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Transaction Trends</CardTitle>
              <CardDescription>All transaction types over time</CardDescription>
            </div>
            <PeriodSelector value={transactionPeriod} onChange={setTransactionPeriod} />
          </div>
        </CardHeader>
        <CardContent>
          {isLoadingTransactions ? (
            <div className="h-80 flex items-center justify-center text-muted-foreground">
              Loading...
            </div>
          ) : (
            <>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={transactions?.data || []}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="purchases" name="Purchases" fill="#ef4444" stackId="a" />
                  <Bar dataKey="sales" name="Sales" fill="#10b981" stackId="a" />
                  <Bar dataKey="topUps" name="Top-ups" fill="#3b82f6" stackId="a" />
                </BarChart>
              </ResponsiveContainer>
              <div className="mt-4 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Total:</span>{' '}
                  <span className="font-semibold">{transactions?.summary?.totalTransactions}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Purchases:</span>{' '}
                  <span className="font-semibold">{transactions?.summary?.totalPurchases}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Sales:</span>{' '}
                  <span className="font-semibold">{transactions?.summary?.totalSales}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Top-ups:</span>{' '}
                  <span className="font-semibold">{transactions?.summary?.totalTopUps}</span>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
