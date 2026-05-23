import { Link } from 'react-router-dom'

import { paths } from '@/app/paths'

export function RegisterPage() {
  return (
    <div className="flex flex-col gap-3">
      <h1 className="text-2xl font-semibold tracking-tight">Register</h1>
      <p className="text-muted-foreground text-sm">Registration form placeholder.</p>
      <p className="text-muted-foreground text-sm">
        Already have an account?{' '}
        <Link to={paths.login} className="text-foreground underline-offset-4 hover:underline">
          Login
        </Link>
      </p>
    </div>
  )
}
