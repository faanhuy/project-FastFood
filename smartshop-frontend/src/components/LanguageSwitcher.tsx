import { useState, useEffect, useRef } from 'react';
import { useLanguageStore } from '../store/useLanguageStore';

const LANGS = [
  { code: 'vi' as const, label: 'Tiếng Việt', flag: '★', flagBg: '#DA251D', flagColor: '#FFFF00' },
  { code: 'en' as const, label: 'English',     flag: 'EN', flagBg: '#1A3A8F', flagColor: '#fff' },
];

export default function LanguageSwitcher() {
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
    <div ref={ref} style={{ position: 'relative' }}>
      {/* trigger button */}
      <button
        type="button"
        onClick={(e) => { e.stopPropagation(); setOpen((o) => !o); }}
        style={{
          display: 'flex', alignItems: 'center', gap: 8,
          padding: '7px 12px', borderRadius: 10,
          border: '1px solid #EFE6E8', background: '#fff',
          fontFamily: 'inherit', fontSize: 13.5, fontWeight: 600, color: '#1B1114',
          cursor: 'pointer',
          boxShadow: '0 1px 2px rgba(27,17,20,.04)',
          transition: 'border-color .2s, box-shadow .2s',
        }}
        onMouseEnter={(e) => {
          (e.currentTarget as HTMLButtonElement).style.borderColor = '#E3C4CB';
          (e.currentTarget as HTMLButtonElement).style.boxShadow = '0 6px 16px -8px rgba(225,29,72,.3)';
        }}
        onMouseLeave={(e) => {
          (e.currentTarget as HTMLButtonElement).style.borderColor = '#EFE6E8';
          (e.currentTarget as HTMLButtonElement).style.boxShadow = '0 1px 2px rgba(27,17,20,.04)';
        }}
      >
        <span style={{
          width: 22, height: 15, borderRadius: 3, display: 'grid', placeItems: 'center',
          background: current.flagBg, color: current.flagColor,
          fontSize: current.code === 'vi' ? 11 : 9, fontWeight: 800, lineHeight: 1,
          boxShadow: 'inset 0 0 0 1px rgba(0,0,0,.06)', flexShrink: 0,
        }}>{current.flag}</span>
        <span>{current.label}</span>
        <svg
          width="14" height="14" viewBox="0 0 24 24" fill="none"
          style={{ color: '#8C7B80', transition: 'transform .25s', transform: open ? 'rotate(180deg)' : 'rotate(0)' }}
        >
          <path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/>
        </svg>
      </button>

      {/* dropdown */}
      <div style={{
        position: 'absolute', right: 0, top: 'calc(100% + 8px)', width: 178,
        background: '#fff', border: '1px solid #EFE6E8', borderRadius: 14,
        boxShadow: '0 24px 60px -28px rgba(27,17,20,.28)', padding: 6, zIndex: 50,
        opacity: open ? 1 : 0,
        transform: open ? 'translateY(0) scale(1)' : 'translateY(-8px) scale(.97)',
        pointerEvents: open ? 'auto' : 'none',
        transition: 'all .2s cubic-bezier(.22,1,.36,1)',
        transformOrigin: 'top right',
      }}>
        {LANGS.map((l) => (
          <button
            key={l.code}
            type="button"
            onClick={() => { setLanguage(l.code); setOpen(false); }}
            style={{
              display: 'flex', alignItems: 'center', gap: 10, width: '100%',
              padding: '9px 10px', borderRadius: 9, border: 0,
              background: l.code === language ? '#FFF1F3' : 'transparent',
              fontFamily: 'inherit', fontSize: 13.5,
              fontWeight: l.code === language ? 600 : 500,
              color: l.code === language ? '#E11D48' : '#1B1114',
              cursor: 'pointer', textAlign: 'left',
              transition: 'background .15s',
            }}
            onMouseEnter={(e) => {
              if (l.code !== language)
                (e.currentTarget as HTMLButtonElement).style.background = '#F6F1F2';
            }}
            onMouseLeave={(e) => {
              if (l.code !== language)
                (e.currentTarget as HTMLButtonElement).style.background = 'transparent';
            }}
          >
            <span style={{
              width: 22, height: 15, borderRadius: 3, display: 'grid', placeItems: 'center',
              background: l.flagBg, color: l.flagColor,
              fontSize: l.code === 'vi' ? 11 : 9, fontWeight: 800, lineHeight: 1,
              boxShadow: 'inset 0 0 0 1px rgba(0,0,0,.06)', flexShrink: 0,
            }}>{l.flag}</span>
            {l.label}
          </button>
        ))}
      </div>
    </div>
  );
}
