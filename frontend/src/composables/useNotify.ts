import { Notify } from 'quasar';

export function useNotify() {
  function notifySuccess(message: string) {
    Notify.create({ message, type: 'positive', position: 'top', timeout: 2200 });
  }

  function notifyError(message: string) {
    Notify.create({
      message,
      type: 'negative',
      position: 'top',
      timeout: 3000,
      actions: [{ label: 'Закрыть' }],
    });
  }

  return { notifyError, notifySuccess };
}
