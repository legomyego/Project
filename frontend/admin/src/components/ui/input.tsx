// Input component from shadcn/ui
// A styled text input field with consistent styling across the app
// Supports all native HTML input attributes

import * as React from "react"
import { cn } from "@/lib/utils"

// Input props interface
// Extends all native input HTML attributes
export interface InputProps
  extends React.InputHTMLAttributes<HTMLInputElement> {}

/**
 * Input component - styled text input field
 *
 * Features:
 * - Consistent border and padding
 * - Focus ring for accessibility
 * - Disabled state styling
 * - File input special styling
 *
 * @example
 * <Input type="email" placeholder="Enter your email" />
 * <Input type="password" disabled />
 * <Input type="file" />
 */
const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={cn(
          // Base styles for all inputs
          "flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-base shadow-sm transition-colors",
          // File input specific styles
          "file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground",
          // Focus and interaction states
          "placeholder:text-muted-foreground",
          "focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring",
          "disabled:cursor-not-allowed disabled:opacity-50",
          // Mobile text size adjustment
          "md:text-sm",
          className
        )}
        ref={ref}
        {...props}
      />
    )
  }
)
Input.displayName = "Input"

export { Input }
