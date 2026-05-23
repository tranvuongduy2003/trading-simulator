import type {
  BalanceUpdatedMessage,
  LastTradePriceMessage,
  OrderBookUpdatedMessage,
  OrderCancellationNotificationMessage,
  OrderFillNotificationMessage,
  TradeTapeEntryMessage,
} from '@/types/realtime'

export type SimulationHubConnectionState =
  | 'disconnected'
  | 'connecting'
  | 'connected'
  | 'reconnecting'

export type SimulationHubHandlers = {
  onOrderBookUpdated?: (message: OrderBookUpdatedMessage) => void
  onTradeTapeEntryPublished?: (message: TradeTapeEntryMessage) => void
  onLastTradePriceChanged?: (message: LastTradePriceMessage) => void
  onOrderFillNotified?: (message: OrderFillNotificationMessage) => void
  onOrderCancellationNotified?: (message: OrderCancellationNotificationMessage) => void
  onBalanceUpdated?: (message: BalanceUpdatedMessage) => void
}

export type SimulationHubMessageName = keyof SimulationHubHandlers

export type SimulationHubMessagePayload = Parameters<
  NonNullable<SimulationHubHandlers[SimulationHubMessageName]>
>[0]

export type SimulationHubMessageInterceptor = (message: {
  name: SimulationHubMessageName
  payload: SimulationHubMessagePayload
}) => void | Promise<void>

export type SimulationHubLifecycleInterceptor = (event: {
  type: 'connecting' | 'connected' | 'reconnecting' | 'reconnected' | 'closed' | 'error'
  error?: Error
}) => void | Promise<void>
