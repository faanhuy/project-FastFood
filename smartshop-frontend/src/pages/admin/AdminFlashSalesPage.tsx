import { useEffect, useState, useRef } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiPlus, FiX, FiCheck, FiAlertCircle, FiEye, FiEdit2 } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { flashSaleService } from '../../services/flashSaleService';
import { productService } from '../../services/productService';
import { sizeService } from '../../services/sizeService';
import { formatDateTime } from '../../utils/formatters';
import { getApiError } from '../../utils/errorHandler';
import { getImageUrl } from '../../utils/imageUrl';
import type { FlashSaleDto, CreateFlashSaleRequest, CreateFlashSaleItemRequest } from '../../types/flashSale';
import type { ProductDto } from '../../types/product';
import type { ProductSize } from '../../types/size';

interface FlashSaleItemForm {
  productId: string;
  sizeId: string | null;
  salePrice: string;
  stockLimit: string;
}

interface FlashSaleFormState {
  name: string;
  startAt: string;
  endAt: string;
  items: FlashSaleItemForm[];
}

interface ItemSearchState {
  [key: number]: string;
}

const EMPTY_FORM: FlashSaleFormState = {
  name: '',
  startAt: '',
  endAt: '',
  items: [],
};

function toInputValue(iso: string) {
  if (!iso) return '';
  return iso.slice(0, 16);
}

function getTimeStatus(flashSale: FlashSaleDto): 'active' | 'upcoming' | 'expired' {
  const now = new Date();
  const start = new Date(flashSale.startAt);
  const end = new Date(flashSale.endAt);
  if (now < start) return 'upcoming';
  if (now > end) return 'expired';
  return 'active';
}

export default function AdminFlashSalesPage() {
  const { t } = useTranslation('admin');
  const { t: tToast } = useTranslation('toast');
  const [flashSales, setFlashSales] = useState<FlashSaleDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Create/Edit modal
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FlashSaleFormState>(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [itemSizes, setItemSizes] = useState<Record<number, ProductSize[]>>({});
  const [itemSearches, setItemSearches] = useState<ItemSearchState>({});
  const [activeDropdown, setActiveDropdown] = useState<number | null>(null);
  const dropdownRefs = useRef<Record<number, HTMLDivElement | null>>({});

  // Detail modal
  const [detailFlashSale, setDetailFlashSale] = useState<FlashSaleDto | null>(null);

  // Reject modal
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectingFlashSaleId, setRejectingFlashSaleId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  // Filters & pagination
  const [statusFilter, setStatusFilter] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const load = async (p = 1) => {
    setLoading(true);
    try {
      const result = await flashSaleService.adminGetAll(p, 20, statusFilter ?? undefined);
      setFlashSales(result.items);
      setTotalPages(result.totalPages);
    } catch {
      toast.error(tToast('loadFailed'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load(page);
    productService.getProducts({ page: 1, pageSize: 200 }).then((r) => setProducts(r.items)).catch(() => {});
  }, [page, statusFilter]);

  const loadSizesForItem = async (index: number, productId: string) => {
    if (!productId) return;
    try {
      const sizes = await sizeService.getProductSizes(productId);
      setItemSizes((prev) => ({ ...prev, [index]: sizes }));
    } catch {
      /* ignore */
    }
  };

  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setItemSizes({});
    setItemSearches({});
    setActiveDropdown(null);
    setFormError(null);
    setShowModal(true);
  };

  const openEdit = async (fs: FlashSaleDto) => {
    setEditingId(fs.id);
    setForm({
      name: fs.name,
      startAt: fs.startAt,
      endAt: fs.endAt,
      items: fs.items.map((it): FlashSaleItemForm => ({
        productId: it.productId,
        sizeId: it.sizeId || null,
        salePrice: String(it.salePrice),
        stockLimit: String(it.stockLimit),
      })),
    });
    const sizesMap: Record<number, ProductSize[]> = {};
    await Promise.all(
      fs.items.map(async (it, i) => {
        const sizes = await sizeService.getProductSizes(it.productId).catch(() => []);
        sizesMap[i] = sizes;
      }),
    );
    setItemSizes(sizesMap);
    setItemSearches({});
    setActiveDropdown(null);
    setFormError(null);
    setShowModal(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);

    if (!form.name.trim() || !form.startAt || !form.endAt || form.items.length === 0) {
      setFormError(t('comboRequiredFields'));
      return;
    }
    if (form.items.some((it) => !it.productId || !it.salePrice || !it.stockLimit)) {
      setFormError(t('comboRequiredFields'));
      return;
    }

    const payload: CreateFlashSaleRequest = {
      name: form.name.trim(),
      startAt: new Date(form.startAt).toISOString(),
      endAt: new Date(form.endAt).toISOString(),
      items: form.items.map((it): CreateFlashSaleItemRequest => ({
        productId: it.productId,
        sizeId: it.sizeId || undefined,
        salePrice: Number(it.salePrice),
        stockLimit: Number(it.stockLimit),
      })),
    };

    setSaving(true);
    try {
      if (editingId) {
        await flashSaleService.update(editingId, payload);
        toast.success(tToast('flashSaleUpdated'));
      } else {
        await flashSaleService.create(payload);
        toast.success(tToast('flashSaleCreated'));
      }
      setShowModal(false);
      await load();
    } catch (err) {
      setFormError(getApiError(err, tToast(editingId ? 'flashSaleUpdateFailed' : 'flashSaleCreateFailed')));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(t('flashSaleDeleteConfirm', { name }))) return;
    try {
      await flashSaleService.delete(id);
      toast.success(tToast('flashSaleDeleted'));
      await load();
    } catch {
      toast.error(tToast('flashSaleDeleteFailed'));
    }
  };

  const handleSubmitForApproval = async (id: string) => {
    if (!confirm(t('flashSaleConfirmSubmit'))) return;
    setActionLoading(id);
    try {
      await flashSaleService.submitForApproval(id);
      toast.success(tToast('flashSaleSubmitted'));
      await load();
    } catch (err) {
      toast.error(getApiError(err, tToast('flashSaleSubmitFailed')));
    } finally {
      setActionLoading(null);
    }
  };

  const handleApprove = async (id: string) => {
    if (!confirm(t('flashSaleConfirmApprove'))) return;
    setActionLoading(id);
    try {
      await flashSaleService.approve(id);
      toast.success(tToast('flashSaleApproved'));
      await load();
    } catch (err) {
      toast.error(getApiError(err, tToast('flashSaleApproveFailed')));
    } finally {
      setActionLoading(null);
    }
  };

  const handleRejectClick = (id: string) => {
    setRejectingFlashSaleId(id);
    setRejectReason('');
    setShowRejectModal(true);
  };

  const handleRejectConfirm = async () => {
    if (!rejectingFlashSaleId || !rejectReason.trim()) {
      toast.error(tToast('returnRejectReasonRequired'));
      return;
    }
    setActionLoading(rejectingFlashSaleId);
    try {
      await flashSaleService.reject(rejectingFlashSaleId, rejectReason.trim());
      toast.success(tToast('flashSaleRejected'));
      setShowRejectModal(false);
      setRejectingFlashSaleId(null);
      setRejectReason('');
      await load();
    } catch (err) {
      toast.error(getApiError(err, tToast('flashSaleRejectFailed')));
    } finally {
      setActionLoading(null);
    }
  };

  const addItem = () => {
    setForm((f) => ({ ...f, items: [...f.items, { productId: '', sizeId: null, salePrice: '', stockLimit: '' }] }));
  };

  const removeItem = (i: number) => {
    setForm((f) => ({ ...f, items: f.items.filter((_, idx) => idx !== i) }));
    setItemSizes((prev) => {
      const next: Record<number, ProductSize[]> = {};
      Object.entries(prev).forEach(([k, v]) => {
        const ki = Number(k);
        if (ki < i) next[ki] = v;
        else if (ki > i) next[ki - 1] = v;
      });
      return next;
    });
    setItemSearches((prev) => {
      const next: Record<number, string> = {};
      Object.entries(prev).forEach(([k, v]) => {
        const ki = Number(k);
        if (ki < i) next[ki] = v;
        else if (ki > i) next[ki - 1] = v;
      });
      return next;
    });
    if (activeDropdown === i) setActiveDropdown(null);
  };

  const updateItem = async (i: number, field: keyof FlashSaleItemForm, value: string | null) => {
    setForm((f) => {
      const items = f.items.map((it, idx) => (idx === i ? { ...it, [field]: value } : it));
      return { ...f, items };
    });
    if (field === 'productId' && value) {
      await loadSizesForItem(i, value);
      setForm((f) => {
        const items = f.items.map((it, idx) => (idx === i ? { ...it, sizeId: null } : it));
        return { ...f, items };
      });
    }
  };

  const resetProductItem = (i: number) => {
    setForm((f) => {
      const items = f.items.map((it, idx) => (idx === i ? { ...it, productId: '', sizeId: null } : it));
      return { ...f, items };
    });
    setItemSearches((prev) => ({ ...prev, [i]: '' }));
    setActiveDropdown(null);
  };

  const fmt = (n: number) => n.toLocaleString('vi-VN') + t('common:currency', { ns: 'common' });

  const getFilteredProducts = (query: string): ProductDto[] => {
    if (!query.trim()) return products.slice(0, 8);
    const lowerQuery = query.toLowerCase();
    return products.filter((p) => p.name.toLowerCase().includes(lowerQuery)).slice(0, 8);
  };

  const getStatusBadge = (status: string) => {
    const map: Record<string, { label: string; className: string; bgColor: string }> = {
      Draft: { label: t('flashSaleStatusDraft'), className: 'text-gray-700', bgColor: 'bg-gray-100' },
      PendingApproval: { label: t('flashSaleStatusPending'), className: 'text-yellow-700', bgColor: 'bg-yellow-100' },
      Approved: { label: t('flashSaleStatusApproved'), className: 'text-green-700', bgColor: 'bg-green-100' },
      Rejected: { label: t('flashSaleStatusRejected'), className: 'text-red-700', bgColor: 'bg-red-100' },
    };
    return map[status] ?? { label: status, className: 'text-gray-700', bgColor: 'bg-gray-100' };
  };

  const getTimeStatusBadge = (timeStatus: 'active' | 'upcoming' | 'expired') => {
    if (timeStatus === 'active') return { label: t('flashSaleStatusActive'), dot: 'bg-green-400' };
    if (timeStatus === 'upcoming') return { label: t('flashSaleStatusUpcoming'), dot: 'bg-blue-400' };
    return { label: t('flashSaleStatusExpired'), dot: 'bg-gray-300' };
  };

  return (
    <AdminLayout title={t('manageFlashSales')}>
      <div className="space-y-4">
        {/* Toolbar */}
        <div className="flex items-center justify-between gap-3 flex-wrap">
          <p className="text-sm text-gray-500">{t('flashSaleItemCount', { count: flashSales.length })}</p>
          <div className="flex items-center gap-2 ml-auto">
            <select
              value={statusFilter || ''}
              onChange={(e) => { setStatusFilter(e.target.value || null); setPage(1); }}
              className="text-sm border rounded-lg px-3 py-2 text-gray-600 hover:border-gray-400"
            >
              <option value="">{t('flashSaleFilterStatus')}</option>
              <option value="Draft">{t('flashSaleStatusDraft')}</option>
              <option value="PendingApproval">{t('flashSaleStatusPending')}</option>
              <option value="Approved">{t('flashSaleStatusApproved')}</option>
              <option value="Rejected">{t('flashSaleStatusRejected')}</option>
            </select>
            <button
              onClick={openCreate}
              className="flex items-center gap-2 bg-rose-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-rose-700"
            >
              <FiPlus size={15} /> {t('createFlashSale')}
            </button>
          </div>
        </div>

        {/* Create/Edit Modal */}
        {showModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="bg-white rounded-2xl w-full max-w-2xl max-h-[90vh] flex flex-col shadow-xl">
              <div className="flex items-center justify-between px-5 py-4 border-b">
                <h2 className="font-semibold text-gray-800">
                  {editingId ? t('edit') : t('createFlashSale')}
                </h2>
                <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600">
                  <FiX size={18} />
                </button>
              </div>

              <form onSubmit={handleSave} className="flex-1 overflow-y-auto px-5 py-4 space-y-4">
                <div>
                  <label className="text-xs font-medium text-gray-600">{t('flashSaleNameLabel')} *</label>
                  <input
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.name}
                    onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                    required
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="text-xs font-medium text-gray-600">{t('flashSaleStartLabel')} *</label>
                    <input
                      type="datetime-local"
                      className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                      value={toInputValue(form.startAt)}
                      onChange={(e) => setForm((f) => ({ ...f, startAt: e.target.value }))}
                      required
                    />
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-600">{t('flashSaleEndLabel')} *</label>
                    <input
                      type="datetime-local"
                      className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                      value={toInputValue(form.endAt)}
                      onChange={(e) => setForm((f) => ({ ...f, endAt: e.target.value }))}
                      required
                    />
                  </div>
                </div>

                {/* Items */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="text-xs font-medium text-gray-600">{t('flashSaleItemsLabel')}</label>
                    <button
                      type="button"
                      onClick={addItem}
                      className="text-xs text-rose-600 hover:underline flex items-center gap-1"
                    >
                      <FiPlus size={12} /> {t('flashSaleAddItem')}
                    </button>
                  </div>

                  {form.items.length === 0 && (
                    <p className="text-xs text-gray-400 italic">{t('flashSaleItemsEmpty')}</p>
                  )}

                  <div className="space-y-3">
                    {form.items.map((item, i) => {
                      const selectedProduct = products.find((p) => p.id === item.productId);
                      const filteredProducts = getFilteredProducts(itemSearches[i] ?? '');
                      const isDropdownOpen = activeDropdown === i;
                      const isProductSelected = item.productId !== '';

                      return (
                        <div key={i} className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                          {!isProductSelected ? (
                            <div className="flex items-start gap-3">
                              <div className="flex-1">
                                <div className="relative">
                                  <input
                                    type="text"
                                    className="w-full border rounded px-2.5 py-1.5 text-xs bg-white"
                                    placeholder={t('admin:searchProductPlaceholder')}
                                    value={itemSearches[i] ?? ''}
                                    onChange={(e) => {
                                      setItemSearches((prev) => ({ ...prev, [i]: e.target.value }));
                                      setActiveDropdown(i);
                                    }}
                                    onFocus={() => setActiveDropdown(i)}
                                  />
                                  {isDropdownOpen && (
                                    <div
                                      ref={(el) => { if (el) dropdownRefs.current[i] = el; }}
                                      className="absolute top-full left-0 right-0 z-10 mt-1 bg-white border rounded shadow-lg max-h-40 overflow-y-auto"
                                    >
                                      {filteredProducts.length === 0 ? (
                                        <div className="px-3 py-2 text-xs text-gray-400">{t('admin:noProductFound')}</div>
                                      ) : (
                                        filteredProducts.map((p) => (
                                          <button
                                            key={p.id}
                                            type="button"
                                            onClick={() => {
                                              updateItem(i, 'productId', p.id);
                                              setItemSearches((prev) => ({ ...prev, [i]: '' }));
                                              setActiveDropdown(null);
                                            }}
                                            className="w-full text-left px-3 py-2 text-xs hover:bg-gray-100 border-b last:border-b-0 flex items-center gap-2"
                                          >
                                            <img
                                              src={getImageUrl(p.imageUrl)}
                                              alt={p.name}
                                              className="h-6 w-6 rounded object-cover border border-gray-200"
                                              onError={(e) => {
                                                (e.target as HTMLImageElement).src =
                                                  'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%226%22 height=%226%22 viewBox=%220 0 24 24%22 fill=%22none%22 stroke=%22%23D1D5DB%22 stroke-width=%222%22%3E%3Crect x=%222%22 y=%222%22 width=%2220%22 height=%2220%22 rx=%222%22/%3E%3C/svg%3E';
                                              }}
                                            />
                                            <div className="flex-1 min-w-0">
                                              <p className="truncate font-medium">{p.name}</p>
                                              <p className="text-gray-500">{fmt(p.price)}</p>
                                            </div>
                                          </button>
                                        ))
                                      )}
                                    </div>
                                  )}
                                </div>
                              </div>
                              <button type="button" onClick={() => removeItem(i)} className="text-gray-400 hover:text-red-500 shrink-0 mt-0.5">
                                <FiX size={14} />
                              </button>
                            </div>
                          ) : (
                            <>
                              <div className="flex items-center gap-3 mb-3">
                                {selectedProduct && (
                                  <img
                                    src={getImageUrl(selectedProduct.imageUrl)}
                                    alt={selectedProduct.name}
                                    className="h-7 w-7 rounded object-cover border border-gray-300 shrink-0"
                                    onError={(e) => {
                                      (e.target as HTMLImageElement).src =
                                        'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%227%22 height=%227%22 viewBox=%220 0 24 24%22 fill=%22none%22 stroke=%22%23D1D5DB%22 stroke-width=%222%22%3E%3Crect x=%222%22 y=%222%22 width=%2220%22 height=%2220%22 rx=%222%22/%3E%3C/svg%3E';
                                    }}
                                  />
                                )}
                                <div className="flex-1 min-w-0">
                                  <p className="text-sm font-medium text-gray-800 truncate">{selectedProduct?.name}</p>
                                  <p className="text-xs text-gray-500">{fmt(selectedProduct?.price || 0)}</p>
                                </div>
                                <button type="button" onClick={() => resetProductItem(i)} className="text-gray-400 hover:text-red-500 shrink-0">
                                  <FiX size={16} />
                                </button>
                              </div>

                              <div className="flex flex-col gap-2 mb-2">
                                {selectedProduct?.hasSizes && (
                                  <select
                                    className="flex-1 border rounded px-2.5 py-1.5 text-xs bg-white"
                                    value={item.sizeId ?? ''}
                                    onChange={(e) => updateItem(i, 'sizeId', e.target.value || null)}
                                  >
                                    <option value="">{t('flashSaleSelectSize')}</option>
                                    {(itemSizes[i] ?? []).map((s) => (
                                      <option key={s.id} value={s.id}>{s.sizeLabel}</option>
                                    ))}
                                  </select>
                                )}
                                <div className="grid grid-cols-2 gap-2">
                                  <div>
                                    <label className="text-[10px] text-gray-600">{t('flashSaleSalePrice')} *</label>
                                    <input
                                      type="number" min={0} step={0.01}
                                      className="w-full border rounded px-2.5 py-1.5 text-xs bg-white"
                                      value={item.salePrice}
                                      onChange={(e) => updateItem(i, 'salePrice', e.target.value)}
                                      required
                                    />
                                  </div>
                                  <div>
                                    <label className="text-[10px] text-gray-600">{t('flashSaleStockLimit')} *</label>
                                    <input
                                      type="number" min={1}
                                      className="w-full border rounded px-2.5 py-1.5 text-xs bg-white"
                                      value={item.stockLimit}
                                      onChange={(e) => updateItem(i, 'stockLimit', e.target.value)}
                                      required
                                    />
                                  </div>
                                </div>
                              </div>

                              <button
                                type="button"
                                onClick={() => removeItem(i)}
                                className="w-full text-xs text-gray-500 hover:text-red-600 py-1 border border-gray-200 rounded hover:bg-red-50"
                              >
                                {t('flashSaleRemoveItem')}
                              </button>
                            </>
                          )}
                        </div>
                      );
                    })}
                  </div>
                </div>

                {formError && <p className="text-sm text-red-500">{formError}</p>}
                <p className="text-xs text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
                  {t('flashSalePriceNote')}
                </p>
              </form>

              <div className="px-5 py-3 border-t flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 text-sm rounded-lg border text-gray-600 hover:bg-gray-50"
                >
                  {t('common:cancel', { ns: 'common' })}
                </button>
                <button
                  onClick={handleSave}
                  disabled={saving}
                  className="px-4 py-2 text-sm rounded-lg bg-rose-600 text-white hover:bg-rose-700 disabled:opacity-50"
                >
                  {saving ? t('common:saving', { ns: 'common' }) : editingId ? t('common:update', { ns: 'common' }) : t('createFlashSale')}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Detail Modal */}
        {detailFlashSale && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="bg-white rounded-2xl w-full max-w-3xl max-h-[90vh] flex flex-col shadow-xl">
              <div className="flex items-start justify-between px-5 py-4 border-b gap-4">
                <div>
                  <h2 className="font-semibold text-gray-800 text-lg">{detailFlashSale.name}</h2>
                  <div className="flex items-center gap-3 mt-1 text-xs text-gray-500 flex-wrap">
                    <span>{formatDateTime(detailFlashSale.startAt)}</span>
                    <span className="text-gray-300">→</span>
                    <span>{formatDateTime(detailFlashSale.endAt)}</span>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  {(() => {
                    const s = getStatusBadge(detailFlashSale.status);
                    return (
                      <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${s.bgColor} ${s.className}`}>
                        {s.label}
                      </span>
                    );
                  })()}
                  <button onClick={() => setDetailFlashSale(null)} className="text-gray-400 hover:text-gray-600 ml-1">
                    <FiX size={18} />
                  </button>
                </div>
              </div>

              <div className="flex-1 overflow-y-auto px-5 py-4">
                {detailFlashSale.approvedBy && (
                  <p className="text-xs text-green-700 bg-green-50 border border-green-200 rounded-lg px-3 py-2 mb-4">
                    {t('flashSaleApprovedBy')}: <span className="font-medium">{detailFlashSale.approvedBy}</span>
                  </p>
                )}
                {detailFlashSale.rejectedReason && (
                  <div className="flex items-start gap-2 bg-red-50 border border-red-200 rounded-lg px-3 py-2 mb-4">
                    <FiAlertCircle size={14} className="text-red-600 shrink-0 mt-0.5" />
                    <div className="text-xs text-red-700">
                      <p className="font-medium">{t('flashSaleRejectedReason')}:</p>
                      <p>{detailFlashSale.rejectedReason}</p>
                    </div>
                  </div>
                )}

                <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
                  {t('flashSaleItemsLabel')} ({detailFlashSale.items.length})
                </p>

                <div className="overflow-x-auto">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="border-b bg-gray-50 text-xs text-gray-500">
                        <th className="text-left py-2 px-3 font-medium">{t('flashSaleSelectProduct')}</th>
                        <th className="text-right py-2 px-3 font-medium">{t('flashSaleOriginalPrice')}</th>
                        <th className="text-right py-2 px-3 font-medium">{t('flashSaleSalePrice')}</th>
                        <th className="text-right py-2 px-3 font-medium">{t('flashSaleDiscount')}</th>
                        <th className="text-right py-2 px-3 font-medium">{t('flashSaleSold')}</th>
                        <th className="text-right py-2 px-3 font-medium">{t('flashSaleRemaining')}</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                      {detailFlashSale.items.map((item, idx) => (
                        <tr key={idx} className="hover:bg-gray-50">
                          <td className="py-2.5 px-3">
                            <div className="flex items-center gap-2">
                              {item.imageUrl && (
                                <img
                                  src={getImageUrl(item.imageUrl)}
                                  alt={item.productName}
                                  className="h-8 w-8 rounded object-cover border border-gray-200 shrink-0"
                                  onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                                />
                              )}
                              <div className="min-w-0">
                                <p className="font-medium text-gray-800 truncate max-w-[180px]">{item.productName}</p>
                                {item.sizeLabel && (
                                  <p className="text-xs text-gray-500">{item.sizeLabel}</p>
                                )}
                              </div>
                            </div>
                          </td>
                          <td className="py-2.5 px-3 text-right text-gray-500">{fmt(item.originalPrice)}</td>
                          <td className="py-2.5 px-3 text-right font-semibold text-rose-600">{fmt(item.salePrice)}</td>
                          <td className="py-2.5 px-3 text-right">
                            <span className="bg-rose-100 text-rose-700 text-xs font-semibold px-1.5 py-0.5 rounded">
                              -{item.percentDiscount}%
                            </span>
                          </td>
                          <td className="py-2.5 px-3 text-right text-gray-600">{item.soldCount}</td>
                          <td className="py-2.5 px-3 text-right">
                            <span className={item.remainingStock <= 0 ? 'text-red-500 font-medium' : 'text-gray-700'}>
                              {item.remainingStock}/{item.stockLimit}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              <div className="px-5 py-3 border-t flex justify-end">
                <button
                  onClick={() => setDetailFlashSale(null)}
                  className="px-4 py-2 text-sm rounded-lg border text-gray-600 hover:bg-gray-50"
                >
                  {t('common:close', { ns: 'common' })}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Reject Modal */}
        {showRejectModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="bg-white rounded-2xl w-full max-w-md shadow-xl">
              <div className="px-5 py-4 border-b">
                <h2 className="font-semibold text-gray-800">{t('flashSaleReject')}</h2>
              </div>
              <div className="px-5 py-4 space-y-3">
                <p className="text-sm text-gray-600">{t('flashSaleConfirmReject')}</p>
                <textarea
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder={t('rejectReasonPlaceholder', { ns: 'admin' })}
                  className="w-full border rounded-lg px-3 py-2 text-sm resize-none h-24"
                />
              </div>
              <div className="px-5 py-3 border-t flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => { setShowRejectModal(false); setRejectingFlashSaleId(null); setRejectReason(''); }}
                  className="px-4 py-2 text-sm rounded-lg border text-gray-600 hover:bg-gray-50"
                >
                  {t('cancel', { ns: 'common' })}
                </button>
                <button
                  onClick={handleRejectConfirm}
                  disabled={actionLoading === rejectingFlashSaleId}
                  className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
                >
                  {actionLoading === rejectingFlashSaleId ? t('common:processing', { ns: 'common' }) : t('flashSaleReject')}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Flash Sale List */}
        {loading ? (
          <p className="text-center py-12 text-gray-400">{t('common:loading', { ns: 'common' })}</p>
        ) : flashSales.length === 0 ? (
          <p className="text-center py-12 text-gray-400">{t('common:noData', { ns: 'common' })}</p>
        ) : (
          <div className="space-y-2">
            {flashSales.map((fs) => {
              const timeStatus = getTimeStatus(fs);
              const statusBadge = getStatusBadge(fs.status);
              const timeBadge = getTimeStatusBadge(timeStatus);
              const isDraft = fs.status === 'Draft';
              const isPendingApproval = fs.status === 'PendingApproval';
              const isRejected = fs.status === 'Rejected';
              const canEdit = isDraft || isRejected;
              const canDelete = isDraft || isRejected;
              const canSubmit = isDraft || isRejected;

              return (
                <div key={fs.id} className="bg-white rounded-xl border px-4 py-3">
                  {/* Main row */}
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <p className="font-semibold text-gray-800 truncate">{fs.name}</p>
                      <div className="flex items-center gap-3 mt-1 text-xs text-gray-500 flex-wrap">
                        <span>{t('flashSaleItemCount', { count: fs.items.length })}</span>
                        <span className="text-gray-300">|</span>
                        <span>{formatDateTime(fs.startAt)}</span>
                        <span className="text-gray-300">→</span>
                        <span>{formatDateTime(fs.endAt)}</span>
                      </div>
                    </div>

                    {/* Badges */}
                    <div className="flex items-center gap-2 shrink-0">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${statusBadge.bgColor} ${statusBadge.className}`}>
                        {statusBadge.label}
                      </span>
                      <div className="flex items-center gap-1 text-xs text-gray-500">
                        <span className={`inline-block h-1.5 w-1.5 rounded-full ${timeBadge.dot}`} />
                        {timeBadge.label}
                      </div>
                    </div>
                  </div>

                  {/* Rejected reason inline */}
                  {isRejected && fs.rejectedReason && (
                    <div className="mt-2 flex items-start gap-1.5 text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-2.5 py-1.5">
                      <FiAlertCircle size={12} className="shrink-0 mt-0.5" />
                      <span className="line-clamp-1">{fs.rejectedReason}</span>
                    </div>
                  )}

                  {/* Approved by inline */}
                  {fs.approvedBy && (
                    <p className="mt-1.5 text-xs text-green-600">
                      {t('flashSaleApprovedBy')}: <span className="font-medium">{fs.approvedBy}</span>
                    </p>
                  )}

                  {/* Actions */}
                  <div className="mt-2.5 flex items-center gap-2 flex-wrap">
                    {/* View Detail — always visible */}
                    <button
                      onClick={() => setDetailFlashSale(fs)}
                      className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs rounded-lg border border-gray-200 text-gray-600 hover:border-gray-300 hover:bg-gray-50 font-medium"
                    >
                      <FiEye size={12} /> {t('flashSaleDetail')}
                    </button>

                    {/* Edit — Draft & Rejected */}
                    {canEdit && (
                      <button
                        onClick={() => openEdit(fs)}
                        disabled={actionLoading !== null}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs rounded-lg border border-blue-200 text-blue-600 hover:bg-blue-50 font-medium disabled:opacity-50"
                      >
                        <FiEdit2 size={12} /> {t('edit', { ns: 'admin' })}
                      </button>
                    )}

                    {/* Delete — Draft & Rejected */}
                    {canDelete && (
                      <button
                        onClick={() => handleDelete(fs.id, fs.name)}
                        disabled={actionLoading !== null}
                        className="px-3 py-1.5 text-xs rounded-lg border border-red-200 text-red-600 hover:bg-red-50 font-medium disabled:opacity-50"
                      >
                        {t('delete', { ns: 'admin' })}
                      </button>
                    )}

                    {/* Separator */}
                    {(canSubmit || isPendingApproval) && (
                      <div className="flex-1" />
                    )}

                    {/* Submit for Approval — Draft & Rejected */}
                    {canSubmit && (
                      <button
                        onClick={() => handleSubmitForApproval(fs.id)}
                        disabled={actionLoading !== null}
                        className="px-3 py-1.5 text-xs rounded-lg bg-blue-600 text-white hover:bg-blue-700 font-medium disabled:opacity-50"
                      >
                        {actionLoading === fs.id ? t('common:processing', { ns: 'common' }) : t('flashSaleSubmitForApproval')}
                      </button>
                    )}

                    {/* Approve & Reject — PendingApproval */}
                    {isPendingApproval && (
                      <>
                        <button
                          onClick={() => handleRejectClick(fs.id)}
                          disabled={actionLoading !== null}
                          className="px-3 py-1.5 text-xs rounded-lg border border-red-300 text-red-600 hover:bg-red-50 font-medium disabled:opacity-50"
                        >
                          {t('flashSaleReject')}
                        </button>
                        <button
                          onClick={() => handleApprove(fs.id)}
                          disabled={actionLoading !== null}
                          className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs rounded-lg bg-green-600 text-white hover:bg-green-700 font-medium disabled:opacity-50"
                        >
                          {actionLoading === fs.id ? t('common:processing', { ns: 'common' }) : (
                            <><FiCheck size={12} /> {t('flashSaleApprove')}</>
                          )}
                        </button>
                      </>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}

        {/* Pagination */}
        {!loading && totalPages > 1 && (
          <div className="mt-4 flex justify-end">
            <div className="flex items-center gap-2">
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                    page === p
                      ? 'bg-rose-600 text-white'
                      : 'border border-gray-200 text-gray-600 hover:border-rose-400'
                  }`}
                >
                  {p}
                </button>
              ))}
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
