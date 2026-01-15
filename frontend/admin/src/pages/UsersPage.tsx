// Users Management Page
// Displays all users with pagination and search
// Uses TanStack Table for data display

import { useQuery } from '@tanstack/react-query'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
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

// User type
interface User {
  id: string
  username: string | null
  email: string
  balance: number
  isAdmin: boolean
  createdAt: string
}

/**
 * UsersPage component
 * Displays paginated list of all users with search functionality
 */
export function UsersPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const pageSize = 20

  // Fetch users with pagination and search
  const { data, isLoading } = useQuery({
    queryKey: ['users', page, search],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      })
      if (search) params.append('search', search)

      const response = await api.get(`/users/list?${params.toString()}`)
      return response.data
    },
  })

  const users: User[] = data?.users || []
  const pagination = data?.pagination

  // Handle search input with debounce
  const [searchInput, setSearchInput] = useState('')
  const handleSearch = () => {
    setSearch(searchInput)
    setPage(1) // Reset to first page on search
  }

  return (
    <div className="p-8">
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900">Users</h1>
        <p className="text-slate-600 mt-1">
          Manage users and view account information
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>All Users ({pagination?.totalUsers || 0})</CardTitle>
            <div className="flex gap-2">
              <Input
                placeholder="Search by username or email..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') handleSearch()
                }}
                className="w-64"
              />
              <Button onClick={handleSearch}>Search</Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8 text-muted-foreground">Loading...</div>
          ) : users.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No users found
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Username</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Balance</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead>Joined</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">
                        {user.username || '—'}
                      </TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>{user.balance} pts</TableCell>
                      <TableCell>
                        {user.isAdmin ? (
                          <Badge variant="default">Admin</Badge>
                        ) : (
                          <Badge variant="secondary">User</Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        {new Date(user.createdAt).toLocaleDateString()}
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
