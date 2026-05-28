import { useLanguageStore } from '../store/useLanguageStore';

const FLAG_VI = '🇻🇳';
const FLAG_EN = '🇬🇧';

export default function LanguageSwitcher() {
  const { language, setLanguage } = useLanguageStore();

  return (
    <button
      onClick={() => setLanguage(language === 'vi' ? 'en' : 'vi')}
      className="flex items-center gap-1 px-2 py-1 text-sm rounded hover:bg-gray-100 transition-colors"
      title={language === 'vi' ? 'Switch to English' : 'Chuyển sang Tiếng Việt'}
    >
      <span className="text-base">{language === 'vi' ? FLAG_VI : FLAG_EN}</span>
      <span className="font-medium uppercase text-gray-700">{language}</span>
    </button>
  );
}
