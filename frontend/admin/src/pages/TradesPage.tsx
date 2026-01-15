// Trades Overview Page
// Displays all trade offers with filtering and pagination
// Shows both users and recipes involved in each trade

import { useQuery } from '@tanstack/react-query'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { api } from '@/lib/api'
import { useState } from 'react'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Badge } from '@/components/ui/badge'

// Trade type
interface Trade {
  id: string
  offeringUser: {
    id: string
    username: string | null
    email: string
  }
  offeredRecipe: {
    id: string
    title: string
  }
  requestedUser: {
    id: string
    username: string | null
    email: string
  }
  requestedRecipe: {
    id: string
    title: string
  }
  status: string
  createdAt: string
  updatedAt: string
}

/**
 * TradesPage component
 * Displays paginated list of all trades with status filtering
 */
export function TradesPage() {
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<string | null>(null)
  const pageSize = 20

  // Fetch trades with pagination and status filter
  const { data, isLoading } = useQuery({
    queryKey: ['trades', 'all', page, statusFilter],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      })
      if (statusFilter) params.append('status', statusFilter)

      const response = await api.get(`/trades/all?${params.toString()}`)
      return response.data
    },
  })

  const trades: Trade[] = data?.trades || []
  const pagination = data?.pagination

  // Status badge variant helper
  const getStatusVariant = (status: string): 'default' | 'secondary' | 'destructive' | 'outline' => {
    switch (status) {
      case 'Accepted':
        return 'default'
      case 'Pending':
        return 'secondary'
      case 'Declined':
      case 'Cancelled':
        return 'outline'
      default:
        return 'secondary'
    }
  }

  return (
    <div className="p-8">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900">Trades</h1>
        <p className="text-slate-600 mt-1">
          Monitor recipe trades between users
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>All Trades ({pagination?.totalTrades || 0})</CardTitle>
            <div className="flex gap-2">
              <Button
                variant={statusFilter === null ? 'default' : 'outline'}
                size="sm"
                onClick={() => {
                  setStatusFilter(null)
                  setPage(1)
                }}
              >
                All
              </Button>
              <Button
                variant={statusFilter === 'Pending' ? 'default' : 'outline'}
                size="sm"
                onClick={() => {
                  setStatusFilter('Pending')
                  setPage(1)
                }}
              >
                Pending
              </Button>
              <Button
                variant={statusFilter === 'Accepted' ? 'default' : 'outline'}
                size="sm"
                onClick={() => {
                  setStatusFilter('Accepted')
                  setPage(1)
                }}
              >
                Accepted
              </Button>
              <Button
                variant={statusFilter === 'Declined' ? 'default' : 'outline'}
                size="sm"
                onClick={() => {
                  setStatusFilter('Declined')
                  setPage(1)
                }}
              >
                Declined
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8 text-muted-foreground">Loading...</div>
          ) : trades.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No trades found
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Offering User</TableHead>
                    <TableHead>Offered Recipe</TableHead>
                    <TableHead>↔</TableHead>
                    <TableHead>Requested User</TableHead>
                    <TableHead>Requested Recipe</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Created</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {trades.map((trade) => (
                    <TableRow key={trade.id}>
                      <TableCell>
                        <div className="font-medium">
                          {trade.offeringUser.username || '—'}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {trade.offeringUser.email}
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">
                        {trade.offeredRecipe.title}
                      </TableCell>
                      <TableCell className="text-center">⇄</TableCell>
                      <TableCell>
                        <div className="font-medium">
                          {trade.requestedUser.username || '—'}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {trade.requestedUser.email}
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">
                        {trade.requestedRecipe.title}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getStatusVariant(trade.status)}>
                          {trade.status}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {new Date(trade.createdAt).toLocaleDateString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Pagination */}
              {pagination && pagination.totalPages > 1 && (
                <div className="flex items-center justify-between mt-4">
                  <div className="text-sm text-muted-foreground">
                    Page {pagination.page} of {pagination.totalPages}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => p + 1)}
                      disabled={page >= pagination.totalPages}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
