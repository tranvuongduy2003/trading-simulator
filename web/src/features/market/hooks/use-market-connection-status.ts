import { HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState, useSyncExternalStore } from 'react'

import { simulationHubClient } from '@/lib/signalr'

const reconnectingBadgeDelayMilliseconds = 5_000

function subscribeToHubState(onStoreChange: () => void) {
  const interval = window.setInterval(onStoreChange, 500)
  return () => window.clearInterval(interval)
}

function getHubConnectionSnapshot() {
  return simulationHubClient.getState()
}

export function useMarketConnectionStatus() {
  const connectionState = useSyncExternalStore(
    subscribeToHubState,
    getHubConnectionSnapshot,
    getHubConnectionSnapshot,
  )
  const [showReconnectingBadge, setShowReconnectingBadge] = useState(false)

  useEffect(() => {
    const isDisconnected =
      connectionState === HubConnectionState.Disconnected ||
      connectionState === HubConnectionState.Reconnecting

    if (!isDisconnected) {
      const hideTimeout = window.setTimeout(() => {
        setShowReconnectingBadge(false)
      }, 0)

      return () => window.clearTimeout(hideTimeout)
    }

    const showTimeout = window.setTimeout(() => {
      setShowReconnectingBadge(true)
    }, reconnectingBadgeDelayMilliseconds)

    return () => window.clearTimeout(showTimeout)
  }, [connectionState])

  return showReconnectingBadge
}
