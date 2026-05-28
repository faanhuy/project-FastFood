import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import HttpBackend from 'i18next-http-backend';

i18n
  .use(HttpBackend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'vi',
    supportedLngs: ['vi', 'en'],
    defaultNS: 'common',
    ns: ['common', 'auth', 'product', 'cart', 'order', 'admin', 'validation', 'toast'],
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'smartshop_language',
    },
    interpolation: {
      escapeValue: false,
    },
    saveMissing: import.meta.env.DEV,
  });

export default i18n;
