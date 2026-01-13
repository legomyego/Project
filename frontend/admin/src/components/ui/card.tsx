// Card component from shadcn/ui
// A container component for content with header, body, and footer sections
// Perfect for dashboard widgets, forms, and content blocks

import * as React from "react"
import { cn } from "@/lib/utils"

/**
 * Card container - main wrapper
 * Provides the card background, border, and rounded corners
 */
const Card = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn(
      "rounded-xl border bg-card text-card-foreground shadow",
      className
    )}
    {...props}
  />
))
Card.displayName = "Card"

/**
 * CardHeader - top section of the card
 * Typically contains title and description
 */
const CardHeader = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn("flex flex-col space-y-1.5 p-6", className)}
    {...props}
  />
))
CardHeader.displayName = "CardHeader"

/**
 * CardTitle - main heading within card header
 */
const CardTitle = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn("font-semibold leading-none tracking-tight", className)}
    {...props}
  />
))
CardTitle.displayName = "CardTitle"

/**
 * CardDescription - subtitle or description text
 */
const CardDescription = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn("text-sm text-muted-foreground", className)}
    {...props}
  />
))
CardDescription.displayName = "CardDescription"

/**
 * CardContent - main content area
 * Contains the primary card content
 */
const CardContent = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div ref={ref} className={cn("p-6 pt-0", className)} {...props} />
))
CardContent.displayName = "CardContent"

/**
 * CardFooter - bottom section
 * Typically contains actions or additional info
 */
const CardFooter = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn("flex items-center p-6 pt-0", className)}
    {...props}
  />
))
CardFooter.displayName = "CardFooter"

/**
 * Example usage:
 * <Card>
 *   <CardHeader>
 *     <CardTitle>Total Users</CardTitle>
 *     <CardDescription>Active users in the system</CardDescription>
 *   </CardHeader>
 *   <CardContent>
 *     <p className="text-3xl font-bold">1,234</p>
 *   </CardContent>
 *   <CardFooter>
 *     <Button>View Details</Button>
 *   </CardFooter>
 * </Card>
 */

export { Card, CardHeader, CardFooter, CardTitle, CardDescription, CardContent }
