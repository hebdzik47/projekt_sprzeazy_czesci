import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { api, errorText, money, today } from './services';
import type { Car, Part } from './services';
import { Heading, Notice } from './components';
import './saleFields.css';

export function SalePage({ token, cars, seller, admin }: { token: string; cars: Car[]; seller: string; admin: boolean }) {
  const [carId, setCarId] = useState(''), [selected, setSelected] = useState<Record<number,{price:string;quantity:string}>>({});
  const [parts, setParts] = useState<Part[]>([]), [sellers, setSellers] = useState<Part[]>([]), [salespersonId, setSalespersonId] = useState('');
  const [priceType, setPriceType] = useState('net'), [search,setSearch] = useState('');
  const normalize = (value:string) => value.toLocaleLowerCase('pl-PL').normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/ł/g,'l');
  const visibleParts=parts.filter(p=>normalize(p.name).includes(normalize(search.trim())));
  const [busy, setBusy] = useState(false), [error, setError] = useState(''), [success, setSuccess] = useState('');
  useEffect(() => { let active = true; Promise.all([api<Part[]>('/parts', token), admin ? api<Part[]>('/salespeople', token) : Promise.resolve([])]).then(([p,s]) => { if (active) { setParts(p); setSellers(s); } }).catch(e => { if(active)setError(errorText(e)); }); return () => { active = false; }; }, [token, admin]);
  const net = (price:string) => priceType === 'gross' ? Math.round(Math.round(Number(price)*100)*100/123)/100 : Number(price);
  const total = Object.values(selected).reduce((sum,item)=>sum+Math.round(net(item.price)*100)*Number(item.quantity),0)/100;
  function toggle(id:number) { setSelected(current=>{ const next={...current}; if(id in next)delete next[id];else next[id]={price:'',quantity:'1'};return next; }); }
  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); if(busy)return; if(Object.values(selected).some(item=>!item.price||Number(item.price)<=0||!Number.isInteger(Number(item.quantity))||Number(item.quantity)<1||Number(item.quantity)>10000)){setSearch('');setError('Wpisz cenę i całkowitą ilość od 1 do 10 000 dla każdej części.');return;} const form=e.currentTarget; const f=new FormData(form); setBusy(true); setError(''); setSuccess('');
    try {
      const result = await api<{count:number;quantity:number;totalNet:number}>('/sales/batch', token, { carId:Number(carId), items:Object.entries(selected).map(([id,item])=>({partId:Number(id),price:Number(item.price),quantity:Number(item.quantity)})), priceType, saleDate:f.get('date'), ...(admin ? {salespersonId:Number(salespersonId)} : {}) });
      form.reset(); setCarId(''); setSelected({}); setSearch(''); setSuccess(`Zapisano ${result.quantity} szt. (${result.count} pozycji). Łącznie netto: ${money(result.totalNet)}.`);
    } catch(e) { setError(errorText(e)); } finally { setBusy(false); }
  }
  return <><Heading title="Nowa sprzedaż" description="Wybierz samochód, zaznacz części i podaj ich ceny."/>
    <div className="sale-layout"><section className="panel"><h2>Dane sprzedaży</h2><Notice text={error}/><Notice text={success} success/>
      <form onSubmit={save} className="form-grid">
        <label>Samochód<select value={carId} onChange={e=>{setCarId(e.target.value);setSelected({});}} required disabled={busy}><option value="">Wybierz samochód</option>{cars.map(c=><option key={c.id} value={c.id}>{c.make} {c.model}</option>)}</select></label>
        <label>Rodzaj wpisywanej ceny<select value={priceType} onChange={e=>{setPriceType(e.target.value);setSelected(current=>Object.fromEntries(Object.entries(current).map(([id,item])=>[id,{...item,price:''}])));}} disabled={busy}><option value="net">Cena netto</option><option value="gross">Cena brutto (VAT 23%)</option></select></label>
        <label>Data sprzedaży<input name="date" type="date" defaultValue={today()} required disabled={busy}/></label>
        {admin ? <label>Sprzedawca<select value={salespersonId} onChange={e=>setSalespersonId(e.target.value)} required disabled={busy}><option value="">Wybierz sprzedawcę</option>{sellers.map(s=><option key={s.id} value={s.id}>{s.name}</option>)}</select></label> : <label>Sprzedawca<input value={seller} readOnly/></label>}
        <fieldset className="part-picker" disabled={!carId||busy}><legend>Części — zaznacz jedną lub kilka</legend>
          {!carId&&<p className="muted">Najpierw wybierz samochód.</p>}
          {carId&&!parts.length&&<p className="muted">Brak części w katalogu. Dodaj je w zakładce Samochody i części.</p>}
          <label>Wyszukaj części<input type="search" placeholder="Wpisz np. lam…" value={search} onChange={e=>setSearch(e.target.value)}/></label>
          {search&&Object.keys(selected).some(id=>!visibleParts.some(p=>p.id===Number(id)))&&<p className="selection-hint">Część zaznaczonych pozycji jest ukryta przez wyszukiwanie. <button type="button" className="secondary" onClick={()=>setSearch('')}>Pokaż wszystkie</button></p>}
          {!visibleParts.length&&<p className="muted">Brak pasujących części.</p>}
          {visibleParts.map(p=><div className={p.id in selected?'part-row selected':'part-row'} key={p.id}>
            <label className="part-choice"><input type="checkbox" checked={p.id in selected} onChange={()=>toggle(p.id)}/>{p.name}</label>
            {p.id in selected&&<div className="part-inline-fields">
              <label>Ilość sztuk<input aria-label={`Ilość sztuk — ${p.name}`} type="number" inputMode="numeric" min="1" max="10000" step="1" required value={selected[p.id].quantity} onChange={e=>setSelected({...selected,[p.id]:{...selected[p.id],quantity:e.target.value}})}/></label>
              <label>Cena {priceType==='gross'?'brutto':'netto'} / szt.<input aria-label={`Cena ${priceType==='gross'?'brutto':'netto'} za sztukę — ${p.name}`} type="number" inputMode="decimal" min="0.01" max="9999999999.99" step="0.01" placeholder="0,00" required value={selected[p.id].price} onChange={e=>setSelected({...selected,[p.id]:{...selected[p.id],price:e.target.value}})}/></label>
              <small className="part-inline-total">{money(net(selected[p.id].price))} netto / szt. · Razem netto: <strong>{money(Math.round(net(selected[p.id].price)*100)*Number(selected[p.id].quantity)/100)}</strong></small>
            </div>}

          </div>)}
        </fieldset>
        <div className="form-footer"><small aria-live="polite">Wybrano: {Object.keys(selected).length} · Razem netto: <strong>{money(total)}</strong></small><button disabled={busy||!Object.keys(selected).length}>{busy?'Zapisywanie…':'Zapisz sprzedaż →'}</button></div>
      </form></section></div>
  </>;
}
