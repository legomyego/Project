// Recipes Management Page
// Displays all recipes in a table with sorting, filtering, and pagination
// Uses TanStack Table for advanced table functionality
// Uses TanStack Query for data fetching and caching

import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

/**
 * RecipesPage component
 * Main page for managing all recipes
 */
export function RecipesPage() {
  const queryClient = useQueryClient()

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1)
  const pageSize = 20

  // Sorting state for table
  const [sorting, setSorting] = useState<SortingState>([])

  // Dialog state for create recipe modal
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)

  // Dialog state for edit recipe modal
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [editingRecipe, setEditingRecipe] = useState<Recipe | null>(null)

  /**
   * React Hook Form for recipe creation
   * Manages form state, validation, and submission
   */
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<{
    title: string
    description: string
    price: number
  }>()

  /**
   * React Hook Form for recipe editing
   * Separate form instance to avoid state conflicts
   */
  const {
    register: registerEdit,
    handleSubmit: handleSubmitEdit,
    formState: { errors: errorsEdit },
    reset: resetEdit,
    setValue: setValueEdit,
  } = useForm<{
    title: string
    description: string
    price: number
    requiresSubscription: boolean
  }>()

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
   * Mutation for creating a new recipe
   * Uses TanStack Query for optimistic updates and cache management
   */
  const createMutation = useMutation({
    // Mutation function calls the API
    mutationFn: api.recipes.create,

    // On success: refetch recipes list and close dialog
    onSuccess: () => {
      // Invalidate the recipes query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['recipes'] })

      // Close the dialog
      setIsCreateDialogOpen(false)

      // Reset form to initial state
      reset()
    },
  })

  /**
   * Mutation for updating a recipe
   */
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => api.recipes.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] })
      setIsEditDialogOpen(false)
      setEditingRecipe(null)
      resetEdit()
    },
    onError: (error: Error) => {
      alert(`Failed to update recipe: ${error.message}`)
    },
  })

  /**
   * Mutation for deleting a recipe
   */
  const deleteMutation = useMutation({
    mutationFn: api.recipes.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] })
    },
    onError: (error: Error) => {
      alert(`Failed to delete recipe: ${error.message}`)
    },
  })

  /**
   * Handle form submission
   * Called when user submits the create recipe form
   */
  const onSubmit = handleSubmit((data) => {
    createMutation.mutate(data)
  })

  /**
   * Handle edit button click
   * Opens edit dialog and populates form with recipe data
   */
  const handleEdit = (recipe: Recipe) => {
    setEditingRecipe(recipe)
    setValueEdit('title', recipe.title)
    setValueEdit('description', recipe.description)
    setValueEdit('price', recipe.price)
    setValueEdit('requiresSubscription', recipe.requiresSubscription || false)
    setIsEditDialogOpen(true)
  }

  /**
   * Handle edit form submission
   */
  const onSubmitEdit = handleSubmitEdit((data) => {
    if (!editingRecipe) return
    updateMutation.mutate({
      id: editingRecipe.id,
      data: {
        title: data.title,
        description: data.description,
        price: data.price,
        requiresSubscription: data.requiresSubscription,
      },
    })
  })

  /**
   * Handle delete button click
   */
  const handleDelete = (id: string, title: string) => {
    if (confirm(`Are you sure you want to delete "${title}"?`)) {
      deleteMutation.mutate(id)
    }
  }

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
      cell: ({ row }) => {
        const recipe = row.original
        return (
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleEdit(recipe)}
            >
              Edit
            </Button>
            <Button
              variant="destructive"
              size="sm"
              onClick={() => handleDelete(recipe.id, recipe.title)}
              disabled={deleteMutation.isPending}
            >
              Delete
            </Button>
          </div>
        )
      },
    },
  ]

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

  return (
    <div className="p-8">
      {/* Page Title */}
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-slate-900">Recipe Management</h1>
            <p className="text-sm text-slate-600 mt-1">
              {data?.pagination.totalCount || 0} total recipes
            </p>
          </div>

          {/* Add Recipe Button with Dialog */}
          <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
            <DialogTrigger asChild>
              <Button>Add Recipe</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Create New Recipe</DialogTitle>
                <DialogDescription>
                  Add a new recipe to the system. Fill in all the details below.
                </DialogDescription>
              </DialogHeader>

              {/* Recipe Creation Form */}
              <form onSubmit={onSubmit} className="space-y-4">
                {/* Title Field */}
                <div className="space-y-2">
                  <Label htmlFor="title">Title</Label>
                  <Input
                    id="title"
                    {...register('title', { required: 'Title is required' })}
                    placeholder="Recipe title"
                  />
                  {errors.title && (
                    <p className="text-sm text-red-500">{errors.title.message}</p>
                  )}
                </div>

                {/* Description Field */}
                <div className="space-y-2">
                  <Label htmlFor="description">Description</Label>
                  <Input
                    id="description"
                    {...register('description', { required: 'Description is required' })}
                    placeholder="Recipe description"
                  />
                  {errors.description && (
                    <p className="text-sm text-red-500">{errors.description.message}</p>
                  )}
                </div>

                {/* Price Field */}
                <div className="space-y-2">
                  <Label htmlFor="price">Price (points)</Label>
                  <Input
                    id="price"
                    type="number"
                    step="0.01"
                    {...register('price', {
                      required: 'Price is required',
                      min: { value: 0, message: 'Price cannot be negative' },
                      valueAsNumber: true,
                    })}
                    placeholder="0.00"
                  />
                  {errors.price && (
                    <p className="text-sm text-red-500">{errors.price.message}</p>
                  )}
                </div>

                {/* Error Message from API */}
                {createMutation.isError && (
                  <div className="text-sm text-red-500">
                    Error: {(createMutation.error as Error).message}
                  </div>
                )}

                <DialogFooter>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setIsCreateDialogOpen(false)}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Creating...' : 'Create Recipe'}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Main content */}
      <div>
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
      </div>

      {/* Edit Recipe Dialog */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Recipe</DialogTitle>
            <DialogDescription>
              Update recipe details. Click save when you're done.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={onSubmitEdit} className="space-y-4">
            <div>
              <Label htmlFor="edit-title">Title</Label>
              <Input
                id="edit-title"
                {...registerEdit('title', { required: 'Title is required' })}
                placeholder="Recipe title"
              />
              {errorsEdit.title && (
                <p className="text-sm text-destructive mt-1">{errorsEdit.title.message}</p>
              )}
            </div>

            <div>
              <Label htmlFor="edit-description">Description</Label>
              <Input
                id="edit-description"
                {...registerEdit('description', { required: 'Description is required' })}
                placeholder="Recipe description"
              />
              {errorsEdit.description && (
                <p className="text-sm text-destructive mt-1">{errorsEdit.description.message}</p>
              )}
            </div>

            <div>
              <Label htmlFor="edit-price">Price (points)</Label>
              <Input
                id="edit-price"
                type="number"
                step="0.01"
                {...registerEdit('price', {
                  required: 'Price is required',
                  min: { value: 0, message: 'Price must be positive' },
                })}
                placeholder="0.00"
              />
              {errorsEdit.price && (
                <p className="text-sm text-destructive mt-1">{errorsEdit.price.message}</p>
              )}
            </div>

            <div className="flex items-center gap-2">
              <input
                id="edit-requires-subscription"
                type="checkbox"
                {...registerEdit('requiresSubscription')}
              />
              <Label htmlFor="edit-requires-subscription" className="cursor-pointer">
                Requires Subscription
              </Label>
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setIsEditDialogOpen(false)
                  setEditingRecipe(null)
                  resetEdit()
                }}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={updateMutation.isPending}>
                {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
