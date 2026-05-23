const apiUrl = import.meta.env.VITE_API_URL

export const env = {
  apiUrl: typeof apiUrl === 'string' && apiUrl.length > 0 ? apiUrl : 'https://localhost:8000',
} as const
