export type Session = { token: string; user: { displayName: string; roles: string[] } };
export type Car = { id: number; make: string; model: string };
export type Part = { id: number; name: string };
export type Sale = { id: number; saleDate: string; car: string; part: string; price: number; quantity: number; totalNet: number; userName: string };
export type Group = { name: string; count: number; value: number };
export type Summary = { totalValue: number; salesCount: number; partsCount: number; byUser: Group[]; byCar: Group[] };
export const money = (value: number) => value.toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });
export const today = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; };
export const errorText = (error: unknown) => error instanceof Error ? error.message : 'Nie udało się wykonać operacji.';
export async function api<T>(path: string, token = '', body?: unknown): Promise<T> {
  let response: Response;
  try { response = await fetch('/api' + path, { method: body === undefined ? 'GET' : 'POST', headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) }, ...(body === undefined ? {} : { body: JSON.stringify(body) }) }); }
  catch { throw new Error('Brak połączenia z serwerem. Sprawdź sieć i spróbuj ponownie.'); }
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    if (response.status === 401 && token) window.dispatchEvent(new Event('session-expired'));
    throw new Error(data?.message || data?.detail || (data?.errors ? Object.values(data.errors).flat().join(' ') : null) || (response.status === 403 ? 'Nie masz uprawnień do tej operacji.' : 'Nie udało się wykonać operacji.'));
  }
  return data;
}
