// Trades Overview Page (placeholder)
// Will display all trade offers and their status

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useNavigate } from 'react-router-dom'
import { useLogout } from '@/hooks/useLogout'

export function TradesPage() {
  const navigate = useNavigate()
  const { handleLogout } = useLogout()

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-slate-900">Trade Overview</h1>
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

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Card>
          <CardHeader>
            <CardTitle>Trades</CardTitle>
            <CardDescription>Coming soon - Trade monitoring features</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-muted-foreground">
              Trade overview table will be implemented here
            </p>
          </CardContent>
        </Card>
      </main>
    </div>
  )
}
