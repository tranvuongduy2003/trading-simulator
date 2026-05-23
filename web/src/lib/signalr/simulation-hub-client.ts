import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'

import { buildSimulationHubUrl } from '@/lib/env'
import { simulationHubClientMethods, simulationHubServerMethods } from '@/types/realtime'

import type {
  SimulationHubHandlers,
  SimulationHubLifecycleInterceptor,
  SimulationHubMessageInterceptor,
  SimulationHubMessageName,
} from './types'

const hubMethodToHandlerKey: Record<string, SimulationHubMessageName> = {
  [simulationHubClientMethods.orderBookUpdated]: 'onOrderBookUpdated',
  [simulationHubClientMethods.tradeTapeEntryPublished]: 'onTradeTapeEntryPublished',
  [simulationHubClientMethods.lastTradePriceChanged]: 'onLastTradePriceChanged',
  [simulationHubClientMethods.orderFillNotified]: 'onOrderFillNotified',
  [simulationHubClientMethods.orderCancellationNotified]: 'onOrderCancellationNotified',
  [simulationHubClientMethods.balanceUpdated]: 'onBalanceUpdated',
}

export class SimulationHubClient {
  private connection: HubConnection | null = null
  private handlers: SimulationHubHandlers = {}
  private readonly messageInterceptors: SimulationHubMessageInterceptor[] = []
  private readonly lifecycleInterceptors: SimulationHubLifecycleInterceptor[] = []

  addMessageInterceptor(interceptor: SimulationHubMessageInterceptor): () => void {
    this.messageInterceptors.push(interceptor)
    return () => {
      const index = this.messageInterceptors.indexOf(interceptor)
      if (index >= 0) {
        this.messageInterceptors.splice(index, 1)
      }
    }
  }

  addLifecycleInterceptor(interceptor: SimulationHubLifecycleInterceptor): () => void {
    this.lifecycleInterceptors.push(interceptor)
    return () => {
      const index = this.lifecycleInterceptors.indexOf(interceptor)
      if (index >= 0) {
        this.lifecycleInterceptors.splice(index, 1)
      }
    }
  }

  setHandlers(handlers: SimulationHubHandlers): void {
    this.handlers = handlers
  }

  getState(): HubConnectionState {
    return this.connection?.state ?? HubConnectionState.Disconnected
  }

  async start(): Promise<void> {
    if (
      this.connection?.state === HubConnectionState.Connected ||
      this.connection?.state === HubConnectionState.Connecting
    ) {
      return
    }

    if (!this.connection) {
      this.connection = new HubConnectionBuilder()
        .withUrl(buildSimulationHubUrl(), { withCredentials: true })
        .withAutomaticReconnect()
        .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
        .build()

      this.registerServerCallbacks()
      this.registerLifecycleCallbacks()
    }

    await this.emitLifecycle({ type: 'connecting' })
    await this.connection.start()
    await this.emitLifecycle({ type: 'connected' })
  }

  async stop(): Promise<void> {
    if (!this.connection) {
      return
    }

    await this.connection.stop()
    await this.emitLifecycle({ type: 'closed' })
  }

  async subscribeToMarket(symbol: string): Promise<void> {
    await this.invokeServer(simulationHubServerMethods.subscribeToMarket, symbol)
  }

  async unsubscribeFromMarket(symbol: string): Promise<void> {
    await this.invokeServer(simulationHubServerMethods.unsubscribeFromMarket, symbol)
  }

  async subscribeToUserNotifications(): Promise<void> {
    await this.invokeServer(simulationHubServerMethods.subscribeToUserNotifications)
  }

  async unsubscribeFromUserNotifications(): Promise<void> {
    await this.invokeServer(simulationHubServerMethods.unsubscribeFromUserNotifications)
  }

  private registerServerCallbacks(): void {
    if (!this.connection) {
      return
    }

    for (const [methodName, handlerKey] of Object.entries(hubMethodToHandlerKey)) {
      this.connection.on(methodName, async (payload: unknown) => {
        await this.dispatchMessage(handlerKey, payload)
      })
    }
  }

  private registerLifecycleCallbacks(): void {
    if (!this.connection) {
      return
    }

    this.connection.onreconnecting(async (error) => {
      await this.emitLifecycle({ type: 'reconnecting', error })
    })

    this.connection.onreconnected(async () => {
      await this.emitLifecycle({ type: 'reconnected' })
    })

    this.connection.onclose(async (error) => {
      await this.emitLifecycle({
        type: 'closed',
        error: error ?? undefined,
      })
    })
  }

  private async dispatchMessage(
    handlerKey: SimulationHubMessageName,
    payload: unknown,
  ): Promise<void> {
    const envelope = {
      name: handlerKey,
      payload: payload as never,
    }

    for (const interceptor of this.messageInterceptors) {
      await interceptor(envelope)
    }

    const handler = this.handlers[handlerKey]
    if (handler) {
      handler(envelope.payload)
    }
  }

  private async invokeServer(method: string, ...args: unknown[]): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('Simulation hub is not connected')
    }

    await this.connection.invoke(method, ...args)
  }

  private async emitLifecycle(
    event: Parameters<SimulationHubLifecycleInterceptor>[0],
  ): Promise<void> {
    for (const interceptor of this.lifecycleInterceptors) {
      await interceptor(event)
    }
  }
}

export const simulationHubClient = new SimulationHubClient()
