// Recipes Management Page
// Displays all recipes in a table with sorting, filtering, and pagination
// Uses TanStack Table for advanced table functionality
// Uses TanStack Query for data fetching and caching

import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getPaginationRowModel,
  type ColumnDef,
  type SortingState,
  flexRender,
} from '@tanstack/react-table'
import { api, type Recipe } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/AuthContext'

/**
 * Column definitions for the recipes table
 * Defines what data to show and how to render it
 */
const columns: ColumnDef<Recipe>[] = [
  {
    accessorKey: 'title',
    header: 'Title',
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue('title')}</div>
    ),
  },
  {
    accessorKey: 'description',
    header: 'Description',
    cell: ({ row }) => (
      <div className="max-w-xs truncate text-muted-foreground">
        {row.getValue('description') || 'No description'}
      </div>
    ),
  },
  {
    accessorKey: 'author',
    header: 'Author',
    cell: ({ row }) => {
      const author = row.getValue('author') as Recipe['author']
      return <div>{author.username}</div>
    },
  },
  {
    accessorKey: 'price',
    header: 'Price',
    cell: ({ row }) => (
      <div className="font-mono">{row.getValue('price')} pts</div>
    ),
  },
  {
    accessorKey: 'views',
    header: 'Views',
    cell: ({ row }) => (
      <div className="text-muted-foreground">{row.getValue('views')}</div>
    ),
  },
  {
    accessorKey: 'createdAt',
    header: 'Created',
    cell: ({ row }) => {
      const date = new Date(row.getValue('createdAt'))
      return (
        <div className="text-sm text-muted-foreground">
          {date.toLocaleDateString()}
        </div>
      )
    },
  },
  {
    id: 'actions',
    header: 'Actions',
    cell: ({ row }) => (
      <div className="flex gap-2">
        <Button variant="outline" size="sm">
          Edit
        </Button>
        <Button variant="destructive" size="sm">
          Delete
        </Button>
      </div>
    ),
  },
]

/**
 * RecipesPage component
 * Main page for managing all recipes
 */
export function RecipesPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1)
  const pageSize = 20

  // Sorting state for table
  const [sorting, setSorting] = useState<SortingState>([])

  /**
   * Fetch recipes using TanStack Query
   * Automatically handles loading, error, caching, and refetching
   */
  const { data, isLoading, error } = useQuery({
    // Query key includes page number for automatic refetch on page change
    queryKey: ['recipes', currentPage],

    // Query function fetches recipes from API
    queryFn: () => api.recipes.getAll(currentPage, pageSize),

    // Keep previous data while fetching new page (prevents loading flicker)
    placeholderData: (previousData) => previousData,

    // Cache for 5 minutes
    staleTime: 5 * 60 * 1000,
  })

  /**
   * Initialize TanStack Table
   * Provides sorting, pagination, and row selection
   */
  const table = useReactTable({
    data: data?.recipes || [],
    columns,

    // Core table functionality
    getCoreRowModel: getCoreRowModel(),

    // Sorting
    onSortingChange: setSorting,
    getSortedRowModel: getSortedRowModel(),

    // Pagination (client-side, for UX - server-side is handled by currentPage)
    getPaginationRowModel: getPaginationRowModel(),

    // State
    state: {
      sorting,
    },

    // Manual pagination - we control it via currentPage state
    manualPagination: true,
    pageCount: data?.pagination.totalPages || 0,
  })

  /**
   * Handle logout
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
              <h1 className="text-2xl font-bold text-slate-900">Recipe Management</h1>
              <p className="text-sm text-slate-600">
                {data?.pagination.totalCount || 0} total recipes
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => navigate('/dashboard')}>
                ← Dashboard
              </Button>
              <Button variant="outline" onClick={handleLogout}>
                Logout
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Card>
          <CardHeader>
            <CardTitle>All Recipes</CardTitle>
            <CardDescription>
              View and manage all recipes in the system
            </CardDescription>
          </CardHeader>
          <CardContent>
            {/* Loading state */}
            {isLoading && (
              <div className="text-center py-8 text-muted-foreground">
                Loading recipes...
              </div>
            )}

            {/* Error state */}
            {error && (
              <div className="text-center py-8 text-red-500">
                Error loading recipes: {(error as Error).message}
              </div>
            )}

            {/* Table */}
            {!isLoading && !error && (
              <>
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      {table.getHeaderGroups().map((headerGroup) => (
                        <TableRow key={headerGroup.id}>
                          {headerGroup.headers.map((header) => (
                            <TableHead key={header.id}>
                              {header.isPlaceholder
                                ? null
                                : flexRender(
                                    header.column.columnDef.header,
                                    header.getContext()
                                  )}
                            </TableHead>
                          ))}
                        </TableRow>
                      ))}
                    </TableHeader>
                    <TableBody>
                      {table.getRowModel().rows?.length ? (
                        table.getRowModel().rows.map((row) => (
                          <TableRow
                            key={row.id}
                            data-state={row.getIsSelected() && 'selected'}
                          >
                            {row.getVisibleCells().map((cell) => (
                              <TableCell key={cell.id}>
                                {flexRender(
                                  cell.column.columnDef.cell,
                                  cell.getContext()
                                )}
                              </TableCell>
                            ))}
                          </TableRow>
                        ))
                      ) : (
                        <TableRow>
                          <TableCell
                            colSpan={columns.length}
                            className="h-24 text-center"
                          >
                            No recipes found
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>

                {/* Pagination */}
                <div className="flex items-center justify-between mt-4">
                  <div className="text-sm text-muted-foreground">
                    Page {currentPage} of {data?.pagination.totalPages || 1}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                      disabled={currentPage === 1}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setCurrentPage(p => p + 1)}
                      disabled={currentPage >= (data?.pagination.totalPages || 1)}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              </>
            )}
          </CardContent>
        </Card>
      </main>
    </div>
  )
}
