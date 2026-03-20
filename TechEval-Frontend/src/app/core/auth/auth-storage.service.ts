import { Injectable } from '@angular/core';
import { AuthSession } from './auth.models';

const SESSION_KEY = 'techeval.session';

@Injectable({ providedIn: 'root' })
export class AuthStorageService {
  save(session: AuthSession): void {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
  }

  load(): AuthSession | null {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthSession;
    } catch {
      sessionStorage.removeItem(SESSION_KEY);
      return null;
    }
  }

  clear(): void {
    sessionStorage.removeItem(SESSION_KEY);
  }
}
