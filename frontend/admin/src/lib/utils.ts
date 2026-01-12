// Utility function for conditionally joining CSS class names
// Used throughout shadcn/ui components for dynamic styling

import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

/**
 * Combines multiple class names and resolves Tailwind CSS conflicts
 * @param inputs - Class names or conditional class objects
 * @returns Merged class string with no conflicts
 *
 * Example:
 * cn("px-2 py-1", condition && "bg-blue-500", "px-4")
 * // Returns: "py-1 bg-blue-500 px-4" (px-4 overrides px-2)
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
