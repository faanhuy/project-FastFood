import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import AdminLayout from '../../components/AdminLayout';
import { couponService } from '../../services/couponService';
import type { CouponDto, CreateCouponRequest } from '../../services/couponService';
import { formatPrice, formatDateTime } from '../../utils/formatters';
import { getApiError } from '../../utils/errorHandler';

// DISCOUNT_TYPES labels are now handled dynamically via getDiscountTypeLabel function since we need i18n
const DISCOUNT_TYPES = [
  { value: 1 as const },
  { value: 2 as const },
];

const INPUT_CLS =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-rose-400 focus:outline-none';

const EMPTY_FORM: CreateCouponRequest = {
  code: '',
  discountType: 1,
  discountValue: 0,
  minOrderValue: 0,
  maxUsage: 1,
  expiresAt: '',
  description: '',
};

// ISO date string → "YYYY-MM-DDThh:mm" for datetime-local input
function toInputValue(iso: string) {
  if (!iso) return '';
  return iso.slice(0, 16);
}

// Earliest expiry for the datetime-local input (today)
function minExpiry() {
  const d = new Date();
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().slice(0, 16);
}

export default function AdminCouponsPage() {
  const { t } = useTranslation('admin');
  const { t: tToast } = useTranslation('toast');
  const [coupons, setCoupons] = useState<CouponDto[]>([]);
  const [loading, setLoading] = useState(true);

  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState<CreateCouponRequest>(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      setCoupons(await couponService.getAll());
    } catch {
      toast.error(t('error', { ns: 'common' }));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setForm(EMPTY_FORM);
    setFormError(null);
    setShowCreate(true);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setSaving(true);
    try {
      await couponService.create({
        ...form,
        description: form.description?.trim() || undefined,
        expiresAt: new Date(form.expiresAt).toISOString(),
      });
      setShowCreate(false);
      toast.success(tToast('couponCreated'));
      await load();
    } catch (err) {
      setFormError(getApiError(err, t('adminCouponErrorCreateFailed')));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (code: string) => {
    if (!confirm(t('confirmDeleteCoupon', { code }))) return;
    try {
      await couponService.remove(code);
      toast.success(tToast('couponDeleted'));
      await load();
    } catch {
      toast.error(t('error', { ns: 'common' }));
    }
  };

  const isExpired = (expiresAt: string) => new Date(expiresAt) < new Date();

  return (
    <AdminLayout title={t('manageCoupons')}>
      <div className="flex justify-end mb-4">
        <button
          onClick={openCreate}
          className="bg-rose-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-rose-700"
        >
          + {t('createCoupon')}
        </button>
      </div>

      {/* Create Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4 overflow-y-auto">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 my-4">
            <h2 className="text-lg font-semibold mb-4">{t('createCoupon')}</h2>
            <form onSubmit={handleCreate} className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('couponCode')}</label>
                <input
                  required
                  className={INPUT_CLS}
                  placeholder="VD: SALE20"
                  value={form.code}
                  onChange={(e) => setForm((f) => ({ ...f, code: e.target.value.toUpperCase() }))}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('discountType')}</label>
                <select
                  className={INPUT_CLS}
                  value={form.discountType}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, discountType: Number(e.target.value) as 1 | 2 }))
                  }
                >
                  <option value={1}>{t('discountTypePercent')}</option>
                  <option value={2}>{t('discountTypeFixed')}</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('discountValue')} {form.discountType === 1 ? '(%)' : '(VNĐ)'}
                  </label>
                  <input
                    required
                    type="number"
                    min={1}
                    max={form.discountType === 1 ? 100 : undefined}
                    className={INPUT_CLS}
                    value={form.discountValue || ''}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, discountValue: Number(e.target.value) }))
                    }
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('minOrderValue')}
                  </label>
                  <input
                    required
                    type="number"
                    min={0}
                    className={INPUT_CLS}
                    value={form.minOrderValue || ''}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, minOrderValue: Number(e.target.value) }))
                    }
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('maxUsage')}
                  </label>
                  <input
                    required
                    type="number"
                    min={1}
                    className={INPUT_CLS}
                    value={form.maxUsage || ''}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, maxUsage: Number(e.target.value) }))
                    }
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">{t('expiryDate')}</label>
                  <input
                    required
                    type="datetime-local"
                    min={minExpiry()}
                    className={INPUT_CLS}
                    value={toInputValue(form.expiresAt)}
                    onChange={(e) => setForm((f) => ({ ...f, expiresAt: e.target.value }))}
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('descriptionLabel')} <span className="text-gray-400">{t('optional')}</span>
                </label>
                <input
                  className={INPUT_CLS}
                  placeholder={t('descriptionPlaceholder')}
                  value={form.description ?? ''}
                  onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                />
              </div>

              {formError && <p className="text-sm text-red-500">{formError}</p>}

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreate(false)}
                  className="px-4 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  {t('cancel', { ns: 'common' })}
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="px-4 py-2 text-sm bg-rose-600 text-white rounded-lg hover:bg-rose-700 disabled:opacity-60"
                >
                  {saving ? t('creating') : t('createCoupon')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Table */}
      {loading ? (
        <div className="flex items-center justify-center h-64 text-gray-400">{t('loading', { ns: 'common' })}</div>
      ) : coupons.length === 0 ? (
        <div className="flex items-center justify-center h-64 text-gray-400">
          {t('noData', { ns: 'common' })}
        </div>
      ) : (
        <div className="bg-white rounded-xl shadow-sm overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">{t('code')}</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">{t('type')}</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">{t('value')}</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600 hidden md:table-cell">
                  {t('minOrder')}
                </th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">{t('used')}</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600 hidden lg:table-cell">
                  {t('expiry')}
                </th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">{t('status')}</th>
                <th className="text-right px-4 py-3 font-medium text-gray-600">{t('actions')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {coupons.map((c) => {
                const expired = isExpired(c.expiresAt);
                const exhausted = c.usedQuantity >= c.maxUsage;
                const active = !expired && !exhausted;
                return (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <span className="font-mono font-semibold text-gray-800">{c.code}</span>
                      {c.description && (
                        <p className="text-xs text-gray-400 mt-0.5 line-clamp-1">{c.description}</p>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {c.discountType === 1 ? t('percent') : t('fixed')}
                    </td>
                    <td className="px-4 py-3 font-medium text-rose-600">
                      {c.discountType === 1
                        ? `${c.discountValue}%`
                        : formatPrice(c.discountValue)}
                    </td>
                    <td className="px-4 py-3 text-gray-600 hidden md:table-cell">
                      {c.minOrderValue > 0 ? formatPrice(c.minOrderValue) : '—'}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {c.usedQuantity}/{c.maxUsage}
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-xs hidden lg:table-cell">
                      {formatDateTime(c.expiresAt)}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                          active
                            ? 'bg-green-100 text-green-700'
                            : expired
                            ? 'bg-gray-100 text-gray-500'
                            : 'bg-orange-100 text-orange-600'
                        }`}
                      >
                        {active ? t('active') : expired ? t('expired') : t('exhausted')}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => handleDelete(c.code)}
                        className="text-red-500 hover:underline text-xs"
                      >
                        {t('delete')}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </AdminLayout>
  );
}
