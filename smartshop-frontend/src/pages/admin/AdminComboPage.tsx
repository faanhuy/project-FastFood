import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { FiEdit2, FiTrash2, FiPlus, FiX } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { promotionService } from '../../services/promotionService';
import { productService } from '../../services/productService';
import { sizeService } from '../../services/sizeService';
import type {
  ComboSummaryDto,
  ComboDto,
  CreateComboRequest,
  CreateComboItemRequest,
} from '../../types/promotion';
import type { ProductDto } from '../../types/product';
import type { ProductSize } from '../../types/size';

interface ComboItemForm {
  productId: string;
  sizeId: string | null;
  quantity: string;
}

const defaultForm = (): {
  name: string;
  title: string;
  description: string;
  imageUrl: string;
  salePrice: string;
  startsAt: string;
  endsAt: string;
  items: ComboItemForm[];
} => ({
  name: '',
  title: '',
  description: '',
  imageUrl: '',
  salePrice: '',
  startsAt: '',
  endsAt: '',
  items: [],
});

function toDatetimeLocal(iso: string) {
  return iso ? iso.slice(0, 16) : '';
}

export default function AdminComboPage() {
  const [combos, setCombos] = useState<ComboSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(defaultForm());
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [itemSizes, setItemSizes] = useState<Record<number, ProductSize[]>>({});

  const loadCombos = async () => {
    setLoading(true);
    try {
      const result = await promotionService.getCombos();
      setCombos(result.items);
    } catch {
      toast.error('Không tải được danh sách combo');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCombos();
    productService.getProducts({ page: 1, pageSize: 200 }).then((r) => setProducts(r.items));
  }, []);

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
    setForm(defaultForm());
    setItemSizes({});
    setShowForm(true);
  };

  const openEdit = async (id: string) => {
    try {
      const dto: ComboDto = await promotionService.getComboById(id);
      setEditingId(id);
      setForm({
        name: dto.name,
        title: dto.title,
        description: dto.description ?? '',
        imageUrl: dto.imageUrl,
        salePrice: String(dto.salePrice),
        startsAt: toDatetimeLocal(dto.startsAt),
        endsAt: dto.endsAt ? toDatetimeLocal(dto.endsAt) : '',
        items: dto.items.map((it) => ({
          productId: it.productId,
          sizeId: it.sizeId,
          quantity: String(it.quantity),
        })),
      });
      const sizesMap: Record<number, ProductSize[]> = {};
      await Promise.all(
        dto.items.map(async (it, i) => {
          const sizes = await sizeService.getProductSizes(it.productId);
          sizesMap[i] = sizes;
        }),
      );
      setItemSizes(sizesMap);
      setShowForm(true);
    } catch {
      toast.error('Không tải được combo');
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Xóa combo "${name}"?`)) return;
    try {
      await promotionService.deleteCombo(id);
      toast.success('Đã xóa combo');
      loadCombos();
    } catch {
      toast.error('Xóa thất bại');
    }
  };

  const addItem = () => {
    setForm((f) => ({ ...f, items: [...f.items, { productId: '', sizeId: null, quantity: '1' }] }));
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
  };

  const updateItem = async (i: number, field: keyof ComboItemForm, value: string | null) => {
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim() || !form.title.trim() || !form.salePrice || !form.startsAt) {
      toast.error('Vui lòng điền đủ thông tin bắt buộc');
      return;
    }
    if (form.items.length === 0) {
      toast.error('Combo phải có ít nhất 1 sản phẩm');
      return;
    }

    const payload: CreateComboRequest = {
      name: form.name.trim(),
      title: form.title.trim(),
      description: form.description.trim() || null,
      imageUrl: form.imageUrl.trim(),
      salePrice: Number(form.salePrice),
      startsAt: new Date(form.startsAt).toISOString(),
      endsAt: form.endsAt ? new Date(form.endsAt).toISOString() : null,
      items: form.items.map(
        (it): CreateComboItemRequest => ({
          productId: it.productId,
          sizeId: it.sizeId || null,
          quantity: Number(it.quantity) || 1,
        }),
      ),
    };

    setSaving(true);
    try {
      if (editingId) {
        await promotionService.updateCombo(editingId, payload);
        toast.success('Đã cập nhật combo');
      } else {
        await promotionService.createCombo(payload);
        toast.success('Đã tạo combo mới');
      }
      setShowForm(false);
      loadCombos();
    } catch {
      toast.error('Lưu thất bại');
    } finally {
      setSaving(false);
    }
  };

  const fmt = (n: number) => n.toLocaleString('vi-VN') + 'đ';
  const fmtDate = (s: string) => new Date(s).toLocaleDateString('vi-VN');

  return (
    <AdminLayout title="Quản lý Combo">
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <p className="text-sm text-gray-500">{combos.length} combo</p>
          <button
            onClick={openCreate}
            className="flex items-center gap-2 bg-rose-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-rose-700"
          >
            <FiPlus size={15} /> Thêm combo
          </button>
        </div>

        {loading ? (
          <p className="text-center py-12 text-gray-400">Đang tải...</p>
        ) : combos.length === 0 ? (
          <p className="text-center py-12 text-gray-400">Chưa có combo nào</p>
        ) : (
          <div className="grid gap-3">
            {combos.map((c) => (
              <div
                key={c.id}
                className="bg-white rounded-xl border px-4 py-3 flex items-center gap-4"
              >
                {c.imageUrl && (
                  <img
                    src={c.imageUrl}
                    alt={c.name}
                    className="w-14 h-14 rounded-lg object-cover shrink-0"
                  />
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-semibold text-gray-800 truncate">{c.name}</p>
                  <p className="text-xs text-gray-500 truncate">{c.title}</p>
                  <div className="flex items-center gap-3 mt-1 text-xs text-gray-500">
                    <span>{c.itemCount} món</span>
                    <span className="text-gray-300">|</span>
                    <span className="line-through">{fmt(c.originalPrice)}</span>
                    <span className="text-rose-600 font-semibold">{fmt(c.salePrice)}</span>
                    <span className="text-gray-300">|</span>
                    <span>
                      {fmtDate(c.startsAt)} {c.endsAt ? `→ ${fmtDate(c.endsAt)}` : '(không hạn)'}
                    </span>
                    <span
                      className={`px-1.5 py-0.5 rounded text-[10px] font-medium ${
                        c.isCurrentlyActive
                          ? 'bg-green-100 text-green-700'
                          : 'bg-gray-100 text-gray-500'
                      }`}
                    >
                      {c.isCurrentlyActive ? 'Đang chạy' : 'Không hoạt động'}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <button
                    onClick={() => openEdit(c.id)}
                    className="p-2 text-gray-500 hover:text-blue-600"
                  >
                    <FiEdit2 size={15} />
                  </button>
                  <button
                    onClick={() => handleDelete(c.id, c.name)}
                    className="p-2 text-gray-500 hover:text-red-600"
                  >
                    <FiTrash2 size={15} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Form modal */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
            <div className="flex items-center justify-between px-5 py-4 border-b">
              <h2 className="font-semibold text-gray-800">
                {editingId ? 'Sửa combo' : 'Thêm combo mới'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-400 hover:text-gray-600">
                <FiX size={18} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto px-5 py-4 space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-gray-600">Tên combo *</label>
                  <input
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.name}
                    onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                    placeholder="combo-ga-lon"
                    required
                  />
                </div>
                <div>
                  <label className="text-xs font-medium text-gray-600">Tiêu đề hiển thị *</label>
                  <input
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.title}
                    onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                    placeholder="Combo Gà Lớn"
                    required
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-medium text-gray-600">Mô tả</label>
                <textarea
                  className="mt-1 w-full border rounded-lg px-3 py-2 text-sm resize-none"
                  rows={2}
                  value={form.description}
                  onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Mô tả ngắn về combo..."
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-gray-600">URL hình ảnh</label>
                  <input
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.imageUrl}
                    onChange={(e) => setForm((f) => ({ ...f, imageUrl: e.target.value }))}
                    placeholder="https://..."
                  />
                </div>
                <div>
                  <label className="text-xs font-medium text-gray-600">Giá khuyến mãi (đ) *</label>
                  <input
                    type="number"
                    min={0}
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.salePrice}
                    onChange={(e) => setForm((f) => ({ ...f, salePrice: e.target.value }))}
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-gray-600">Bắt đầu *</label>
                  <input
                    type="datetime-local"
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.startsAt}
                    onChange={(e) => setForm((f) => ({ ...f, startsAt: e.target.value }))}
                    required
                  />
                </div>
                <div>
                  <label className="text-xs font-medium text-gray-600">Kết thúc (tùy chọn)</label>
                  <input
                    type="datetime-local"
                    className="mt-1 w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.endsAt}
                    onChange={(e) => setForm((f) => ({ ...f, endsAt: e.target.value }))}
                  />
                </div>
              </div>

              {/* Items */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-xs font-medium text-gray-600">Sản phẩm trong combo</label>
                  <button
                    type="button"
                    onClick={addItem}
                    className="text-xs text-rose-600 hover:underline flex items-center gap-1"
                  >
                    <FiPlus size={12} /> Thêm món
                  </button>
                </div>

                {form.items.length === 0 && (
                  <p className="text-xs text-gray-400 italic">Chưa có món nào — nhấn "Thêm món"</p>
                )}

                <div className="space-y-2">
                  {form.items.map((item, i) => (
                    <div key={i} className="flex items-start gap-2 bg-gray-50 rounded-lg p-2">
                      <div className="flex-1 grid grid-cols-3 gap-2">
                        <select
                          className="border rounded px-2 py-1.5 text-xs col-span-1"
                          value={item.productId}
                          onChange={(e) => updateItem(i, 'productId', e.target.value)}
                        >
                          <option value="">-- Chọn sản phẩm --</option>
                          {products.map((p) => (
                            <option key={p.id} value={p.id}>
                              {p.name}
                            </option>
                          ))}
                        </select>

                        <select
                          className="border rounded px-2 py-1.5 text-xs"
                          value={item.sizeId ?? ''}
                          onChange={(e) => updateItem(i, 'sizeId', e.target.value || null)}
                        >
                          <option value="">Không có size</option>
                          {(itemSizes[i] ?? []).map((s) => (
                            <option key={s.id} value={s.id}>
                              {s.label}
                            </option>
                          ))}
                        </select>

                        <input
                          type="number"
                          min={1}
                          placeholder="SL"
                          className="border rounded px-2 py-1.5 text-xs"
                          value={item.quantity}
                          onChange={(e) => updateItem(i, 'quantity', e.target.value)}
                        />
                      </div>
                      <button
                        type="button"
                        onClick={() => removeItem(i)}
                        className="text-gray-400 hover:text-red-500 mt-1"
                      >
                        <FiX size={14} />
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </form>

            <div className="px-5 py-3 border-t flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="px-4 py-2 text-sm rounded-lg border text-gray-600 hover:bg-gray-50"
              >
                Hủy
              </button>
              <button
                onClick={handleSubmit}
                disabled={saving}
                className="px-4 py-2 text-sm rounded-lg bg-rose-600 text-white hover:bg-rose-700 disabled:opacity-50"
              >
                {saving ? 'Đang lưu...' : editingId ? 'Cập nhật' : 'Tạo combo'}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
