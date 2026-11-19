export const requiredRule = (value: string) => !!value || 'Заполните поле';
export const emailRule = (value: string) =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || 'Введите корректный email';
export const passwordRule = (value: string) =>
  (value.length >= 8 && value.length <= 128) || 'Пароль должен содержать от 8 до 128 символов';

export const userNameRule = (value: string) =>
  /^[a-zA-Z0-9._-]{3,32}$/.test(value) || 'От 3 до 32 символов: латиница, цифры, точка, _ и -';
