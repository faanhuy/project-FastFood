import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiEdit2, FiTrash2, FiPlus, FiX, FiToggleLeft, FiToggleRight } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { sizeService } from '../../services/sizeService';
import type { SizeCategory, SizeDto, CreateSizeRequest, UpdateSizeRequest } from '../../types/size';
import { SIZE_CATEGORY_LABELS } from '../../types/size';

const CATEGORIES: SizeCategory[] = ['DrinkSize', 'FoodPortion', 'MealSize', 'Custom'];

type TabKey = SizeCategory | 'all';

const INPUT_CLS = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-rose-400 focus:outline-none';

export default function SizeManagementPage() {
  const { t } = useTranslation(['admin', 'common', 'toast']);
  const [tab, setTab] = useState<TabKey>('all');
  const [sizes, setSizes] = useState<SizeDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Create modal
  const [showCreate, setShowCreate] = useState(false);
  const [createForm, setCreateForm] = useState<CreateSizeRequest>({
    category: 'DrinkSize',
    label: '',
    displayOrder: 0,
  });
  const [creating, setCreating] = useState(false);

  // Edit modal
  const [editingSize, setEditingSize] = useState<SizeDto | null>(null);
  const [editForm, setEditForm] = useState<UpdateSizeRequest>({
    label: '',
    displayOrder: 0,
  });
  const [editing, setEditing] = useState(false);

  const loadSizes = async () => {
    setLoading(true);
    try {
      const data = await sizeService.getAllAdmin();
      setSizes(data);
    } catch {
      toast.error(t('toast:sizeLoadFailed'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSizes();
  }, []);

  const filteredSizes = tab === 'all' ? sizes : sizes.filter((s) => s.category === tab);

  /* ── Create ── */
  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!createForm.label.trim()) {
      toast.error(t('toast:sizeNameRequired'));
      return;
    }
    setCreating(true);
    try {
      const created = await sizeService.createSize(createForm);
      setSizes((prev) => [...prev, created]);
      setShowCreate(false);
      setCreateForm({ category: 'DrinkSize', label: '', displayOrder: 0 });
      toast.success(t('toast:sizeCreated'));
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? t('toast:sizeCreateFailed'));
    } finally {
      setCreating(false);
    }
  };

  /* ── Edit ── */
  const openEdit = (size: SizeDto) => {
    setEditingSize(size);
    setEditForm({ label: size.label, displayOrder: size.displayOrder });
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingSize || !editForm.label.trim()) {
      toast.error(t('toast:sizeNameRequired'));
      return;
    }
    setEditing(true);
    try {
      const updated = await sizeService.updateMasterSize(editingSize.id, editForm);
      setSizes((prev) => prev.map((s) => s.id === updated.id ? updated : s));
      setEditingSize(null);
      toast.success(t('toast:sizeUpdated'));
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? t('toast:sizeUpdateFailed'));
    } finally {
      setEditing(false);
    }
  };

  /* ── Toggle Active ── */
  const handleToggleActive = async (size: SizeDto) => {
    if (size.isActive) {
      if (!confirm(t('admin:sizeDeactivateConfirm', { label: size.label }))) return;
    }
    try {
      const updated = await sizeService.toggleSizeActive(size.id);
      setSizes((prev) => prev.map((s) => s.id === updated.id ? updated : s));
      toast.success(t('toast:sizeToggled'));
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? t('toast:sizeToggleFailed'));
    }
  };

  /* ── Delete ── */
  const handleDelete = async (size: SizeDto) => {
    if (!confirm(t('admin:sizeDeleteConfirm', { label: size.label }))) return;
    try {
      await sizeService.deleteMasterSize(size.id);
      setSizes((prev) => prev.filter((s) => s.id !== size.id));
      toast.success(t('toast:sizeDeleted'));
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? t('toast:sizeDeleteFailed'));
    }
  };

  return (
    <AdminLayout title={t('admin:manageSizes')}>
      {/* Tabs + Add button */}
      <div className="flex items-end justify-between mb-4 border-b">
        <div className="flex gap-2 flex-wrap">
          <button
            onClick={() => setTab('all')}
            className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors ${
              tab === 'all' ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            {t('common:all')} ({sizes.length})
          </button>
          {CATEGORIES.map((cat) => {
            const count = sizes.filter((s) => s.category === cat).length;
            return (
              <button
                key={cat}
                onClick={() => setTab(cat)}
                className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors ${
                  tab === cat ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                {SIZE_CATEGORY_LABELS[cat]} ({count})
              </button>
            );
          })}
        </div>

        <button
          onClick={() => {
            setShowCreate(true);
            setCreateForm({ category: 'DrinkSize', label: '', displayOrder: 0 });
          }}
          className="mb-2 flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors shrink-0"
        >
          <FiPlus size={14} />
          {t('admin:addSize')}
        </button>
      </div>

      {/* Sizes table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">{t('admin:loadingSizes')}</div>
        ) : filteredSizes.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">
            {tab === 'all' ? t('admin:noSizes') : t('admin:noSizesInCategory', { category: SIZE_CATEGORY_LABELS[tab as SizeCategory] })}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('admin:sizeName')}</th>
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('admin:sizeCategory')}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('admin:displayOrder')}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('admin:sizeStatusCol')}</th>
                <th className="text-right px-5 py-3 font-semibold text-gray-600">{t('admin:sizeActions')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {filteredSizes.map((size) => (
                <tr key={size.id} className="hover:bg-gray-50">
                  <td className="px-5 py-3 font-medium text-gray-800">{size.label}</td>
                  <td className="px-5 py-3 text-sm text-gray-600">{SIZE_CATEGORY_LABELS[size.category]}</td>
                  <td className="px-5 py-3 text-center text-gray-600">{size.displayOrder}</td>
                  <td className="px-5 py-3 text-center">
                    {size.isActive ? (
                      <span className="inline-block bg-green-100 text-green-700 text-xs px-2 py-0.5 rounded-full font-medium">
                        {t('admin:sizeActive')}
                      </span>
                    ) : (
                      <span className="inline-block bg-gray-100 text-gray-700 text-xs px-2 py-0.5 rounded-full font-medium">
                        {t('admin:sizeInactive')}
                      </span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => openEdit(size)}
                        className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 hover:text-rose-600 transition-colors"
                        title={t('admin:edit')}
                      >
                        <FiEdit2 size={14} />
                      </button>
                      <button
                        onClick={() => handleToggleActive(size)}
                        className={`p-1.5 rounded-lg transition-colors ${
                          size.isActive
                            ? 'text-green-600 hover:bg-green-50 hover:text-green-700'
                            : 'text-gray-400 hover:bg-gray-100 hover:text-gray-600'
                        }`}
                        title={size.isActive ? t('admin:deactivate') : t('admin:activate')}
                      >
                        {size.isActive ? <FiToggleRight size={16} /> : <FiToggleLeft size={16} />}
                      </button>
                      <button
                        onClick={() => handleDelete(size)}
                        className="p-1.5 rounded-lg text-gray-500 hover:bg-red-50 hover:text-red-600 transition-colors"
                        title={t('admin:delete')}
                      >
                        <FiTrash2 size={14} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Create Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h3 className="text-base font-semibold text-gray-800">{t('admin:addNewSize')}</h3>
              <button
                onClick={() => setShowCreate(false)}
                disabled={creating}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors disabled:opacity-50"
              >
                <FiX size={16} />
              </button>
            </div>

            <form onSubmit={handleCreate} className="px-6 py-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('admin:sizeCategory')}</label>
                <select
                  className={INPUT_CLS}
                  value={createForm.category}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, category: e.target.value as SizeCategory }))
                  }
                >
                  {CATEGORIES.map((cat) => (
                    <option key={cat} value={cat}>
                      {SIZE_CATEGORY_LABELS[cat]}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('admin:sizeName')} <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  className={INPUT_CLS}
                  value={createForm.label}
                  onChange={(e) => setCreateForm((f) => ({ ...f, label: e.target.value }))}
                  placeholder={t('admin:sizeNamePlaceholder')}
                  autoFocus
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('admin:displayOrder')}</label>
                <input
                  type="number"
                  min={0}
                  className={INPUT_CLS}
                  value={createForm.displayOrder}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, displayOrder: Number(e.target.value) }))
                  }
                />
              </div>
            </form>

            <div className="flex items-center gap-3 px-6 py-4 bg-gray-50 border-t rounded-b-2xl">
              <button
                onClick={() => setShowCreate(false)}
                disabled={creating}
                className="flex-1 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
              >
                {t('common:cancel')}
              </button>
              <button
                onClick={handleCreate}
                disabled={creating}
                className="flex-1 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors disabled:opacity-50"
              >
                {creating ? t('common:saving') : t('common:save')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {editingSize && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h3 className="text-base font-semibold text-gray-800">{t('admin:editSize')}</h3>
              <button
                onClick={() => setEditingSize(null)}
                disabled={editing}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors disabled:opacity-50"
              >
                <FiX size={16} />
              </button>
            </div>

            <form onSubmit={handleUpdate} className="px-6 py-5 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('admin:sizeCategory')}</label>
                <div className="px-3 py-2 text-sm text-gray-600 bg-gray-50 rounded-lg border border-gray-300">
                  {SIZE_CATEGORY_LABELS[editingSize.category]}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('sizeNameLabel')} <span className="text-red-500">{t('required')}</span>
                </label>
                <input
                  type="text"
                  required
                  className={INPUT_CLS}
                  value={editForm.label}
                  onChange={(e) => setEditForm((f) => ({ ...f, label: e.target.value }))}
                  autoFocus
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('displayOrderLabel')}</label>
                <input
                  type="number"
                  min={0}
                  className={INPUT_CLS}
                  value={editForm.displayOrder}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, displayOrder: Number(e.target.value) }))
                  }
                />
              </div>
            </form>

            <div className="flex items-center gap-3 px-6 py-4 bg-gray-50 border-t rounded-b-2xl">
              <button
                onClick={() => setEditingSize(null)}
                disabled={editing}
                className="flex-1 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
              >
                {t('common:cancel')}
              </button>
              <button
                onClick={handleUpdate}
                disabled={editing}
                className="flex-1 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors disabled:opacity-50"
              >
                {editing ? t('common:saving') : t('common:save')}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
