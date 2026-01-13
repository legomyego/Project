// Label component from shadcn/ui
// A styled label for form inputs with proper accessibility attributes
// Works seamlessly with Input and other form components

import * as React from "react"
import { cn } from "@/lib/utils"

// Label props interface
// Extends native label HTML attributes
export interface LabelProps
  extends React.LabelHTMLAttributes<HTMLLabelElement> {}

/**
 * Label component for form fields
 *
 * Features:
 * - Consistent typography and spacing
 * - Disabled state styling
 * - Peer-disabled state support (when associated input is disabled)
 *
 * @example
 * <Label htmlFor="email">Email</Label>
 * <Input id="email" type="email" />
 *
 * <Label htmlFor="disabled-input">Disabled Field</Label>
 * <Input id="disabled-input" disabled />
 */
const Label = React.forwardRef<HTMLLabelElement, LabelProps>(
  ({ className, ...props }, ref) => (
    <label
      ref={ref}
      className={cn(
        // Base typography and spacing
        "text-sm font-medium leading-none",
        // Disabled state - reduce opacity when label or associated input is disabled
        "peer-disabled:cursor-not-allowed peer-disabled:opacity-70",
        className
      )}
      {...props}
    />
  )
)
Label.displayName = "Label"

export { Label }
