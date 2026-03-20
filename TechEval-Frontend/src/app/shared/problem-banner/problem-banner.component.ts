import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ErrorStateService } from '../../core/http/error-state.service';

@Component({
  selector: 'app-problem-banner',
  templateUrl: './problem-banner.component.html',
  styleUrl: './problem-banner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblemBannerComponent {
  protected readonly errorState = inject(ErrorStateService);

  protected dismiss(): void {
    this.errorState.clear();
  }
}
