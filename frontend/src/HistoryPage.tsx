import { useEffect, useState } from 'react';
import { api, errorText } from './services';
import type { Car, Sale } from './services';
import { Heading, Notice, SalesList } from './components';
import { dateRange, presets } from './dateRanges';

export function HistoryPage({token,admin,cars}:{token:string;admin:boolean;cars:Car[]}) {
  const empty={search:'',from:'',to:'',seller:'',carId:''};
  const [filter,setFilter]=useState(empty), [draft,setDraft]=useState(empty), [page,setPage]=useState(1);
  const [sellers,setSellers]=useState<{key:string;name:string}[]>([]);
  const [data,setData]=useState<{items:Sale[];total:number}>({items:[],total:0}), [loading,setLoading]=useState(true), [error,setError]=useState('');
  useEffect(()=>{let active=true;if(admin)api<typeof sellers>('/sales/sellers',token).then(s=>{if(active)setSellers(s);}).catch(e=>{if(active)setError(errorText(e));});return()=>{active=false;};},[token,admin]);
  useEffect(()=>{let active=true;setLoading(true);setError('');const query=new URLSearchParams({page:String(page),pageSize:'10',...Object.fromEntries(Object.entries(filter).filter(([,v])=>v))});api<typeof data>('/sales?'+query,token).then(d=>{if(active)setData(d);}).catch(e=>{if(active)setError(errorText(e));}).finally(()=>{if(active)setLoading(false);});return()=>{active=false;};},[token,page,filter]);
  function apply(next:typeof empty){setDraft(next);setFilter(next);setPage(1);}
  return <><Heading tag="Rejestr sprzedaży" title="Historia sprzedaży" description={admin?'Wszystkie transakcje firmy. Kwoty netto.':'Twoje transakcje — od najnowszej do najstarszej. Kwoty netto.'}/>
    <div className="report-presets">{presets.map(([key,label])=><button key={key} className="secondary" onClick={()=>apply({...draft,...dateRange(key)})}>{label}</button>)}<button className="secondary" onClick={()=>apply(empty)}>Wyczyść filtry</button></div>
    <section className="panel history-panel"><div className="section-header"><h2>Zapisane transakcje <span className="badge">{data.total}</span></h2></div>
      <form className="filters" onSubmit={e=>{e.preventDefault();apply({...draft});}}>
        {admin&&<label>Sprzedawca<select value={draft.seller} onChange={e=>apply({...draft,seller:e.target.value})}><option value="">Wszyscy sprzedawcy</option>{sellers.map(s=><option key={s.key} value={s.key}>{s.name}</option>)}</select></label>}
        <label>Samochód<select value={draft.carId} onChange={e=>apply({...draft,carId:e.target.value})}><option value="">Wszystkie samochody</option>{cars.map(c=><option key={c.id} value={c.id}>{c.make} {c.model}</option>)}</select></label>
        <label className="search">Szukaj<input value={draft.search} onChange={e=>setDraft({...draft,search:e.target.value})} placeholder="Samochód, część lub sprzedawca…"/></label>
        <label>Od<input type="date" value={draft.from} max={draft.to||undefined} onChange={e=>setDraft({...draft,from:e.target.value})}/></label>
        <label>Do<input type="date" value={draft.to} min={draft.from||undefined} onChange={e=>setDraft({...draft,to:e.target.value})}/></label><button className="secondary">Filtruj</button>
      </form><Notice text={error}/>{loading?<div className="empty">Ładowanie historii…</div>:!error&&<SalesList items={data.items}/>}
      <div className="pagination"><small>{data.total} transakcji · strona {page} z {Math.max(1,Math.ceil(data.total/10))}</small><div><button className="secondary" disabled={page===1||loading} onClick={()=>setPage(p=>p-1)}>← Wstecz</button><button className="secondary" disabled={page*10>=data.total||loading} onClick={()=>setPage(p=>p+1)}>Dalej →</button></div></div>
    </section></>;
}
