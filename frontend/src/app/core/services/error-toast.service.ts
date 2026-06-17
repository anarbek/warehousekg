import { Injectable } from '@angular/core';
import notify from 'devextreme/ui/notify';
import { HttpErrorResponse } from '@angular/common/http';

/**
 * Centralized error toast service.
 * Shows a popup notification with the actual business error message
 * from the backend, falling back to a generic message if unavailable.
 */
@Injectable({ providedIn: 'root' })
export class ErrorToastService {
  /**
   * Shows a red error toast popup.
   * @param err The error object (HttpErrorResponse, string, or unknown).
   * @param fallback Fallback message if the error body can't be parsed.
   */
  show(err: unknown, fallback: string): void {
    const msg = this.extractMessage(err, fallback);
    notify({ message: msg, type: 'error', width: 400, displayTime: 6000 });
  }

  /** Shorthand for save errors. */
  showSave(err: unknown): void {
    this.show(err, 'Не удалось сохранить данные');
  }

  /** Shorthand for load errors. */
  showLoad(err: unknown): void {
    this.show(err, 'Не удалось загрузить данные');
  }

  private extractMessage(err: unknown, fallback: string): string {
    if (!err) return fallback;

    // HttpErrorResponse
    if (err instanceof HttpErrorResponse) {
      const body = err.error;
      if (typeof body === 'object' && body !== null) {
        // e.g. { error: "Transaction date ..." }
        if (typeof body.error === 'string' && body.error.length > 0) return body.error;
        // e.g. { message: "..." }
        if (typeof body.message === 'string' && body.message.length > 0) return body.message;
        // e.g. { title: "..." }
        if (typeof body.title === 'string' && body.title.length > 0) return body.title;
      }
      if (typeof body === 'string' && body.length > 0) return body;
      if (err.message && err.message.length > 0) return err.message;
      return `${err.status} ${err.statusText}`;
    }

    // Plain string
    if (typeof err === 'string') return err;

    // Error with message
    if (err instanceof Error) return err.message;

    return fallback;
  }
}
