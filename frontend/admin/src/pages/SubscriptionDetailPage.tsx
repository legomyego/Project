// Subscription Detail Page
// Allows admins to add/remove recipes from a subscription plan

import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/contexts/AuthContext'
import { api, type SubscriptionDetail, type Recipe } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'

/**
 * SubscriptionDetailPage component
 * Shows subscription details and allows managing which recipes are included
 */
export function SubscriptionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { logout } = useAuth()
  const queryClient = useQueryClient()

  // State for recipe search and selection
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedRecipeIds, setSelectedRecipeIds] = useState<string[]>([])

  /**
   * Fetch subscription details
   * Includes the list of recipes already assigned to this subscription
   */
  const { data: subscription, isLoading: isLoadingSubscription, error: subscriptionError } = useQuery({
    queryKey: ['subscription', id],
    queryFn: async () => {
      if (!id) throw new Error('Subscription ID is required')
      return api.subscriptions.getById(id)
    },
    enabled: !!id,
  })

  /**
   * Fetch all available recipes
   * Used to show recipes that can be added to the subscription
   */
  const { data: recipesData, isLoading: isLoadingRecipes } = useQuery({
    queryKey: ['recipes'],
    queryFn: async () => {
      const response = await fetch('http://localhost:5010/api/recipes?page=1&pageSize=100', {
        credentials: 'include',
      })
      if (!response.ok) {
        if (response.status === 401) {
          logout()
          throw new Error('Unauthorized')
        }
        throw new Error('Failed to fetch recipes')
      }
      return response.json() as Promise<{ recipes: Recipe[] }>
    },
  })

  /**
   * Mutation to add recipes to subscription
   * Sends array of recipe IDs to backend
   */
  const addRecipesMutation = useMutation({
    mutationFn: async (recipeIds: string[]) => {
      if (!id) throw new Error('Subscription ID is required')
      return api.subscriptions.addRecipes(id, recipeIds)
    },
    onSuccess: () => {
      // Invalidate subscription query to refetch with new recipes
      queryClient.invalidateQueries({ queryKey: ['subscription', id] })
      // Clear selection
      setSelectedRecipeIds([])
    },
    onError: (error: Error) => {
      alert(`Failed to add recipes: ${error.message}`)
    },
  })

  /**
   * Mutation to remove a recipe from subscription
   * Removes the association between subscription and recipe
   */
  const removeRecipeMutation = useMutation({
    mutationFn: async (recipeId: string) => {
      if (!id) throw new Error('Subscription ID is required')
      return api.subscriptions.removeRecipe(id, recipeId)
    },
    onSuccess: () => {
      // Invalidate subscription query to refetch without removed recipe
      queryClient.invalidateQueries({ queryKey: ['subscription', id] })
    },
    onError: (error: Error) => {
      alert(`Failed to remove recipe: ${error.message}`)
    },
  })

  /**
   * Handle adding selected recipes
   * Validates selection and calls mutation
   */
  const handleAddRecipes = () => {
    if (selectedRecipeIds.length === 0) {
      alert('Please select at least one recipe')
      return
    }
    addRecipesMutation.mutate(selectedRecipeIds)
  }

  /**
   * Toggle recipe selection
   * Used for multi-select functionality
   */
  const toggleRecipeSelection = (recipeId: string) => {
    setSelectedRecipeIds(prev =>
      prev.includes(recipeId)
        ? prev.filter(id => id !== recipeId)
        : [...prev, recipeId]
    )
  }

  // Loading state
  if (isLoadingSubscription || isLoadingRecipes) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-muted-foreground">Loading...</div>
      </div>
    )
  }

  // Error state
  if (subscriptionError) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-destructive">
          Error loading subscription: {(subscriptionError as Error).message}
        </div>
      </div>
    )
  }

  if (!subscription) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-muted-foreground">Subscription not found</div>
      </div>
    )
  }

  // Filter recipes based on search query
  const allRecipes = recipesData?.recipes || []
  const filteredRecipes = allRecipes.filter(recipe =>
    recipe.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    recipe.description.toLowerCase().includes(searchQuery.toLowerCase())
  )

  // Get IDs of recipes already in subscription
  const assignedRecipeIds = new Set(subscription.recipes.map(r => r.id))

  // Filter out recipes that are already assigned
  const availableRecipes = filteredRecipes.filter(recipe => !assignedRecipeIds.has(recipe.id))

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <div>
            <Button variant="ghost" onClick={() => navigate('/subscriptions')}>
              ← Back to Subscriptions
            </Button>
            <h1 className="text-2xl font-bold mt-2">{subscription.name}</h1>
            <p className="text-muted-foreground">{subscription.description}</p>
          </div>
          <Button variant="outline" onClick={logout}>
            Logout
          </Button>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Left Column: Assigned Recipes */}
          <div>
            <Card>
              <CardHeader>
                <CardTitle>Assigned Recipes ({subscription.recipes.length})</CardTitle>
                <CardDescription>
                  Recipes included in this subscription plan
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {subscription.recipes.length === 0 ? (
                    <p className="text-muted-foreground text-center py-8">
                      No recipes assigned yet
                    </p>
                  ) : (
                    subscription.recipes.map(recipe => (
                      <div
                        key={recipe.id}
                        className="flex items-center justify-between p-3 border rounded-lg"
                      >
                        <div className="flex-1">
                          <p className="font-medium">{recipe.title}</p>
                          <p className="text-sm text-muted-foreground line-clamp-1">
                            {recipe.description}
                          </p>
                        </div>
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() => removeRecipeMutation.mutate(recipe.id)}
                          disabled={removeRecipeMutation.isPending}
                        >
                          Remove
                        </Button>
                      </div>
                    ))
                  )}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Right Column: Available Recipes */}
          <div>
            <Card>
              <CardHeader>
                <CardTitle>Add Recipes</CardTitle>
                <CardDescription>
                  Select recipes to add to this subscription
                </CardDescription>
              </CardHeader>
              <CardContent>
                {/* Search Input */}
                <div className="mb-4">
                  <Input
                    placeholder="Search recipes..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </div>

                {/* Recipe List */}
                <div className="space-y-2 max-h-[500px] overflow-y-auto mb-4">
                  {availableRecipes.length === 0 ? (
                    <p className="text-muted-foreground text-center py-8">
                      {searchQuery ? 'No recipes found' : 'All recipes are already assigned'}
                    </p>
                  ) : (
                    availableRecipes.map(recipe => (
                      <div
                        key={recipe.id}
                        className={`flex items-start gap-3 p-3 border rounded-lg cursor-pointer transition-colors ${
                          selectedRecipeIds.includes(recipe.id)
                            ? 'bg-primary/10 border-primary'
                            : 'hover:bg-muted'
                        }`}
                        onClick={() => toggleRecipeSelection(recipe.id)}
                      >
                        <input
                          type="checkbox"
                          checked={selectedRecipeIds.includes(recipe.id)}
                          onChange={() => {}}
                          className="mt-1"
                        />
                        <div className="flex-1">
                          <p className="font-medium">{recipe.title}</p>
                          <p className="text-sm text-muted-foreground line-clamp-2">
                            {recipe.description}
                          </p>
                          <p className="text-sm text-muted-foreground mt-1">
                            Price: {recipe.price} points
                          </p>
                        </div>
                      </div>
                    ))
                  )}
                </div>

                {/* Add Button */}
                <Button
                  onClick={handleAddRecipes}
                  disabled={selectedRecipeIds.length === 0 || addRecipesMutation.isPending}
                  className="w-full"
                >
                  {addRecipesMutation.isPending
                    ? 'Adding...'
                    : `Add ${selectedRecipeIds.length} Recipe${selectedRecipeIds.length !== 1 ? 's' : ''}`}
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  )
}
