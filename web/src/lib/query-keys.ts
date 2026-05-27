export const portfolioPanelQueryKeys = {
  wallet: (userId: string) => ['wallet', userId] as const,
  portfolio: (userId: string) => ['portfolio', userId] as const,
  ordersOpen: (userId: string) => ['orders', 'open', userId] as const,
  ordersHistory: (userId: string) => ['orders', 'history', userId] as const,
  trades: (userId: string) => ['trades', userId] as const,
}
