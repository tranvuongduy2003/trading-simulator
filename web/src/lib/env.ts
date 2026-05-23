const apiUrl = import.meta.env.VITE_API_URL

export const env = {
  apiUrl: typeof apiUrl === 'string' && apiUrl.length > 0 ? apiUrl : 'http://localhost:8001',
} as const
