import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { createRoot } from 'react-dom/client';
import { api, errorText } from './services';
import type { Session, Car } from './services';
import { Notice } from './components';
import { SalePage, HistoryPage, CatalogPage, ReportPage } from './pages';
import './style.css';

function Login({ onLogin, expired }: { onLogin: (session: Session) => void; expired: string }) {
  const [register, setRegister] = useState(false);
  const [error, setError] = useState(expired);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = new FormData(event.currentTarget);
    const password = String(form.get('password') ?? '');
    if (register && password !== form.get('confirmPassword')) {
      setError('Hasła muszą być takie same.');
      return;
    }
    setBusy(true);
    setError('');
    try {
      const path = register ? '/auth/register' : '/auth/login';
      const body = register
        ? { userName: form.get('login'), displayName: form.get('displayName'), password }
        : { userName: form.get('login'), password };
      onLogin(await api<Session>(path, '', body));
    } catch (cause) {
      setError(errorText(cause));
    } finally {
      setBusy(false);
    }
  }

  return <div className="login-page">
    <div className="login-brand"><div className="brand"><img className="brand-logo" src="/megamot-logo.png" alt="" width="42" height="42"/>Megamot</div><h1>Sprzedaż pod kontrolą.</h1><p>Samochody, części i wyniki Twojego zespołu.<br/>W jednym miejscu, na każdym urządzeniu.</p></div>
    <form className="login-card" onSubmit={submit}>
      <span className="eyebrow">{register ? 'Dołącz do zespołu' : 'Witaj ponownie'}</span>
      <h2>{register ? 'Załóż konto pracownika' : 'Zaloguj się do firmy'}</h2>
      <p className="muted">{register ? 'Nowe konto otrzyma dostęp sprzedawcy.' : 'Użyj konta administratora lub sprzedawcy.'}</p>
      <Notice text={error}/>
      <label>Login<input name="login" autoComplete="username" placeholder="3–100 znaków" minLength={3} maxLength={100} pattern="[a-zA-Z0-9._-]+" required/></label>
      {register && <label>Imię i nazwisko<input name="displayName" autoComplete="name" minLength={2} maxLength={100} required/></label>}
      <label>Hasło<input name="password" type="password" autoComplete={register ? 'new-password' : 'current-password'} minLength={register ? 8 : undefined} maxLength={100} required/></label>
      {register && <label>Powtórz hasło<input name="confirmPassword" type="password" autoComplete="new-password" minLength={8} maxLength={100} required/></label>}
      <button disabled={busy}>{busy ? (register ? 'Tworzenie konta…' : 'Logowanie…') : (register ? 'Utwórz konto' : 'Zaloguj się →')}</button>
      <button type="button" className="secondary auth-switch" disabled={busy} onClick={() => { setRegister(!register); setError(''); }}>{register ? 'Mam już konto — zaloguj się' : 'Nie masz konta? Zarejestruj się'}</button>
      <small>Dostęp wyłącznie dla pracowników firmy.</small>
    </form>
  </div>;
}

function App() {
  const [session, setSession] = useState<Session | null>(null);
  const [tab, setTab] = useState('sale');
  const [cars, setCars] = useState<Car[]>([]);
  const [error, setError] = useState('');
  const [expired, setExpired] = useState('');
  const admin = !!session?.user.roles.includes('Administrator');
  const reload = async () => { if (session) setCars(await api<Car[]>('/cars', session.token)); };

  useEffect(() => { reload().catch(cause => setError(errorText(cause))); }, [session]);
  useEffect(() => {
    const expire = () => { setSession(null); setExpired('Sesja wygasła. Zaloguj się ponownie.'); };
    window.addEventListener('session-expired', expire);
    return () => window.removeEventListener('session-expired', expire);
  }, []);

  if (!session) return <Login expired={expired} onLogin={value => { setSession(value); setTab('sale'); setError(''); setExpired(''); }}/>;
  const links: [string, string, string][] = [['sale', '＋', 'Nowa sprzedaż'], ['history', '≡', 'Historia sprzedaży'], ...(admin ? [['catalog', '▦', 'Samochody i części'], ['report', '▤', 'Raporty']] as [string, string, string][] : [])];
  return <div className="app"><aside className="sidebar"><div className="brand"><img className="brand-logo" src="/megamot-logo.png" alt="" width="42" height="42"/>Megamot</div><div className="workspace">PANEL FIRMY</div><nav aria-label="Menu główne">{links.map(([id, icon, label]) => <button key={id} className={tab === id ? 'active' : ''} aria-current={tab === id ? 'page' : undefined} onClick={() => setTab(id)}><span aria-hidden="true">{icon}</span>{label}</button>)}</nav><div className="account"><span className="avatar">{session.user.displayName.slice(0, 1)}</span><div><strong>{session.user.displayName}</strong><small>{admin ? 'Administrator' : 'Sprzedawca'}</small></div><button className="logout" onClick={() => { setSession(null); setCars([]); setExpired(''); }} aria-label="Wyloguj" title="Wyloguj">↪</button></div></aside><main><Notice text={error}/>{tab === 'sale' && <SalePage token={session.token} cars={cars} seller={session.user.displayName} admin={admin}/>} {tab === 'history' && <HistoryPage token={session.token} admin={admin} cars={cars}/>} {tab === 'catalog' && admin && <CatalogPage token={session.token} cars={cars} reload={reload}/>} {tab === 'report' && admin && <ReportPage token={session.token}/>}<footer>Megamot · sprzedaż części</footer></main></div>;
}

createRoot(document.getElementById('app')!).render(<App/>);
