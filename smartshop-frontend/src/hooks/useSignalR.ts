import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/store/authStore';

export interface SignalRNotification {
  notificationId: string;
  title: string;
  message: string;
  orderId?: string;
}

export function useSignalR(onOrderStatusUpdated: (data: SignalRNotification) => void) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const callbackRef = useRef(onOrderStatusUpdated);
  const { accessToken } = useAuthStore();

  // Keep callback ref fresh without re-connecting
  useEffect(() => {
    callbackRef.current = onOrderStatusUpdated;
  }, [onOrderStatusUpdated]);

  useEffect(() => {
    if (!accessToken) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5284/hubs/orders', {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .build();

    connection.on('OrderStatusUpdated', (data: SignalRNotification) => {
      callbackRef.current(data);
    });

    connection.start().catch((err) => console.error('SignalR connection failed:', err));
    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [accessToken]);
}
