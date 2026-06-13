import { useState, useCallback, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store/authStore';
import { authService } from '../services/authService';
import { getApiError, getApiErrors } from '../utils/errorHandler';
import AuthPageLayout from '../components/AuthPageLayout';
import { GoogleLoginButton } from '../components/GoogleLoginButton';

type SubmitState = 'idle' | 'loading' | 'success';

/* ── Icons ── */
const MailIcon = () => (
  <svg width="19" height="19" viewBox="0 0 24 24" fill="none">
    <rect x="3" y="5" width="18" height="14" rx="3" stroke="currentColor" strokeWidth="1.8"/>
    <path d="M4 7l8 6 8-6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
  </svg>
);
const LockIcon = () => (
  <svg width="19" height="19" viewBox="0 0 24 24" fill="none">
    <rect x="4" y="10" width="16" height="11" rx="3" stroke="currentColor" strokeWidth="1.8"/>
    <path d="M8 10V7a4 4 0 0 1 8 0v3" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
  </svg>
);
const EyeIcon = ({ off }: { off: boolean }) => off ? (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
    <path d="M3 3l18 18" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
    <path d="M10.6 6.2A9.7 9.7 0 0 1 12 5c6.4 0 10 7 10 7a16.8 16.8 0 0 1-3.3 3.9M6.5 7.5C3.7 9 2 12 2 12s3.6 7 10 7a9.6 9.6 0 0 0 3.4-.6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
  </svg>
) : (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
    <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" stroke="currentColor" strokeWidth="1.8"/>
    <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="1.8"/>
  </svg>
);
const ArrowIcon = ({ className }: { className?: string }) => (
  <svg className={className} width="19" height="19" viewBox="0 0 24 24" fill="none">
    <path d="M5 12h14M13 6l6 6-6 6" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);
const CheckIcon = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none">
    <path d="M5 12l5 5L20 6" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);
const AlertIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
    <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2"/>
    <line x1="12" y1="8" x2="12" y2="12" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <line x1="12" y1="16" x2="12.01" y2="16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

/* ── Forgot Password Modal ── */
function ForgotPasswordModal({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation(['auth', 'toast']);
  const [email, setEmail] = useState('');
  const [sending, setSending] = useState(false);
  const [done, setDone] = useState(false);

  const handleSubmit = useCallback(async (e: FormEvent) => {
    e.preventDefault();
    if (sending || !email.trim()) return;
    setSending(true);
    try {
      await authService.forgotPassword(email.trim());
      setDone(true);
    } catch (err) {
      toast.error(getApiError(err, t('toast:forgotPasswordFailed')));
    } finally {
      setSending(false);
    }
  }, [email, sending, t]);

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 px-4"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6"
        onClick={(e) => e.stopPropagation()}
      >
        {done ? (
          <div className="text-center">
            <div className="mx-auto mb-4 w-12 h-12 rounded-full bg-green-100 text-green-600 flex items-center justify-center">
              <CheckIcon />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">{t('forgotPasswordDoneTitle')}</h3>
            <p className="text-sm text-gray-500 mb-6">{t('forgotPasswordDoneDesc')}</p>
            <button
              type="button"
              onClick={onClose}
              className="w-full bg-rose-600 text-white py-2.5 rounded-xl text-sm font-semibold hover:bg-rose-700 transition-colors"
            >
              {t('backToLogin')}
            </button>
          </div>
        ) : (
          <>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">{t('forgotPasswordTitle')}</h3>
            <p className="text-sm text-gray-500 mb-5">{t('forgotPasswordDesc')}</p>
            <form onSubmit={handleSubmit} noValidate className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="fp-email">{t('email')}</label>
                <input
                  id="fp-email"
                  type="email"
                  inputMode="email"
                  autoComplete="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  className="w-full border border-gray-300 rounded-xl px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400"
                />
              </div>
              <div className="flex gap-3 pt-1">
                <button
                  type="button"
                  onClick={onClose}
                  className="flex-1 border border-gray-300 text-gray-700 py-2.5 rounded-xl text-sm font-medium hover:bg-gray-50 transition-colors"
                >
                  {t('backToLogin')}
                </button>
                <button
                  type="submit"
                  disabled={sending}
                  className="flex-1 bg-rose-600 text-white py-2.5 rounded-xl text-sm font-semibold hover:bg-rose-700 disabled:opacity-50 transition-colors"
                >
                  {sending ? t('forgotPasswordSending') : t('forgotPasswordSubmit')}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
}

export default function LoginPage() {
  const navigate = useNavigate();
  const { t } = useTranslation(['auth', 'toast']);
  const setAuth = useAuthStore((s) => s.setAuth);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPwd, setShowPwd] = useState(false);
  const [remember, setRemember] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [status, setStatus] = useState<SubmitState>('idle');
  const [showForgot, setShowForgot] = useState(false);
  const [googleLoading, setGoogleLoading] = useState(false);

  const handleSubmit = useCallback(async (e: FormEvent) => {
    e.preventDefault();
    if (status === 'loading') return;
    setErrors([]);
    setStatus('loading');
    try {
      const auth = await authService.login({ email, password });
      setAuth(auth);
      setStatus('success');
      setTimeout(() => navigate('/'), 600);
    } catch (err) {
      setErrors(getApiErrors(err, t('loginFailedDefault')));
      setStatus('idle');
    }
  }, [email, password, status, setAuth, navigate, t]);

  const handleGoogleSuccess = useCallback(async (idToken: string) => {
    setGoogleLoading(true);
    try {
      const auth = await authService.googleLogin(idToken);
      setAuth(auth);
      toast.success(t('toast:googleLoginSuccess'));
      setTimeout(() => navigate('/'), 600);
    } catch (err) {
      toast.error(getApiError(err, t('toast:googleLoginFailed')));
    } finally {
      setGoogleLoading(false);
    }
  }, [setAuth, navigate, t]);

  const btnLabel =
    status === 'loading' ? t('loggingIn') :
    status === 'success' ? t('loginSuccess2') :
    t('login');

  return (
    <AuthPageLayout>
      <div className="lp-card">
        <div className="lp-h-row lp-reveal lp-d3">
          <h2>{t('welcomeBack')}</h2>
          <span className="lp-wave">👋</span>
        </div>
        <p className="lp-lede lp-reveal lp-d3">{t('loginSubtitle')}</p>

        {errors.length > 0 && (
          <div className="lp-error lp-reveal lp-d3" style={{ marginTop: 20 }}>
            <span className="lp-error-icon"><AlertIcon /></span>
            <div>{errors.map((e, i) => <p key={i}>{e}</p>)}</div>
          </div>
        )}

        <form className="lp-form" onSubmit={handleSubmit} noValidate>
          <div className="lp-reveal lp-d4">
            <label className="lp-field-label" htmlFor="lp-email">{t('email')}</label>
            <div className="lp-input">
              <span className="lp-input-lead"><MailIcon /></span>
              <input id="lp-email" type="email" inputMode="email" autoComplete="email"
                placeholder="you@example.com" value={email}
                onChange={(e) => setEmail(e.target.value)} required />
            </div>
          </div>

          <div className="lp-reveal lp-d5">
            <label className="lp-field-label" htmlFor="lp-password">{t('password')}</label>
            <div className="lp-input">
              <span className="lp-input-lead"><LockIcon /></span>
              <input id="lp-password" type={showPwd ? 'text' : 'password'}
                autoComplete="current-password" placeholder={t('passwordPlaceholder')} value={password}
                onChange={(e) => setPassword(e.target.value)} required />
              <button className="lp-eye" type="button"
                aria-label={showPwd ? t('hidePassword') : t('showPassword')}
                onClick={() => setShowPwd((s) => !s)}>
                <EyeIcon off={showPwd} />
              </button>
            </div>
          </div>

          <div className="lp-row-between lp-reveal lp-d6">
            <label className="lp-remember">
              <input type="checkbox" checked={remember}
                onChange={(e) => setRemember(e.target.checked)} />
              <span className="lp-check"><CheckIcon /></span>
              <span className="lp-remember-txt">{t('rememberMe')}</span>
            </label>
            <a
              className="lp-forgot"
              href="#"
              onClick={(e) => { e.preventDefault(); setShowForgot(true); }}
            >
              {t('forgotPassword')}
            </a>
          </div>

          <button
            className={`lp-submit lp-reveal lp-d7${status === 'success' ? ' success' : ''}`}
            type="submit" disabled={status === 'loading'}>
            <span className="lp-submit-inner">
              {status === 'loading' && <span className="lp-spinner" />}
              <span>{btnLabel}</span>
              {status === 'idle' && <ArrowIcon className="lp-arrow" />}
            </span>
          </button>

          <div className="lp-reveal lp-d8 my-4">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-200" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-400">{t('orDivider')}</span>
              </div>
            </div>
          </div>

          <div className="lp-reveal lp-d8">
            <GoogleLoginButton
              onSuccess={handleGoogleSuccess}
              onError={() => toast.error(t('toast:googleLoginFailed'))}
              disabled={googleLoading}
            />
          </div>

          <p className="lp-signup lp-reveal lp-d8">
            {t('noAccount')}{' '}
            <Link to="/register">{t('register')}</Link>
          </p>
        </form>
      </div>

      {showForgot && <ForgotPasswordModal onClose={() => setShowForgot(false)} />}
    </AuthPageLayout>
  );
}
