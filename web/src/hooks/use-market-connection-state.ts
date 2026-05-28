import { HubConnectionState } from '@microsoft/signalr'
import { useSyncExternalStore } from 'react'

import { simulationHubClient } from '@/lib/signalr'

const RECONNECTING_THRESHOLD_MS = 5000

let disconnectedAt: number | null = null

function subscribeToMarketConnectionState(onStoreChange: () => void) {
  const interval = window.setInterval(onStoreChange, 500)
  return () => window.clearInterval(interval)
}

function getMarketConnectionSnapshot(): boolean {
  const connectionState = simulationHubClient.getState()

  if (connectionState === HubConnectionState.Connected) {
    disconnectedAt = null
  } else if (
    connectionState === HubConnectionState.Reconnecting ||
    connectionState === HubConnectionState.Disconnected
  ) {
    disconnectedAt ??= Date.now()
  }

  return (
    disconnectedAt !== null &&
    connectionState !== HubConnectionState.Connected &&
    Date.now() - disconnectedAt >= RECONNECTING_THRESHOLD_MS
  )
}

export function useMarketConnectionState() {
  const showReconnecting = useSyncExternalStore(
    subscribeToMarketConnectionState,
    getMarketConnectionSnapshot,
    getMarketConnectionSnapshot,
  )

  return { showReconnecting }
}
