import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useLanguageStore } from '../store/useLanguageStore';
import '../pages/LoginPage.css';

/* ── Types ── */
interface ImageParticle {
  id: number; imageUrl: string; label: string;
  size: number; left: number; duration: number; delay: number; rotate: number;
}

const LANGS = [
  { code: 'vi' as const, label: 'Tiếng Việt', flag: '★', flagClass: '' },
  { code: 'en' as const, label: 'English',     flag: 'EN', flagClass: 'en' },
];

const QUICK_REPLIES_KEYS = ['chatQuick1', 'chatQuick2', 'chatQuick3'] as const;

const rand = (min: number, max: number) => min + Math.random() * (max - min);

function assignParticleLayout(items: { imageUrl: string; label: string }[]): ImageParticle[] {
  return items.map((item, id) => ({
    id, ...item,
    size: rand(56, 88), left: rand(2, 88),
    duration: rand(14, 26), delay: -rand(0, 24), rotate: rand(-25, 25),
  }));
}

/* ── Icons ── */
const ChevronIcon = ({ className }: { className?: string }) => (
  <svg className={className} width="16" height="16" viewBox="0 0 24 24" fill="none">
    <path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

/* ── Brand Panel ── */
function BrandPanel() {
  const { t } = useTranslation('auth');
  const [particles, setParticles] = useState<ImageParticle[]>([]);

  useEffect(() => {
    const BASE = import.meta.env.VITE_API_URL ?? 'https://localhost:7046/api';
    Promise.allSettled([
      fetch(`${BASE}/products?page=1&pageSize=30`).then((r) => r.json()),
      fetch(`${BASE}/combos?page=1&pageSize=20`).then((r) => r.json()),
    ]).then(([prodResult, comboResult]) => {
      const images: { imageUrl: string; label: string }[] = [];
      if (prodResult.status === 'fulfilled') {
        (prodResult.value?.data?.items ?? []).forEach((p: any) => {
          if (p.imageUrl) images.push({ imageUrl: p.imageUrl, label: p.name ?? '' });
        });
      }
      if (comboResult.status === 'fulfilled') {
        (comboResult.value?.data?.items ?? []).forEach((c: any) => {
          if (c.imageUrl) images.push({ imageUrl: c.imageUrl, label: c.name ?? '' });
        });
      }
      if (images.length >= 4) {
        setParticles(assignParticleLayout(images.sort(() => Math.random() - 0.5).slice(0, 16)));
      }
    });
  }, []);

  return (
    <section className="lp-brand">
      <div className="lp-brand-bg" />
      <span className="lp-blob b1" /><span className="lp-blob b2" /><span className="lp-blob b3" />
      <div className="lp-food-layer">
        {particles.map((p) => (
          <div key={p.id} className="lp-food lp-food-img" style={{
            '--d': p.duration + 's', '--delay': p.delay + 's', '--r': p.rotate + 'deg',
            left: p.left + '%', width: p.size + 'px', height: p.size + 'px',
          } as React.CSSProperties}>
            <img src={p.imageUrl} alt={p.label} draggable={false}
              onError={(e) => { (e.currentTarget.parentElement as HTMLElement).style.display = 'none'; }} />
          </div>
        ))}
      </div>

      <Link to="/products" className="lp-logo lp-reveal lp-d1" style={{ textDecoration: 'none', color: 'inherit' }}>
        <div className="lp-logo-mark">S</div>
        <div className="lp-logo-name">SmartShop</div>
      </Link>

      <div className="lp-brand-mid">
        <span className="lp-badge lp-reveal lp-d2">
          <span className="lp-dot" />{t('brandBadge')}
        </span>
        <h1 className="lp-headline lp-reveal lp-d3">
          {t('brandHeadline1')} <span className="lp-hl">{t('brandHeadline2')}</span>, {t('brandHeadline3')}
        </h1>
        <p className="lp-sub lp-reveal lp-d4">{t('brandSub')}</p>
        <div className="lp-foot lp-reveal lp-d5">
          <div className="lp-avatars">
            <span>🧑‍🍳</span><span>👩</span><span>🧑</span><span>👵</span>
          </div>
          <div className="lp-foot-txt">
            <div className="lp-stars">★★★★★</div>
            <div><b>50.000+</b> {t('brandCustomers')}</div>
          </div>
        </div>
      </div>
    </section>
  );
}

/* ── Language Switcher ── */
function LangSwitcher() {
  const { language, setLanguage } = useLanguageStore();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const current = LANGS.find((l) => l.code === language) ?? LANGS[0];

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('click', handler);
    return () => document.removeEventListener('click', handler);
  }, []);

  return (
    <div className={`lp-lang${open ? ' open' : ''}`} ref={ref}>
      <button className="lp-lang-btn" type="button"
        onClick={(e) => { e.stopPropagation(); setOpen((o) => !o); }}>
        <span className={`lp-flag${current.flagClass ? ' ' + current.flagClass : ''}`}>{current.flag}</span>
        <span>{current.label}</span>
        <ChevronIcon className="lp-chev" />
      </button>
      <div className="lp-lang-menu">
        {LANGS.map((l) => (
          <button key={l.code} type="button"
            className={`lp-lang-item${l.code === language ? ' active' : ''}`}
            onClick={() => { setLanguage(l.code); setOpen(false); }}>
            <span className={`lp-flag${l.flagClass ? ' ' + l.flagClass : ''}`}>{l.flag}</span>
            {l.label}
          </button>
        ))}
      </div>
    </div>
  );
}

/* ── Chat Widget ── */
function ChatWidget() {
  const { t } = useTranslation('auth');
  const [open, setOpen] = useState(false);

  return (
    <>
      <div className={`lp-chat-panel${open ? ' open' : ''}`}>
        <div className="lp-chat-head">
          <div className="lp-chat-ava">🛍️</div>
          <div>
            <h4>{t('chatTitle')}</h4>
            <p><span className="lp-dot" />{t('chatOnline')}</p>
          </div>
        </div>
        <div className="lp-chat-body">
          <div className="lp-bubble">{t('chatGreeting')}</div>
          <div className="lp-quicks">
            {QUICK_REPLIES_KEYS.map((k) => (
              <button key={k} className="lp-quick" type="button">{t(k)}</button>
            ))}
          </div>
        </div>
        <div className="lp-chat-input">
          <input type="text" placeholder={t('chatPlaceholder')} />
          <button className="lp-chat-send" type="button" aria-label={t('chatPlaceholder')}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
              <path d="M4 12l16-7-7 16-2-7-7-2Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round"/>
            </svg>
          </button>
        </div>
      </div>
      <button className={`lp-chat-fab${open ? ' open' : ''}`} aria-label={t('chatTitle')}
        onClick={() => setOpen((o) => !o)}>
        {open ? (
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round"/>
          </svg>
        ) : (
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none">
            <path d="M21 11.5a8.38 8.38 0 0 1-8.5 8.5 8.5 8.5 0 0 1-3.8-.9L3 21l1.9-5.7A8.38 8.38 0 0 1 4 11.5 8.5 8.5 0 0 1 12.5 3 8.38 8.38 0 0 1 21 11.5Z" stroke="currentColor" strokeWidth="1.9" strokeLinejoin="round"/>
          </svg>
        )}
      </button>
    </>
  );
}

/* ── Auth Page Layout ── */
interface Props {
  children: React.ReactNode;
}

export default function AuthPageLayout({ children }: Props) {
  const { t } = useTranslation('auth');
  return (
    <>
      <div className="lp-shell">
        <BrandPanel />
        <section className="lp-panel">
          <div className="lp-panel-top lp-reveal lp-d1">
            <LangSwitcher />
          </div>
          <div className="lp-form-wrap">
            {children}
          </div>
          <div className="lp-copy">© {new Date().getFullYear()} SmartShop. {t('brandSub2', 'Mua sắm thông minh mỗi ngày.')}</div>
        </section>
      </div>
      <ChatWidget />
    </>
  );
}
