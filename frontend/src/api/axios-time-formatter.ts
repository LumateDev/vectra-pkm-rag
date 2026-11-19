import dayjs from 'dayjs';

/** Форматирует момент времени с локальным смещением клиента. */
export function formatDateWithOffset(date: Date): string {
  return dayjs(date).format('YYYY-MM-DDTHH:mm:ss.SSSZ');
}

/** Преобразует Date в массивах и обычных объектах, сохраняя прочие типы. */
export function replaceDatesWithOffset(data: unknown): unknown {
  if (data instanceof Date) return formatDateWithOffset(data);
  if (Array.isArray(data)) return data.map(replaceDatesWithOffset);
  if (data === null || typeof data !== 'object') return data;

  const prototype: unknown = Object.getPrototypeOf(data);
  if (prototype !== Object.prototype && prototype !== null) return data;
  return Object.fromEntries(
    Object.entries(data).map(([key, value]) => [key, replaceDatesWithOffset(value)]),
  );
}
