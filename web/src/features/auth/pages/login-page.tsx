import { Link } from 'react-router-dom'

import { paths } from '@/app/paths'

import { LoginForm } from '../login-form'

export function LoginPage() {
  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Log in</h1>
        <p className="text-muted-foreground text-sm">Access your portfolio and trade AAPL.</p>
      </div>

      <LoginForm />

      <p className="text-muted-foreground text-center text-sm">
        No account?{' '}
        <Link to={paths.register} className="text-foreground underline-offset-4 hover:underline">
          Register
        </Link>
      </p>
    </div>
  )
}
