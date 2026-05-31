import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiEdit2, FiPlus, FiX } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { storeService } from '../../services/storeService';
import type { AdminStore as Store, CreateStoreRequest, UpdateStoreRequest } from '../../types/store';
import { AddressSelector, type AddressSelection } from '@/components/AddressSelector';

// ─── Modal form state ─────────────────────────────────────────────────────────

interface FormState {
  name: string;
  phone: string;
  isActive: boolean;
  provinceId?: number;
  wardId?: number;
  provinceName?: string;
  wardName?: string;
  street: string;
}

interface FormErrors {
  name?: string;
  address?: string;
  phone?: string;
}

const EMPTY_FORM: FormState = {
  name: '',
  phone: '',
  isActive: true,
  street: '',
};

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function AdminStoresPage() {
  const { t } = useTranslation(['admin', 'common', 'toast']);
  const [stores, setStores] = useState<Store[]>([]);
  const [loading, setLoading] = useState(true);

  // Modal state
  const [modalOpen, setModalOpen] = useState(false);
  const [editingStore, setEditingStore] = useState<Store | null>(null);
  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  const loadStores = () => {
    setLoading(true);
    storeService
      .getAdminStores()
      .then(setStores)
      .catch(() => toast.error(t('storeLoadFailed', { ns: 'toast' })))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadStores();
  }, []);

  const openCreateModal = () => {
    setEditingStore(null);
    setForm(EMPTY_FORM);
    setErrors({});
    setModalOpen(true);
  };

  const openEditModal = (store: Store) => {
    setEditingStore(store);
    setForm({
      name: store.name,
      phone: store.phone,
      isActive: store.isActive ?? true,
      provinceId: store.provinceId,
      wardId: store.wardId,
      provinceName: store.provinceName,
      wardName: store.wardName,
      street: store.street ?? '',
    });
    setErrors({});
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingStore(null);
    setForm(EMPTY_FORM);
    setErrors({});
  };

  const handleAddressSelection = (selection: AddressSelection) => {
    setForm((f) => ({
      ...f,
      provinceId: selection.provinceId,
      wardId: selection.wardId,
      provinceName: selection.provinceName,
      wardName: selection.wardName,
      street: selection.street,
    }));
  };

  const buildAddressString = () => {
    const parts = [form.street, form.wardName, form.provinceName].filter(Boolean);
    return parts.join(', ');
  };

  const validate = (): boolean => {
    const next: FormErrors = {};
    if (!form.name.trim()) next.name = t('storeNameRequired');
    if (!form.provinceId || !form.wardId || !form.street.trim())
      next.address = t('storeAddressRequired');
    if (!form.phone.trim()) next.phone = t('storePhoneRequired');
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    setSubmitting(true);
    const address = buildAddressString();
    try {
      if (editingStore) {
        const body: UpdateStoreRequest = {
          name: form.name.trim(),
          address,
          phone: form.phone.trim(),
          isActive: form.isActive,
          provinceId: form.provinceId,
          wardId: form.wardId,
          provinceName: form.provinceName,
          wardName: form.wardName,
          street: form.street.trim(),
        };
        await storeService.updateStore(editingStore.id, body);
        toast.success(t('storeUpdated', { ns: 'toast' }));
      } else {
        const body: CreateStoreRequest = {
          name: form.name.trim(),
          address,
          phone: form.phone.trim(),
          provinceId: form.provinceId,
          wardId: form.wardId,
          provinceName: form.provinceName,
          wardName: form.wardName,
          street: form.street.trim(),
        };
        await storeService.createStore(body);
        toast.success(t('storeCreated', { ns: 'toast' }));
      }
      closeModal();
      loadStores();
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? t('error', { ns: 'common' }));
    } finally {
      setSubmitting(false);
    }
  };

  const displayAddress = (store: Store) => {
    if (store.street && store.wardName && store.provinceName) {
      return `${store.street}, ${store.wardName}, ${store.provinceName}`;
    }
    return store.address;
  };

  return (
    <AdminLayout title={t('manageStores')}>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-lg font-semibold text-gray-800">{t('storeListTitle')}</h2>
        <button
          onClick={openCreateModal}
          className="flex items-center gap-2 bg-rose-600 hover:bg-rose-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <FiPlus size={15} />
          {t('addStore')}
        </button>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">{t('loading', { ns: 'common' })}</div>
        ) : stores.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">{t('storeNoData')}</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('storeNameLabel')}</th>
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('storeAddressLabel')}</th>
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('storePhoneLabel')}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('status')}</th>
                <th className="text-right px-5 py-3 font-semibold text-gray-600">{t('actions')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {stores.map((store) => (
                <tr key={store.id} className="hover:bg-gray-50">
                  <td className="px-5 py-3 font-medium text-gray-800">{store.name}</td>
                  <td className="px-5 py-3 text-gray-600">{displayAddress(store)}</td>
                  <td className="px-5 py-3 text-gray-600">{store.phone}</td>
                  <td className="px-5 py-3 text-center">
                    {store.isActive !== false ? (
                      <span className="inline-block bg-green-100 text-green-700 text-xs px-2.5 py-0.5 rounded-full font-medium">
                        {t('active')}
                      </span>
                    ) : (
                      <span className="inline-block bg-gray-100 text-gray-500 text-xs px-2.5 py-0.5 rounded-full font-medium">
                        {t('inactive')}
                      </span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-right">
                    <button
                      onClick={() => openEditModal(store)}
                      className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 hover:text-rose-600 transition-colors"
                      title={t('edit')}
                    >
                      <FiEdit2 size={14} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal */}
      {modalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 max-h-[90vh] flex flex-col">
            {/* Modal header */}
            <div className="flex items-center justify-between px-6 py-4 border-b shrink-0">
              <h3 className="text-base font-semibold text-gray-800">
                {editingStore ? t('editStore') : t('createStore')}
              </h3>
              <button
                onClick={closeModal}
                disabled={submitting}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors disabled:opacity-50"
              >
                <FiX size={16} />
              </button>
            </div>

            {/* Modal body */}
            <div className="px-6 py-5 space-y-4 overflow-y-auto">
              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('storeNameLabel')} <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  placeholder={t('storeNamePlaceholder')}
                  className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400 ${
                    errors.name ? 'border-red-400' : 'border-gray-300'
                  }`}
                />
                {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
              </div>

              {/* Phone */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('storePhoneLabel')} <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.phone}
                  onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
                  placeholder={t('storePhonePlaceholder')}
                  className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400 ${
                    errors.phone ? 'border-red-400' : 'border-gray-300'
                  }`}
                />
                {errors.phone && <p className="mt-1 text-xs text-red-500">{errors.phone}</p>}
              </div>

              {/* Address via AddressSelector */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  {t('storeAddressLabel')} <span className="text-red-500">*</span>
                </label>
                <div className="border border-gray-200 rounded-lg p-3">
                  <AddressSelector
                    value={{
                      provinceId: form.provinceId,
                      wardId: form.wardId,
                      street: form.street,
                    }}
                    onChange={handleAddressSelection}
                  />
                </div>
                {errors.address && <p className="mt-1 text-xs text-red-500">{errors.address}</p>}
              </div>

              {/* isActive toggle — edit mode only */}
              {editingStore && (
                <div className="flex items-center gap-3 pt-1">
                  <input
                    id="isActive"
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                    className="h-4 w-4 rounded border-gray-300 text-rose-600 focus:ring-rose-400"
                  />
                  <label htmlFor="isActive" className="text-sm font-medium text-gray-700 cursor-pointer">
                    {t('active')}
                  </label>
                </div>
              )}
            </div>

            {/* Modal footer */}
            <div className="flex items-center justify-end gap-3 px-6 py-4 border-t shrink-0">
              <button
                onClick={closeModal}
                disabled={submitting}
                className="px-4 py-2 text-sm font-medium text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors disabled:opacity-50"
              >
                {t('cancel', { ns: 'common' })}
              </button>
              <button
                onClick={handleSubmit}
                disabled={submitting}
                className="px-4 py-2 text-sm font-medium text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors disabled:opacity-60 flex items-center gap-2"
              >
                {submitting && (
                  <span className="inline-block h-3.5 w-3.5 rounded-full border-2 border-white border-t-transparent animate-spin" />
                )}
                {t('save')}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
