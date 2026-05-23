/** Mirrors TradingSimulator.Contracts.Realtime — keep in sync with backend records. */

export type OrderBookLevel = {
  price: number
  quantity: number
}

export type OrderBookUpdatedMessage = {
  symbol: string
  bids: OrderBookLevel[]
  asks: OrderBookLevel[]
  updatedAt: string
}

export type TradeTapeEntryMessage = {
  symbol: string
  tradeIdentifier: string
  price: number
  quantity: number
  executedAt: string
}

export type LastTradePriceMessage = {
  symbol: string
  price: number
  updatedAt: string
}

export type OrderFillNotificationMessage = {
  orderIdentifier: string
  symbol: string
  filledQuantity: number
  averagePrice: number
  orderStatus: string
  filledAt: string
}

export type OrderCancellationNotificationMessage = {
  orderIdentifier: string
  symbol: string
  cancelledAt: string
}

export type BalanceUpdatedMessage = {
  userIdentifier: string
  totalCash: number
  reservedCash: number
  availableCash: number
  updatedAt: string
}

export const simulationHubClientMethods = {
  orderBookUpdated: 'OrderBookUpdated',
  tradeTapeEntryPublished: 'TradeTapeEntryPublished',
  lastTradePriceChanged: 'LastTradePriceChanged',
  orderFillNotified: 'OrderFillNotified',
  orderCancellationNotified: 'OrderCancellationNotified',
  balanceUpdated: 'BalanceUpdated',
} as const

export const simulationHubServerMethods = {
  subscribeToMarket: 'SubscribeToMarket',
  unsubscribeFromMarket: 'UnsubscribeFromMarket',
  subscribeToUserNotifications: 'SubscribeToUserNotifications',
  unsubscribeFromUserNotifications: 'UnsubscribeFromUserNotifications',
} as const
