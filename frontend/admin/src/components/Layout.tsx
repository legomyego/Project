import { Sidebar } from './Sidebar'

interface LayoutProps {
  children: React.ReactNode
}

// Layout component that wraps all admin pages
// Provides consistent sidebar navigation across the app
export function Layout({ children }: LayoutProps) {
  return (
    <div className="flex min-h-screen">
      {/* Sidebar navigation - fixed on the left */}
      <Sidebar />

      {/* Main content area - offset by sidebar width */}
      <main className="ml-64 flex-1 bg-gray-50">
        {children}
      </main>
    </div>
  )
}
