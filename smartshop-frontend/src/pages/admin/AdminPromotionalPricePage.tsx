import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { FiEdit2, FiTrash2, FiPlus, FiX, FiCheck, FiLayers, FiAlertTriangle } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { promotionService } from '../../services/promotionService';
import { productService } from '../../services/productService';
import { sizeService } from '../../services/sizeService';
import { storeService } from '../../services/storeService';
import type { PriceCampaignSummaryDto, PriceCampaignDto, CreatePriceCampaignRequest } from '../../types/promotion';
import type { ProductDto } from '../../types/product';
import type { ProductSize } from '../../types/size';
import type { AdminStore } from '../../types/store';

const RULE_TYPES = [
  { value: 1, label: 'Giảm theo % (Discount %)' },
  { value: 2, label: 'Giá cố định (Fixed Price)' },
];

interface SizeDiscountEntry {
  sizeId: string;
  sizeLabel: string;
  ruleType: number;
  discountValue: string;
}

interface CampaignItemForm {
  productId: string;
  hasSizes: boolean;
  // used when hasSizes=false:
  sizeId: string | null;
  ruleType: number;
  discountValue: string;
  // used when hasSizes=true:
  sizeEntries: SizeDiscountEntry[];
}

const defaultForm = () => ({
  name: '',
  startsAt: '',
  endsAt: '',
  appliesToAll: true,
  storeIds: [] as string[],
  items: [] as CampaignItemForm[],
});

export default function AdminPromotionalPricePage() {
  const [campaigns, setCampaigns] = useState<PriceCampaignSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(defaultForm());

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [stores, setStores] = useState<AdminStore[]>([]);
  const [itemSizes, setItemSizes] = useState<Record<number, ProductSize[]>>({});
  const [sizePopupIdx, setSizePopupIdx] = useState<number | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const loadCampaigns = async () => {
    setLoading(true);
    try {
      const result = await promotionService.getPriceCampaigns();
      setCampaigns(result.items);
    } catch {
      toast.error('Không tải được danh sách campaign.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCampaigns();
    productService.getProducts({ pageSize: 200 }).then((r) => setProducts(r?.items ?? [])).catch(() => {});
    storeService.getAdminStores().then(setStores).catch(() => {});
  }, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(defaultForm());
    setItemSizes({});
    setShowForm(true);
  };

  const openEdit = async (id: string) => {
    try {
      const dto: PriceCampaignDto = await promotionService.getPriceCampaignById(id);
      setEditingId(id);

      // Group raw API items by productId
      type RawEntry = { sizeId: string | null; ruleType: number; discountValue: number };
      const productMap = new Map<string, RawEntry[]>();
      for (const it of (dto.items ?? [])) {
        const list = productMap.get(it.productId) ?? [];
        list.push({ sizeId: it.sizeId, ruleType: it.ruleType, discountValue: it.discountValue });
        productMap.set(it.productId, list);
      }

      const sizesMap: Record<number, ProductSize[]> = {};
      const items: CampaignItemForm[] = [];
      let itemIdx = 0;

      for (const [productId, rawEntries] of productMap.entries()) {
        const product = products.find((p) => p.id === productId);
        const hasSizes = product?.hasSizes ?? rawEntries.some((e) => e.sizeId !== null);

        if (!hasSizes) {
          const e = rawEntries[0];
          items.push({
            productId,
            hasSizes: false,
            sizeId: e.sizeId,
            ruleType: e.ruleType,
            discountValue:
              e.ruleType === 1
                ? String(Math.round((1 - e.discountValue) * 10000) / 100)
                : String(e.discountValue),
            sizeEntries: [],
          });
        } else {
          let activeSizes: ProductSize[] = [];
          try {
            const allSizes = await sizeService.getProductSizes(productId);
            activeSizes = allSizes.filter((s) => s.isActive);
            sizesMap[itemIdx] = activeSizes;
          } catch {
            sizesMap[itemIdx] = [];
          }
          const existingBySizeId = new Map(
            rawEntries.filter((e) => e.sizeId !== null).map((e) => [e.sizeId!, e]),
          );
          const sizeEntries: SizeDiscountEntry[] = activeSizes.map((s) => {
            const existing = existingBySizeId.get(s.id);
            return {
              sizeId: s.id,
              sizeLabel: s.sizeLabel,
              ruleType: existing?.ruleType ?? 1,
              discountValue: existing
                ? existing.ruleType === 1
                  ? String(Math.round((1 - existing.discountValue) * 10000) / 100)
                  : String(existing.discountValue)
                : '',
            };
          });
          items.push({
            productId,
            hasSizes: true,
            sizeId: null,
            ruleType: 1,
            discountValue: '',
            sizeEntries,
          });
        }
        itemIdx++;
      }

      setForm({
        name: dto.name,
        startsAt: dto.startsAt.slice(0, 16),
        endsAt: dto.endsAt.slice(0, 16),
        appliesToAll: dto.appliesToAll,
        storeIds: dto.stores?.map((s) => s.id) ?? [],
        items,
      });
      setItemSizes(sizesMap);
      setShowForm(true);
    } catch {
      toast.error('Không tải được campaign.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Xóa campaign này?')) return;
    setDeletingId(id);
    try {
      await promotionService.deletePriceCampaign(id);
      setCampaigns((prev) => prev.filter((c) => c.id !== id));
      toast.success('Đã xóa campaign.');
    } catch {
      toast.error('Xóa thất bại.');
    } finally {
      setDeletingId(null);
    }
  };

  const addItem = async () => {
    const defaultProduct = products[0];
    const hasSizes = defaultProduct?.hasSizes ?? false;
    const newIdx = form.items.length;
    let sizeEntries: SizeDiscountEntry[] = [];
    if (hasSizes && defaultProduct) {
      try {
        const sizes = await sizeService.getProductSizes(defaultProduct.id);
        const activeSizes = sizes.filter((s) => s.isActive);
        sizeEntries = activeSizes.map((s) => ({
          sizeId: s.id,
          sizeLabel: s.sizeLabel,
          ruleType: 1,
          discountValue: '',
        }));
        setItemSizes((prev) => ({ ...prev, [newIdx]: activeSizes }));
      } catch {
        setItemSizes((prev) => ({ ...prev, [newIdx]: [] }));
      }
    }
    setForm((prev) => ({
      ...prev,
      items: [
        ...prev.items,
        {
          productId: defaultProduct?.id ?? '',
          hasSizes,
          sizeId: null,
          ruleType: 1,
          discountValue: '',
          sizeEntries,
        },
      ],
    }));
  };

  const removeItem = (idx: number) => {
    setForm((prev) => ({ ...prev, items: prev.items.filter((_, i) => i !== idx) }));
    setItemSizes((prev) => {
      const next = { ...prev };
      delete next[idx];
      return next;
    });
  };

  const updateItem = (idx: number, patch: Partial<CampaignItemForm>) => {
    setForm((prev) => ({
      ...prev,
      items: prev.items.map((it, i) => (i === idx ? { ...it, ...patch } : it)),
    }));
  };

  const updateSizeEntry = (itemIdx: number, sizeIdx: number, patch: Partial<SizeDiscountEntry>) => {
    setForm((prev) => ({
      ...prev,
      items: prev.items.map((it, i) =>
        i === itemIdx
          ? {
              ...it,
              sizeEntries: it.sizeEntries.map((se, j) => (j === sizeIdx ? { ...se, ...patch } : se)),
            }
          : it,
      ),
    }));
  };

  const handleProductChange = async (idx: number, productId: string) => {
    const p = products.find((pr) => pr.id === productId);
    const hasSizes = p?.hasSizes ?? false;
    let sizeEntries: SizeDiscountEntry[] = [];
    if (hasSizes && productId) {
      try {
        const sizes = await sizeService.getProductSizes(productId);
        const activeSizes = sizes.filter((s) => s.isActive);
        sizeEntries = activeSizes.map((s) => ({
          sizeId: s.id,
          sizeLabel: s.sizeLabel,
          ruleType: 1,
          discountValue: '',
        }));
        setItemSizes((prev) => ({ ...prev, [idx]: activeSizes }));
      } catch {
        setItemSizes((prev) => ({ ...prev, [idx]: [] }));
      }
    } else {
      setItemSizes((prev) => ({ ...prev, [idx]: [] }));
    }
    setForm((prev) => ({
      ...prev,
      items: prev.items.map((it, i) =>
        i === idx
          ? { ...it, productId, hasSizes, sizeId: null, ruleType: 1, discountValue: '', sizeEntries }
          : it,
      ),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim() || !form.startsAt || !form.endsAt) {
      toast.error('Vui lòng điền đầy đủ tên, ngày bắt đầu và ngày kết thúc.');
      return;
    }
    const flatItems: CreatePriceCampaignRequest['items'] = [];
    for (const it of form.items) {
      if (!it.hasSizes) {
        const raw = parseFloat(it.discountValue) || 0;
        flatItems.push({
          productId: it.productId,
          sizeId: it.sizeId || null,
          ruleType: it.ruleType,
          discountValue: it.ruleType === 1 ? (100 - raw) / 100 : raw,
        });
      } else {
        for (const se of it.sizeEntries) {
          if (se.discountValue !== '' && parseFloat(se.discountValue) > 0) {
            const raw = parseFloat(se.discountValue) || 0;
            flatItems.push({
              productId: it.productId,
              sizeId: se.sizeId,
              ruleType: se.ruleType,
              discountValue: se.ruleType === 1 ? (100 - raw) / 100 : raw,
            });
          }
        }
      }
    }
    const body: CreatePriceCampaignRequest = {
      id: editingId ?? '',
      name: form.name.trim(),
      startsAt: new Date(form.startsAt).toISOString(),
      endsAt: new Date(form.endsAt).toISOString(),
      appliesToAll: form.appliesToAll,
      storeIds: form.appliesToAll ? [] : form.storeIds,
      items: flatItems,
    };
    setSaving(true);
    try {
      if (editingId) {
        await promotionService.updatePriceCampaign(editingId, body);
        toast.success('Đã cập nhật campaign.');
      } else {
        await promotionService.createPriceCampaign(body);
        toast.success('Đã tạo campaign mới.');
      }
      setShowForm(false);
      await loadCampaigns();
    } catch (e: any) {
      toast.error(e.response?.data?.message ?? 'Lưu thất bại.');
    } finally {
      setSaving(false);
    }
  };

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });

  return (
    <AdminLayout title="Quản lý giá khuyến mãi">
      <div className="flex items-center justify-between mb-6">
        <p className="text-sm text-gray-500">Quản lý các chiến dịch giá (Price Campaign) theo sản phẩm và size.</p>
        <button
          onClick={openCreate}
          className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors"
        >
          <FiPlus size={14} />
          Tạo campaign
        </button>
      </div>

      {/* Campaign list */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">Đang tải...</div>
        ) : campaigns.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">Chưa có campaign nào.</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="text-left px-5 py-3 font-semibold text-gray-600">Tên</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">Thời gian</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">Phạm vi</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">Số item</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">Trạng thái</th>
                <th className="text-right px-5 py-3 font-semibold text-gray-600">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {campaigns.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-5 py-3 font-medium text-gray-800">{c.name}</td>
                  <td className="px-5 py-3 text-center text-gray-600 text-xs">
                    {formatDate(c.startsAt)} — {formatDate(c.endsAt)}
                  </td>
                  <td className="px-5 py-3 text-center">
                    {c.appliesToAll ? (
                      <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">Tất cả</span>
                    ) : (
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                        {c.storeCount} chi nhánh
                      </span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-center text-gray-600">{c.itemCount}</td>
                  <td className="px-5 py-3 text-center">
                    {c.isActive ? (
                      <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full font-medium">
                        Hoạt động
                      </span>
                    ) : (
                      <span className="text-xs bg-gray-100 text-gray-500 px-2 py-0.5 rounded-full">
                        Không hoạt động
                      </span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-right">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => openEdit(c.id)}
                        className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 hover:text-rose-600 transition-colors"
                        title="Sửa"
                      >
                        <FiEdit2 size={14} />
                      </button>
                      <button
                        onClick={() => handleDelete(c.id)}
                        disabled={deletingId === c.id}
                        className="p-1.5 rounded-lg text-gray-500 hover:bg-red-50 hover:text-red-600 transition-colors disabled:opacity-50"
                        title="Xóa"
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

      {/* Form modal */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-start justify-center p-4 bg-black/50 overflow-y-auto">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl my-8">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h3 className="text-base font-semibold text-gray-800">
                {editingId ? 'Sửa campaign' : 'Tạo campaign mới'}
              </h3>
              <button
                onClick={() => setShowForm(false)}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100"
              >
                <FiX size={16} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Tên campaign <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400"
                  placeholder="VD: Khuyến mãi hè 2026"
                />
              </div>

              {/* Dates */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Bắt đầu <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="datetime-local"
                    value={form.startsAt}
                    onChange={(e) => setForm((p) => ({ ...p, startsAt: e.target.value }))}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Kết thúc <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="datetime-local"
                    value={form.endsAt}
                    onChange={(e) => setForm((p) => ({ ...p, endsAt: e.target.value }))}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400"
                  />
                </div>
              </div>

              {/* Scope */}
              <div className="space-y-3">
                <label className="flex items-center gap-2 text-sm font-medium text-gray-700 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={form.appliesToAll}
                    onChange={(e) =>
                      setForm((p) => ({ ...p, appliesToAll: e.target.checked, storeIds: [] }))
                    }
                    className="accent-rose-600"
                  />
                  Áp dụng cho tất cả chi nhánh
                </label>
                {!form.appliesToAll && (
                  <div>
                    <p className="text-xs text-gray-500 mb-2">Chọn chi nhánh áp dụng:</p>
                    {stores.length === 0 ? (
                      <p className="text-xs text-gray-400">Không có chi nhánh nào.</p>
                    ) : (
                      <div className="grid grid-cols-2 gap-1.5 max-h-40 overflow-y-auto border border-gray-200 rounded-lg p-2 bg-gray-50">
                        {stores.map((s) => {
                          const checked = form.storeIds.includes(s.id);
                          return (
                            <label
                              key={s.id}
                              className={`flex items-center gap-2 px-2 py-1.5 rounded-lg cursor-pointer text-xs transition-colors ${
                                checked ? 'bg-rose-50 text-rose-700 font-medium' : 'hover:bg-white text-gray-700'
                              }`}
                            >
                              <input
                                type="checkbox"
                                checked={checked}
                                onChange={(e) =>
                                  setForm((p) => ({
                                    ...p,
                                    storeIds: e.target.checked
                                      ? [...p.storeIds, s.id]
                                      : p.storeIds.filter((id) => id !== s.id),
                                  }))
                                }
                                className="accent-rose-600 shrink-0"
                              />
                              <span className="truncate">{s.name}</span>
                            </label>
                          );
                        })}
                      </div>
                    )}
                    {form.storeIds.length === 0 && (
                      <p className="text-xs text-amber-600 mt-1">Chưa chọn chi nhánh nào.</p>
                    )}
                  </div>
                )}
              </div>

              {/* Items */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-medium text-gray-700">Sản phẩm áp dụng</label>
                  <button
                    type="button"
                    onClick={addItem}
                    className="flex items-center gap-1 text-xs text-rose-600 hover:text-rose-700 font-medium"
                  >
                    <FiPlus size={12} />
                    Thêm dòng
                  </button>
                </div>
                {form.items.length === 0 ? (
                  <p className="text-sm text-gray-400 py-2">Chưa có item nào. Nhấn "Thêm dòng" để thêm.</p>
                ) : (
                  <div className="space-y-2">
                    {form.items.map((it, idx) => (
                      <div key={idx} className="border border-gray-200 rounded-xl overflow-hidden">
                        {/* Product selector row */}
                        <div className="flex items-center gap-2 px-3 py-2.5 bg-gray-50 border-b border-gray-100">
                          <select
                            value={it.productId}
                            onChange={(e) => handleProductChange(idx, e.target.value)}
                            className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:ring-1 focus:ring-rose-400 focus:outline-none bg-white"
                          >
                            {products.map((p) => (
                              <option key={p.id} value={p.id}>
                                {p.name}{p.hasSizes ? ' (có size)' : ''}
                              </option>
                            ))}
                          </select>
                          {!it.hasSizes && (
                            <>
                              <select
                                value={it.ruleType}
                                onChange={(e) => updateItem(idx, { ruleType: Number(e.target.value) })}
                                className="w-36 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-rose-400 bg-white"
                              >
                                {RULE_TYPES.map((r) => (
                                  <option key={r.value} value={r.value}>{r.label}</option>
                                ))}
                              </select>
                              <div className="relative w-24">
                                <input
                                  type="number"
                                  step="1"
                                  min="0"
                                  max={it.ruleType === 1 ? '99' : undefined}
                                  value={it.discountValue}
                                  onChange={(e) => updateItem(idx, { discountValue: e.target.value })}
                                  className="w-full border border-gray-200 rounded-lg px-2 py-1.5 pr-6 text-xs focus:outline-none focus:ring-1 focus:ring-rose-400"
                                  placeholder={it.ruleType === 1 ? 'VD: 10' : 'VD: 50000'}
                                />
                                {it.ruleType === 1 && (
                                  <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400 pointer-events-none">
                                    %
                                  </span>
                                )}
                              </div>
                            </>
                          )}
                          <button
                            type="button"
                            onClick={() => removeItem(idx)}
                            className="p-1 rounded text-gray-400 hover:text-red-500 shrink-0"
                          >
                            <FiX size={14} />
                          </button>
                        </div>

                        {/* Size price button — for products with sizes */}
                        {it.hasSizes && it.productId && (
                          <div className="px-3 py-2.5 bg-white flex items-center gap-3">
                            {it.sizeEntries.length === 0 ? (
                              <p className="text-xs text-orange-600">Sản phẩm này chưa có size master hợp lệ.</p>
                            ) : (
                              <>
                                <button
                                  type="button"
                                  onClick={() => setSizePopupIdx(idx)}
                                  className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border transition-colors ${
                                    it.sizeEntries.some(
                                      (se) => se.discountValue !== '' && parseFloat(se.discountValue) > 0,
                                    )
                                      ? 'border-rose-300 bg-rose-50 text-rose-700 hover:bg-rose-100'
                                      : 'border-gray-200 bg-white text-gray-600 hover:bg-gray-50'
                                  }`}
                                >
                                  <FiLayers size={12} />
                                  Cài giá theo size
                                </button>
                                {it.sizeEntries.some(
                                  (se) => se.discountValue !== '' && parseFloat(se.discountValue) > 0,
                                ) ? (
                                  <span className="text-xs text-gray-500">
                                    {
                                      it.sizeEntries.filter(
                                        (se) => se.discountValue !== '' && parseFloat(se.discountValue) > 0,
                                      ).length
                                    }{' '}
                                    size đã cài giá
                                  </span>
                                ) : (
                                  <span className="text-xs text-amber-600 flex items-center gap-1">
                                    <FiAlertTriangle size={11} />
                                    Chưa cài giá cho size nào
                                  </span>
                                )}
                              </>
                            )}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div className="flex items-center justify-end gap-3 pt-2 border-t">
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="px-4 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg disabled:opacity-50 flex items-center gap-2"
                >
                  {saving ? (
                    <span className="inline-block h-3.5 w-3.5 rounded-full border-2 border-white border-t-transparent animate-spin" />
                  ) : (
                    <FiCheck size={14} />
                  )}
                  {editingId ? 'Cập nhật' : 'Tạo mới'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Size price popup */}
      {sizePopupIdx !== null && form.items[sizePopupIdx]?.hasSizes && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm">
            <div className="flex items-center justify-between px-5 py-4 border-b">
              <div>
                <h4 className="text-sm font-semibold text-gray-800">Cài giá theo size</h4>
                <p className="text-xs text-gray-500 mt-0.5">
                  {products.find((p) => p.id === form.items[sizePopupIdx!].productId)?.name ?? ''}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setSizePopupIdx(null)}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
              >
                <FiX size={15} />
              </button>
            </div>

            <div className="px-5 py-4 space-y-3 max-h-72 overflow-y-auto">
              {form.items[sizePopupIdx!].sizeEntries.map((se, si) => {
                const hasValue = se.discountValue !== '' && parseFloat(se.discountValue) > 0;
                return (
                  <div
                    key={se.sizeId}
                    className={`rounded-lg border p-2.5 transition-colors ${
                      hasValue ? 'border-rose-200 bg-rose-50/50' : 'border-gray-200 bg-white'
                    }`}
                  >
                    <p className="text-xs font-semibold text-gray-800 mb-2">{se.sizeLabel}</p>
                    <div className="flex items-center gap-2">
                      <select
                        value={se.ruleType}
                        onChange={(e) => updateSizeEntry(sizePopupIdx!, si, { ruleType: Number(e.target.value) })}
                        className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-rose-400 bg-white"
                      >
                        {RULE_TYPES.map((r) => (
                          <option key={r.value} value={r.value}>{r.label}</option>
                        ))}
                      </select>
                      <div className="relative w-24">
                        <input
                          type="number"
                          step="1"
                          min="0"
                          max={se.ruleType === 1 ? '99' : undefined}
                          value={se.discountValue}
                          onChange={(e) =>
                            updateSizeEntry(sizePopupIdx!, si, { discountValue: e.target.value })
                          }
                          placeholder={se.ruleType === 1 ? 'VD: 10' : 'VD: 50000'}
                          className="w-full border border-gray-200 rounded-lg px-2 py-1.5 pr-6 text-xs focus:outline-none focus:ring-1 focus:ring-rose-400"
                        />
                        {se.ruleType === 1 && (
                          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400 pointer-events-none">
                            %
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>

            <div className="flex items-center justify-between px-5 py-3 border-t bg-gray-50 rounded-b-2xl">
              <span className="text-xs text-gray-500">
                {
                  form.items[sizePopupIdx!].sizeEntries.filter(
                    (se) => se.discountValue !== '' && parseFloat(se.discountValue) > 0,
                  ).length
                }{' '}
                /{' '}
                {form.items[sizePopupIdx!].sizeEntries.length} size đã cài giá
              </span>
              <button
                type="button"
                onClick={() => setSizePopupIdx(null)}
                className="px-4 py-1.5 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors"
              >
                Xong
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
