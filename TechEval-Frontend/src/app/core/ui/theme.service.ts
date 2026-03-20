import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

type ThemeName = 'light' | 'dark';

const THEME_KEY = 'techeval.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly themeState = signal<ThemeName>(this.readStoredTheme());

  readonly theme = computed(() => this.themeState());
  readonly isDark = computed(() => this.themeState() === 'dark');

  constructor() {
    effect(() => {
      const current = this.themeState();
      this.document.documentElement.setAttribute('data-theme', current);
      localStorage.setItem(THEME_KEY, current);
    });
  }

  toggle(): void {
    this.themeState.update((current) => (current === 'dark' ? 'light' : 'dark'));
  }

  private readStoredTheme(): ThemeName {
    const stored = localStorage.getItem(THEME_KEY);
    return stored === 'dark' ? 'dark' : 'light';
  }
}
