import { Notify } from 'quasar';

export function useNotify() {
  function notifyError(message: string) {
    Notify.create({
      message,
      type: 'negative',
      position: 'top',
      timeout: 3000,
      actions: [{ label: 'Закрыть' }],
    });
  }

  return { notifyError };
}
