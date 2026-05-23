import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useSyncExternalStore } from 'react'

import { env } from '@/lib/env'
import {
  createSimulationHubQueryBridge,
  simulationHubClient,
  simulationHubDebugInterceptor,
} from '@/lib/signalr'
import { useAuthStore } from '@/store/auth-store'

function subscribeToHubState(onStoreChange: () => void) {
  const interval = window.setInterval(onStoreChange, 500)
  return () => window.clearInterval(interval)
}

function getHubConnectionSnapshot() {
  return simulationHubClient.getState()
}

export function useSimulationHub() {
  const queryClient = useQueryClient()
  const authStatus = useAuthStore((state) => state.status)
  const connectionState = useSyncExternalStore(
    subscribeToHubState,
    getHubConnectionSnapshot,
    getHubConnectionSnapshot,
  )

  useEffect(() => {
    const removeDebug = simulationHubClient.addMessageInterceptor(simulationHubDebugInterceptor)
    const removeBridge = simulationHubClient.addMessageInterceptor(
      createSimulationHubQueryBridge(queryClient),
    )

    return () => {
      removeDebug()
      removeBridge()
    }
  }, [queryClient])

  useEffect(() => {
    if (authStatus !== 'authenticated') {
      void simulationHubClient.stop()
      return
    }

    let cancelled = false

    async function connect() {
      try {
        await simulationHubClient.start()
        if (cancelled) {
          return
        }

        await simulationHubClient.subscribeToMarket(env.defaultSymbol)
        await simulationHubClient.subscribeToUserNotifications()
      } catch (error) {
        console.error('Simulation hub connection failed', error)
      }
    }

    void connect()

    return () => {
      cancelled = true
      void simulationHubClient.stop()
    }
  }, [authStatus])

  return {
    connectionState,
    client: simulationHubClient,
  }
}
