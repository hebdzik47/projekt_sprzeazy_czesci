import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { api, errorText, money } from './services';
import type { Car, Part, Summary, Group } from './services';
import { Heading, Notice } from './components';

export { SalePage } from './SalePage';
export { HistoryPage } from './HistoryPage';
import { dateRange, presets } from './dateRanges';

export function CatalogPage({token,cars,reload}:{token:string;cars:Car[];reload:()=>Promise<void>}) {
  const [makes,setMakes]=useState<string[]>([]);
  useEffect(()=>{api<string[]>('/car-makes',token).then(setMakes).catch(e=>setError(errorText(e)));},[token]);
  const [parts,setParts]=useState<Part[]>([]),[error,setError]=useState(''),[success,setSuccess]=useState(''),[busy,setBusy]=useState(false);
  useEffect(()=>{api<Part[]>('/parts',token).then(setParts).catch(e=>setError(errorText(e)));},[token]);
  async function save(e:FormEvent<HTMLFormElement>,kind:string){e.preventDefault();if(busy)return;const form=e.currentTarget,f=new FormData(form);setBusy(true);setError('');setSuccess('');try{if(kind==='car')await api('/cars',token,{make:f.get('make'),model:f.get('model')});else await api('/parts',token,{name:f.get('name')});form.reset();setSuccess('Zmiany zostały zapisane.');await reload();setParts(await api<Part[]>('/parts',token));}catch(e){setError(errorText(e));}finally{setBusy(false);}}
  const groups = Object.entries(cars.reduce<Record<string,Car[]>>((result,car)=>{const make=car.make.trim().toLocaleLowerCase('pl-PL');(result[make]??=[]).push(car);return result;},{})).sort(([a],[b])=>a.localeCompare(b,'pl',{sensitivity:'base'}));
  return <><Heading tag="Administracja" title="Samochody i części" description="Wspólna lista części dla wszystkich samochodów. Samochody uporządkowane alfabetycznie według marki."/><Notice text={error}/><Notice text={success} success/><div className="catalog-grid"><form className="panel stack" onSubmit={e=>save(e,'car')}><span className="number">01</span><h2>Dodaj samochód</h2><label>Marka<select name="make" defaultValue="" required><option value="">Wybierz markę</option>{makes.map(make=><option key={make} value={make}>{make}</option>)}</select></label><label>Model<input name="model" placeholder="np. A6 C5" maxLength={100} required/></label><button disabled={busy}>Dodaj samochód</button></form><form className="panel stack" onSubmit={e=>save(e,'part')}><span className="number">02</span><h2>Dodaj część</h2><label>Nazwa części<input name="name" placeholder="np. Prawe lusterko" maxLength={200} required/></label><p className="muted">Ta część od razu będzie dostępna dla każdego samochodu.</p><button disabled={busy}>Dodaj część</button></form></div><section className="panel"><h2>Katalog samochodów <span className="badge">{cars.length}</span></h2><div className="make-groups">{groups.map(([make,models])=><section className="make-group" key={make}><h3>{make==='vw'?'vw':make==='bmw'?'BMW':make.charAt(0).toUpperCase()+make.slice(1)} <span className="badge">{models.length}</span></h3><div className="car-chips">{[...models].sort((a,b)=>a.model.localeCompare(b.model,'pl',{sensitivity:'base',numeric:true})).map(c=><span key={c.id}>{c.model}</span>)}</div></section>)}</div></section><section className="panel"><h2>Katalog części <span className="badge">{parts.length}</span></h2><div className="car-chips">{parts.map(p=><span key={p.id}>{p.name}</span>)}</div></section></>;
}

function Breakdown({title,rows}:{title:string;rows:Group[]}){const max=Math.max(1,...rows.map(r=>r.value));return <section className="panel"><h2>{title}</h2>{!rows.length?<p className="muted">Brak sprzedaży w tym okresie.</p>:rows.map((r,i)=><div className="breakdown" key={`${r.name}-${i}`}><div><strong>{r.name}</strong><span>{money(r.value)}</span></div><div className="bar"><i style={{width:`${r.value/max*100}%`}}/></div><small>{r.count} sprzedanych części</small></div>)}</section>;}
export function ReportPage({token}:{token:string}) {
  const [pdf,setPdf]=useState<{url:string;filename:string}|null>(null);
  useEffect(()=>()=>{if(pdf)URL.revokeObjectURL(pdf.url);},[pdf]);
  const initial=dateRange('month');
  const [range,setRange]=useState(initial),[draft,setDraft]=useState(initial),[data,setData]=useState<Summary|null>(null),[error,setError]=useState(''),[busy,setBusy]=useState(true),[exporting,setExporting]=useState(false),[pdfError,setPdfError]=useState('');
  useEffect(()=>{let active=true;setBusy(true);setError('');setPdfError('');setPdf(null);api<Summary>('/reports/summary?'+new URLSearchParams(range),token).then(d=>{if(active)setData(d);}).catch(e=>{if(active)setError(errorText(e));}).finally(()=>{if(active)setBusy(false);});return()=>{active=false;};},[token,range]);
  const changed=draft.from!==range.from||draft.to!==range.to;
  function preset(kind:string) { const next=dateRange(kind);setDraft(next);setRange(next); }
  async function download() {
    if(!data||busy||changed||exporting)return;
    setExporting(true);setPdfError('');
    try { const {downloadReportPdf}=await import('./reportPdf');setPdf(await downloadReportPdf(data,range)); }
    catch(e){setPdfError(errorText(e));}finally{setExporting(false);}
  }
  return <><Heading tag="Wyniki firmy" title="Raporty sprzedaży" description="Sprawdź wyniki firmy i pobierz raport PDF z wybranego okresu."/>
    <div className="report-presets">{presets.map(([key,label])=><button key={key} className="secondary" disabled={exporting} onClick={()=>preset(key)}>{label}</button>)}</div>
    <form className="panel filters" onSubmit={e=>{e.preventDefault();if(draft.from>draft.to){setPdfError('Data od nie może być późniejsza od daty do.');return;}setRange({...draft});}}>
      <label>Data od<input type="date" value={draft.from} max={draft.to} onChange={e=>setDraft({...draft,from:e.target.value})} disabled={exporting} required/></label>
      <label>Data do<input type="date" value={draft.to} min={draft.from} onChange={e=>setDraft({...draft,to:e.target.value})} disabled={exporting} required/></label>
      <button disabled={busy||exporting}>Pokaż zestawienie</button>
      <button type="button" className="secondary" disabled={busy||exporting||!data||!!error||changed} onClick={download}>{exporting?'Tworzenie PDF…':'Pobierz PDF ↓'}</button>
      {changed&&<small className="export-hint">Zastosuj daty przyciskiem „Pokaż zestawienie”, aby pobrać raport dla nowego okresu.</small>}
    </form><Notice text={error}/><Notice text={pdfError}/>
    {pdf&&<div className="notice success" role="status">PDF jest gotowy. Jeśli pobieranie nie rozpoczęło się automatycznie: <a href={pdf.url} download={pdf.filename}>pobierz {pdf.filename}</a>.</div>}
    {busy?<div className="empty">Obliczanie wyników…</div>:data&&!error&&<><div className="metrics"><article><small>Wartość netto</small><strong>{money(data.totalValue)}</strong><span>Łącznie w wybranym okresie</span></article><article><small>Sprzedane części</small><strong>{data.partsCount}</strong><span>Łączna liczba sprzedanych sztuk</span></article></div><div className="report-grid"><Breakdown title="Sprzedaż według sprzedawców" rows={data.byUser}/><Breakdown title="Sprzedaż według samochodów" rows={data.byCar}/></div></>}</>;
}
