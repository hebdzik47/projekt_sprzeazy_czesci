import type { Sale } from './services';
import { money } from './services';
export function Notice({ text, success = false }: { text: string; success?: boolean }) {
  return text ? <div className={`notice ${success ? 'success' : 'error'}`} role={success ? 'status' : 'alert'}>{text}</div> : null;
}
export function Heading({ title, description, tag }: { title: string; description: string; tag?: string }) {
  return <header className="page-heading"><div>{tag&&<p className="eyebrow">{tag}</p>}<h1>{title}</h1><p className="muted">{description}</p></div><span className="live"><i /> Połączono z firmą</span></header>;
}
export function SalesList({ items }: { items: Sale[] }) {
  if (!items.length) return <div className="empty"><span>◎</span><h3>Brak sprzedaży</h3><p>Zmień filtry lub dodaj pierwszą sprzedaż.</p></div>;
  return <div className="table-wrap"><table className="sales-table"><thead><tr><th>Data / numer</th><th>Samochód i część</th><th>Sprzedawca</th><th className="amount">Kwota netto</th></tr></thead><tbody>{items.map(s => <tr key={s.id}><td data-label="Data"><strong>{s.saleDate.split('-').reverse().join('.')}</strong><small>SPR / {String(s.id).padStart(5,'0')}</small></td><td data-label="Przedmiot"><strong>{s.car}</strong><small>{s.part} · {s.quantity} szt.</small></td><td data-label="Sprzedawca"><span className="seller"><span className="avatar">{s.userName.slice(0,1)}</span>{s.userName}</span></td><td className="amount" data-label="Netto"><strong>{money(s.totalNet)}</strong><small>{s.quantity} × {money(s.price)} / szt.</small></td></tr>)}</tbody></table></div>;
}
