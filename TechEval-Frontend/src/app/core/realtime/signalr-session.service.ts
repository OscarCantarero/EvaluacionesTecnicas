import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class SignalrSessionService {
  private connection: HubConnection | null = null;

  readonly isConnected = signal(false);
  readonly secondsRemaining = signal<number | null>(null);

  async connect(url: string): Promise<void> {
    await this.disconnect();

    this.connection = new HubConnectionBuilder().withUrl(url).withAutomaticReconnect().configureLogging(LogLevel.Warning).build();

    this.connection.on('TickTemporizador', (data: { segundosRestantes?: number }) => {
      this.secondsRemaining.set(data?.segundosRestantes ?? null);
    });

    this.connection.onclose(() => {
      this.isConnected.set(false);
    });

    await this.connection.start();
    this.isConnected.set(true);
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    try {
      await this.connection.stop();
    } finally {
      this.connection = null;
      this.isConnected.set(false);
      this.secondsRemaining.set(null);
    }
  }
}
