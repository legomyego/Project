// Subscriptions Management Page
// Allows admins to create, edit, and manage subscription plans
// Includes ability to add/remove recipes from subscriptions

import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/AuthContext'
import { api, type Subscription } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
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
 * SubscriptionsPage component
 * Main page for managing subscription plans
 */
export function SubscriptionsPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const queryClient = useQueryClient()

  // Dialog states
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [editingSubscription, setEditingSubscription] = useState<Subscription | null>(null)

  /**
   * React Hook Form for subscription creation
   * Manages form state, validation, and submission
   */
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<{
    name: string
    description?: string
    durationDays: number
    price: number
  }>()

  /**
   * React Hook Form for subscription editing
   */
  const {
    register: registerEdit,
    handleSubmit: handleSubmitEdit,
    formState: { errors: errorsEdit },
    reset: resetEdit,
    setValue: setValueEdit,
  } = useForm<{
    name: string
    description?: string
    durationDays: number
    price: number
    isActive: boolean
  }>()

  /**
   * Fetch subscriptions using TanStack Query
   * Automatically handles loading, error, caching, and refetching
   */
  const { data: subscriptions, isLoading, error } = useQuery({
    queryKey: ['subscriptions'],
    queryFn: () => api.subscriptions.getAll(),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })

  /**
   * Mutation for creating a new subscription
   * Uses TanStack Query for optimistic updates and cache management
   */
  const createMutation = useMutation({
    mutationFn: api.subscriptions.create,
    onSuccess: () => {
      // Invalidate the subscriptions query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
      // Close the dialog
      setIsCreateDialogOpen(false)
      // Reset form to initial state
      reset()
    },
  })

  /**
   * Mutation for updating a subscription
   */
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => api.subscriptions.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
      setIsEditDialogOpen(false)
      setEditingSubscription(null)
      resetEdit()
    },
  })

  /**
   * Mutation for deleting (deactivating) a subscription
   */
  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.subscriptions.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
    },
  })

  /**
   * Handle form submission
   * Called when user submits the create subscription form
   */
  const onSubmit = handleSubmit((data) => {
    createMutation.mutate(data)
  })

  /**
   * Handle edit form submission
   */
  const onEditSubmit = handleSubmitEdit((data) => {
    if (editingSubscription) {
      updateMutation.mutate({ id: editingSubscription.id, data })
    }
  })

  /**
   * Open edit dialog with subscription data
   */
  const handleEdit = (subscription: Subscription) => {
    setEditingSubscription(subscription)
    setValueEdit('name', subscription.name)
    setValueEdit('description', subscription.description || '')
    setValueEdit('durationDays', subscription.durationDays)
    setValueEdit('price', subscription.price)
    setValueEdit('isActive', subscription.isActive)
    setIsEditDialogOpen(true)
  }

  /**
   * Handle logout
   */
  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  /**
   * Handle delete subscription
   */
  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to deactivate this subscription?')) {
      deleteMutation.mutate(id)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <header className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-slate-900">Subscription Management</h1>
              <p className="text-sm text-slate-600">
                {subscriptions?.length || 0} subscription plans
              </p>
            </div>
            <div className="flex gap-2">
              {/* Add Subscription Button with Dialog */}
              <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
                <DialogTrigger asChild>
                  <Button>Add Subscription</Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Create New Subscription</DialogTitle>
                    <DialogDescription>
                      Add a new subscription plan to the system. Set duration and price.
                    </DialogDescription>
                  </DialogHeader>

                  {/* Subscription Creation Form */}
                  <form onSubmit={onSubmit} className="space-y-4">
                    {/* Name Field */}
                    <div className="space-y-2">
                      <Label htmlFor="name">Name</Label>
                      <Input
                        id="name"
                        {...register('name', { required: 'Name is required' })}
                        placeholder="e.g., Weekly Pass"
                      />
                      {errors.name && (
                        <p className="text-sm text-red-500">{errors.name.message}</p>
                      )}
                    </div>

                    {/* Description Field */}
                    <div className="space-y-2">
                      <Label htmlFor="description">Description (optional)</Label>
                      <Input
                        id="description"
                        {...register('description')}
                        placeholder="Access to premium recipes"
                      />
                    </div>

                    {/* Duration Field */}
                    <div className="space-y-2">
                      <Label htmlFor="durationDays">Duration (days)</Label>
                      <Input
                        id="durationDays"
                        type="number"
                        {...register('durationDays', {
                          required: 'Duration is required',
                          min: { value: 1, message: 'Duration must be at least 1 day' },
                          valueAsNumber: true,
                        })}
                        placeholder="7"
                      />
                      {errors.durationDays && (
                        <p className="text-sm text-red-500">{errors.durationDays.message}</p>
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
                        placeholder="100.00"
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
                        {createMutation.isPending ? 'Creating...' : 'Create Subscription'}
                      </Button>
                    </DialogFooter>
                  </form>
                </DialogContent>
              </Dialog>

              {/* Edit Subscription Dialog */}
              <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Edit Subscription</DialogTitle>
                    <DialogDescription>
                      Update subscription details. Changes will affect future purchases only.
                    </DialogDescription>
                  </DialogHeader>

                  {/* Subscription Edit Form */}
                  <form onSubmit={onEditSubmit} className="space-y-4">
                    {/* Name Field */}
                    <div className="space-y-2">
                      <Label htmlFor="edit-name">Name</Label>
                      <Input
                        id="edit-name"
                        {...registerEdit('name', { required: 'Name is required' })}
                        placeholder="e.g., Weekly Pass"
                      />
                      {errorsEdit.name && (
                        <p className="text-sm text-red-500">{errorsEdit.name.message}</p>
                      )}
                    </div>

                    {/* Description Field */}
                    <div className="space-y-2">
                      <Label htmlFor="edit-description">Description (optional)</Label>
                      <Input
                        id="edit-description"
                        {...registerEdit('description')}
                        placeholder="Access to premium recipes"
                      />
                    </div>

                    {/* Duration Field */}
                    <div className="space-y-2">
                      <Label htmlFor="edit-durationDays">Duration (days)</Label>
                      <Input
                        id="edit-durationDays"
                        type="number"
                        {...registerEdit('durationDays', {
                          required: 'Duration is required',
                          min: { value: 1, message: 'Duration must be at least 1 day' },
                          valueAsNumber: true,
                        })}
                        placeholder="7"
                      />
                      {errorsEdit.durationDays && (
                        <p className="text-sm text-red-500">{errorsEdit.durationDays.message}</p>
                      )}
                    </div>

                    {/* Price Field */}
                    <div className="space-y-2">
                      <Label htmlFor="edit-price">Price (points)</Label>
                      <Input
                        id="edit-price"
                        type="number"
                        step="0.01"
                        {...registerEdit('price', {
                          required: 'Price is required',
                          min: { value: 0, message: 'Price cannot be negative' },
                          valueAsNumber: true,
                        })}
                        placeholder="100.00"
                      />
                      {errorsEdit.price && (
                        <p className="text-sm text-red-500">{errorsEdit.price.message}</p>
                      )}
                    </div>

                    {/* IsActive Toggle */}
                    <div className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        id="edit-isActive"
                        {...registerEdit('isActive')}
                        className="w-4 h-4"
                      />
                      <Label htmlFor="edit-isActive" className="cursor-pointer">
                        Active (available for purchase)
                      </Label>
                    </div>

                    {/* Error Message from API */}
                    {updateMutation.isError && (
                      <div className="text-sm text-red-500">
                        Error: {(updateMutation.error as Error).message}
                      </div>
                    )}

                    <DialogFooter>
                      <Button
                        type="button"
                        variant="outline"
                        onClick={() => setIsEditDialogOpen(false)}
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
        {/* Loading state */}
        {isLoading && (
          <div className="text-center py-8 text-muted-foreground">
            Loading subscriptions...
          </div>
        )}

        {/* Error state */}
        {error && (
          <div className="text-center py-8 text-red-500">
            Error loading subscriptions: {(error as Error).message}
          </div>
        )}

        {/* Subscriptions Grid */}
        {!isLoading && !error && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {subscriptions?.map((subscription) => (
              <Card key={subscription.id}>
                <CardHeader>
                  <CardTitle>{subscription.name}</CardTitle>
                  <CardDescription>
                    {subscription.description || 'No description'}
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Subscription Details */}
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Duration:</span>
                      <span className="font-medium">{subscription.durationDays} days</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Price:</span>
                      <span className="font-medium">{subscription.price} pts</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Recipes:</span>
                      <span className="font-medium">{subscription.recipeCount || 0}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Status:</span>
                      <span className={subscription.isActive ? 'text-green-600' : 'text-red-600'}>
                        {subscription.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEdit(subscription)}
                    >
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1"
                      onClick={() => navigate(`/subscriptions/${subscription.id}`)}
                    >
                      Manage Recipes
                    </Button>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleDelete(subscription.id)}
                      disabled={deleteMutation.isPending}
                    >
                      Delete
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}

            {/* Empty state */}
            {subscriptions?.length === 0 && (
              <div className="col-span-full text-center py-12">
                <p className="text-muted-foreground mb-4">No subscriptions yet</p>
                <Button onClick={() => setIsCreateDialogOpen(true)}>
                  Create your first subscription
                </Button>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  )
}
