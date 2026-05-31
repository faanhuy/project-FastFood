import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import HttpBackend from 'i18next-http-backend';

// Read persisted language from Zustand store's localStorage entry.
// Zustand persist stores { state: { language: 'vi' }, version: 0 } — not a raw string.
function getPersistedLanguage(): 'vi' | 'en' {
  try {
    const raw = localStorage.getItem('smartshop_language');
    if (raw) {
      const parsed = JSON.parse(raw) as { state?: { language?: string } };
      const lang = parsed?.state?.language;
      if (lang === 'vi' || lang === 'en') return lang;
    }
  } catch {
    // ignore parse errors
  }
  return 'vi';
}

i18n
  .use(HttpBackend)
  .use(initReactI18next)
  .init({
    lng: getPersistedLanguage(),
    fallbackLng: 'vi',
    supportedLngs: ['vi', 'en'],
    defaultNS: 'common',
    ns: ['common', 'auth', 'product', 'cart', 'order', 'admin', 'validation', 'toast'],
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
    interpolation: {
      escapeValue: false,
    },
    saveMissing: import.meta.env.DEV,
  });

export default i18n;
