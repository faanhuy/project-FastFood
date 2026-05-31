import { useEffect, useState, useCallback } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiAlertTriangle, FiEdit2, FiCheck, FiX, FiPlusCircle, FiLayers, FiEye, FiTrash2, FiPlus, FiChevronDown, FiChevronRight } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { storeService } from '../../services/storeService';
import { sizeService } from '../../services/sizeService';
import { productService } from '../../services/productService';
import { stockReceiptService } from '../../services/stockReceiptService';
import type { Store, StoreInventory } from '../../types/store';
import type { ProductDto } from '../../types/product';
import type { ProductSize } from '../../types/size';
import type { StockReceiptDto, StockReceiptDetailDto, CreateStockReceiptRequest, ReceiptStatus } from '../../types/stockReceipt';

type TabKey = 'all' | 'lowStock' | 'bySize' | 'stockReceipt';

const toReceiptSizeEntry = (size: ProductSize) => ({
  sizeId: size.sizeId ?? '',
  sizeLabel: size.sizeLabel,
  quantity: '',
});

export default function InventoryManagementPage() {
  const { t } = useTranslation(['admin', 'common', 'toast']);
  const { t: tToast } = useTranslation('toast');
  const [stores, setStores] = useState<Store[]>([]);
  const [selectedStoreId, setSelectedStoreId] = useState<string>('');
  const [storesLoading, setStoresLoading] = useState(true);

  const [inventory, setInventory] = useState<StoreInventory[]>([]);
  const [inventoryLoading, setInventoryLoading] = useState(false);

  const [tab, setTab] = useState<TabKey>('stockReceipt');

  // By-size inventory state
  const [sizeProducts, setSizeProducts] = useState<ProductDto[]>([]);
  const [selectedSizeProductId, setSelectedSizeProductId] = useState<string>('');
  const [productSizes, setProductSizes] = useState<ProductSize[]>([]);
  const [sizeInventory, setSizeInventory] = useState<Record<string, number>>({});
  const [sizeInventoryLoading, setSizeInventoryLoading] = useState(false);

  // All products (for receipt item dropdown)
  const [allProducts, setAllProducts] = useState<ProductDto[]>([]);

  // Stock Receipt state
  const [receipts, setReceipts] = useState<StockReceiptDto[]>([]);
  const [receiptsLoading, setReceiptsLoading] = useState(false);
  const [receiptStatusFilter, setReceiptStatusFilter] = useState<ReceiptStatus | 'all'>('all');
  const [selectedReceipt, setSelectedReceipt] = useState<StockReceiptDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [createReceiptOpen, setCreateReceiptOpen] = useState(false);
  // Each row = one product. Non-sized: uses `quantity`. Sized: uses `sizeEntries`.
  const [receiptItems, setReceiptItems] = useState<Array<{
    productId: string;
    productName: string;
    hasSizes: boolean;
    quantity: string;        // used when hasSizes=false
    sizeEntries: Array<{ sizeId: string; sizeLabel: string; quantity: string }>; // used when hasSizes=true
    notes: string;
  }>>([]);
  const [receiptDate, setReceiptDate] = useState(new Date().toISOString().split('T')[0]);
  const [receiptNotes, setReceiptNotes] = useState('');
  const [creatingReceipt, setCreatingReceipt] = useState(false);
  const [completingReceipt, setCompletingReceipt] = useState(false);
  const [cancellingReceipt, setCancellingReceipt] = useState(false);
  const [editReceiptId, setEditReceiptId] = useState<string | null>(null);
  const [sizePopupIdx, setSizePopupIdx] = useState<number | null>(null);
  const [collapsedDetailProducts, setCollapsedDetailProducts] = useState<Record<string, boolean>>({});

  // Load stores on mount
  useEffect(() => {
    setStoresLoading(true);
    storeService
      .getStores()
      .then((list) => {
        setStores(list);
        if (list.length > 0) setSelectedStoreId(list[0].id);
      })
      .catch(() => toast.error(t('storeLoadFailed', { ns: 'toast' })))
      .finally(() => setStoresLoading(false));
  }, []);

  // Load inventory for selected store
  const loadInventory = useCallback(async (storeId: string) => {
    if (!storeId) return;
    setInventoryLoading(true);
    try {
      const data = await storeService.getStoreInventory(storeId);
      setInventory(data);
    } catch {
      toast.error(t('inventoryLoadFailed', { ns: 'toast' }));
    } finally {
      setInventoryLoading(false);
    }
  }, []);

  useEffect(() => {
    if (selectedStoreId) loadInventory(selectedStoreId);
  }, [selectedStoreId, loadInventory]);

  // Load stock receipts for selected store
  const loadReceipts = useCallback(async (storeId: string) => {
    if (!storeId) return;
    setReceiptsLoading(true);
    try {
      const status = receiptStatusFilter !== 'all' ? receiptStatusFilter : undefined;
      const data = await stockReceiptService.getList(storeId, 1, 50, status);
      setReceipts(data.items);
    } catch {
      toast.error(t('stockReceiptLoadFailed', { ns: 'toast' }));
    } finally {
      setReceiptsLoading(false);
    }
  }, [receiptStatusFilter]);

  useEffect(() => {
    if (tab === 'stockReceipt' && selectedStoreId) {
      loadReceipts(selectedStoreId);
    }
  }, [tab, selectedStoreId, loadReceipts]);

  // Preload all products when on stockReceipt tab (for receipt item dropdown)
  useEffect(() => {
    if (tab !== 'stockReceipt') return;
    if (allProducts.length > 0) return;
    productService.getProducts({ pageSize: 200 })
      .then((r) => setAllProducts(r?.items ?? []))
      .catch(() => {});
  }, [tab]);

  // Load products with sizes whenever entering bySize tab
  useEffect(() => {
    if (tab !== 'bySize') return;
    productService.getProducts({ pageSize: 200 }).then((r) => {
      const withSizes = (r?.items ?? []).filter((p) => p.hasSizes);
      setSizeProducts(withSizes);
      if (withSizes.length > 0 && !selectedSizeProductId) {
        setSelectedSizeProductId(withSizes[0].id);
      }
    }).catch(() => {});
  }, [tab]);

  // Load sizes for selected product in bySize tab
  useEffect(() => {
    if (!selectedSizeProductId || tab !== 'bySize') return;
    setSizeInventoryLoading(true);
    setProductSizes([]);
    setSizeInventory({});
    sizeService.getProductSizes(selectedSizeProductId)
      .then((sizes) => {
        setProductSizes(sizes.filter((s) => s.isActive));
      })
      .catch(() => {})
      .finally(() => setSizeInventoryLoading(false));
  }, [selectedSizeProductId, tab]);

  const displayedItems = tab === 'lowStock'
    ? inventory.filter((i) => i.quantity <= i.lowStockThreshold)
    : inventory;

  const lowStockCount = inventory.filter((i) => i.quantity <= i.lowStockThreshold).length;
  const detailProductGroups = selectedReceipt
    ? Array.from(
        selectedReceipt.items.reduce((groups, item) => {
          const existing = groups.get(item.productId);
          const row = {
            id: item.id,
            sizeLabel: item.sizeLabel,
            quantity: item.quantity,
            notes: item.notes,
          };

          if (existing) {
            existing.items.push(row);
            existing.totalQuantity += item.quantity;
          } else {
            groups.set(item.productId, {
              productId: item.productId,
              productName: item.productName,
              totalQuantity: item.quantity,
              items: [row],
            });
          }

          return groups;
        }, new Map<string, {
          productId: string;
          productName: string;
          totalQuantity: number;
          items: Array<{
            id: string;
            sizeLabel: string | null;
            quantity: number;
            notes: string | null;
          }>;
        }>()),
      ).map(([, group]) => group)
    : [];

  return (
    <AdminLayout title={t('manageInventory')}>
      {/* Store selector */}
      <div className="flex items-center gap-3 mb-6">
        <label className="text-sm font-medium text-gray-700 shrink-0">{t('branch')}:</label>
        {storesLoading ? (
          <div className="h-9 w-48 bg-gray-100 rounded-lg animate-pulse" />
        ) : (
          <select
            value={selectedStoreId}
            onChange={(e) => setSelectedStoreId(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400 bg-white"
          >
            {stores.map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
        )}
      </div>

      {/* Tabs + Add button */}
      <div className="flex items-end justify-between mb-4 border-b">
        <div className="flex gap-2">
          <button
            onClick={() => setTab('stockReceipt')}
            className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors flex items-center gap-1.5 ${
              tab === 'stockReceipt' ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            <FiPlusCircle size={13} />
            {t('stockReceiptLabel')}
          </button>
          <button
            onClick={() => setTab('all')}
            className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors ${
              tab === 'all' ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            {t('all', { ns: 'common' })} ({inventory.length})
          </button>
          <button
            onClick={() => setTab('lowStock')}
            className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors flex items-center gap-1.5 ${
              tab === 'lowStock' ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            <FiAlertTriangle size={13} />
            {t('lowStock')}
            {lowStockCount > 0 && (
              <span className="bg-red-100 text-red-600 text-xs px-1.5 py-0.5 rounded-full font-semibold">
                {lowStockCount}
              </span>
            )}
          </button>
          <button
            onClick={() => setTab('bySize')}
            className={`pb-2.5 px-1 text-sm font-medium border-b-2 transition-colors flex items-center gap-1.5 ${
              tab === 'bySize' ? 'border-rose-600 text-rose-600' : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            <FiLayers size={13} />
            {t('bySize')}
          </button>
        </div>

        {tab === 'stockReceipt' && (
          <button
            onClick={() => {
              setEditReceiptId(null);
              setCreateReceiptOpen(true);
              setReceiptItems([{ productId: '', productName: '', hasSizes: false, quantity: '', sizeEntries: [], notes: '' }]);
              setReceiptDate(new Date().toISOString().split('T')[0]);
              setReceiptNotes('');
            }}
            className="mb-2 flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors shrink-0"
          >
            <FiPlus size={14} />
            {t('createStockReceipt')}
          </button>
        )}
      </div>

      {/* Inventory table — read-only, shown for all/lowStock tabs */}
      {(tab === 'all' || tab === 'lowStock') && <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        {inventoryLoading ? (
          <div className="p-8 text-center text-gray-400 text-sm">{t('loading', { ns: 'common' })}</div>
        ) : displayedItems.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">
            {tab === 'lowStock' ? t('noLowStockItems') : t('noInventoryData')}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('product', { ns: 'common' })}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('stock')}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('lowStockThreshold')}</th>
                <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('status')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {displayedItems.map((item) => {
                const isLow = item.quantity <= item.lowStockThreshold;
                return (
                  <tr key={item.productId} className={isLow ? 'bg-red-50/60' : 'hover:bg-gray-50'}>
                    <td className="px-5 py-3 font-medium text-gray-800">{item.productName}</td>

                    <td className="px-5 py-3 text-center">
                      <span className={`font-semibold ${isLow ? 'text-red-600' : 'text-gray-800'}`}>
                        {item.quantity}
                      </span>
                    </td>

                    <td className="px-5 py-3 text-center text-gray-500">{item.lowStockThreshold}</td>

                    <td className="px-5 py-3 text-center">
                      {isLow ? (
                        <span className="inline-flex items-center gap-1 bg-red-100 text-red-700 text-xs px-2 py-0.5 rounded-full font-medium">
                          <FiAlertTriangle size={10} />
                          {t('lowStock')}
                        </span>
                      ) : (
                        <span className="inline-block bg-green-100 text-green-700 text-xs px-2 py-0.5 rounded-full font-medium">
                          {t('normal')}
                        </span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>}

      {/* Stock Receipt section */}
      {tab === 'stockReceipt' && (
        <div className="space-y-4">
          {/* Filter */}
          <div className="flex items-center gap-3 flex-wrap">
            <label className="text-sm font-medium text-gray-700 shrink-0">{t('status')}:</label>
            <div className="flex gap-2">
              {(['all', 'Pending', 'Completed', 'Cancelled'] as const).map((status) => (
                <button
                  key={status}
                  onClick={() => setReceiptStatusFilter(status)}
                  className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                    receiptStatusFilter === status
                      ? 'bg-rose-600 text-white'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  {status === 'all' ? t('all', { ns: 'common' }) : status === 'Pending' ? t('pending') : status === 'Completed' ? t('completed') : t('cancelled')}
                </button>
              ))}
            </div>
          </div>

          {/* Receipts table */}
          <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
            {receiptsLoading ? (
              <div className="p-8 text-center text-gray-400 text-sm">{t('loading', { ns: 'common' })}</div>
            ) : receipts.length === 0 ? (
              <div className="p-8 text-center text-gray-400 text-sm">{t('noReceipts')}</div>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-gray-50">
                    <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('receiptNumber')}</th>
                    <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('receiptDate')}</th>
                    <th className="text-left px-5 py-3 font-semibold text-gray-600 hidden sm:table-cell">{t('notes')}</th>
                    <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('status')}</th>
                    <th className="text-right px-5 py-3 font-semibold text-gray-600">{t('actions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {receipts.map((receipt) => (
                    <tr key={receipt.id} className="hover:bg-gray-50">
                      <td className="px-5 py-3 font-medium text-gray-800">{receipt.receiptNumber}</td>
                      <td className="px-5 py-3 text-gray-600">
                        {new Date(receipt.receiptDate).toLocaleDateString('vi-VN')}
                      </td>
                      <td className="px-5 py-3 text-gray-600 text-xs hidden sm:table-cell truncate">
                        {receipt.notes || '—'}
                      </td>
                      <td className="px-5 py-3 text-center">
                        <span
                          className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${
                            receipt.status === 'Pending'
                              ? 'bg-yellow-100 text-yellow-700'
                              : receipt.status === 'Completed'
                              ? 'bg-green-100 text-green-700'
                              : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {receipt.status === 'Pending' ? t('pending') : receipt.status === 'Completed' ? t('completed') : t('cancelled')}
                        </span>
                      </td>
                      <td className="px-5 py-3 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={async () => {
                              setDetailLoading(true);
                              try {
                                const detail = await stockReceiptService.getById(receipt.id);
                                setCollapsedDetailProducts({});
                                setSelectedReceipt(detail);
                              } catch {
                                toast.error(t('error', { ns: 'common' }));
                              } finally {
                                setDetailLoading(false);
                              }
                            }}
                            className="p-1.5 rounded-lg text-gray-500 hover:bg-blue-50 hover:text-blue-600 transition-colors"
                            title={t('view')}
                          >
                            <FiEye size={14} />
                          </button>
                          {receipt.status === 'Pending' && (
                            <button
                              onClick={async () => {
                                if (allProducts.length === 0) {
                                  try {
                                    const result = await productService.getProducts({ pageSize: 200 });
                                    setAllProducts(result?.items ?? []);
                                  } catch { /* ignore */ }
                                }
                                setDetailLoading(true);
                                try {
                                  const detail = await stockReceiptService.getById(receipt.id);
                                  setEditReceiptId(receipt.id);
                                  setReceiptDate(detail.receiptDate.split('T')[0]);
                                  setReceiptNotes(detail.notes ?? '');

                                  // Group items by productId; sized products get sizeEntries
                                  type GroupEntry = {
                                    productId: string; productName: string; isSized: boolean;
                                    simpleQty?: string;
                                    existingBySize: Record<string, string>; // sizeId -> qty
                                    notes: string;
                                  };
                                  const groups = new Map<string, GroupEntry>();
                                  for (const item of detail.items) {
                                    const existing = groups.get(item.productId);
                                    if (existing) {
                                      if (item.sizeId) existing.existingBySize[item.sizeId] = String(item.quantity);
                                    } else {
                                      groups.set(item.productId, {
                                        productId: item.productId, productName: item.productName,
                                        isSized: item.sizeId !== null,
                                        simpleQty: item.sizeId ? undefined : String(item.quantity),
                                        existingBySize: item.sizeId ? { [item.sizeId]: String(item.quantity) } : {},
                                        notes: item.notes ?? '',
                                      });
                                    }
                                  }

                                  const rows: typeof receiptItems = [];
                                  for (const g of groups.values()) {
                                    if (!g.isSized) {
                                      rows.push({ productId: g.productId, productName: g.productName, hasSizes: false, quantity: g.simpleQty ?? '', sizeEntries: [], notes: g.notes });
                                    } else {
                                      const allSizes = await sizeService.getProductSizes(g.productId);
                                      const activeSizes = allSizes.filter(s => s.isActive && s.sizeId);
                                      rows.push({
                                        productId: g.productId, productName: g.productName, hasSizes: true, quantity: '',
                                        sizeEntries: activeSizes.map(s => {
                                          const receiptSizeId = s.sizeId as string;
                                          return {
                                            sizeId: receiptSizeId,
                                            sizeLabel: s.sizeLabel,
                                            quantity: g.existingBySize[receiptSizeId] ?? '',
                                          };
                                        }),
                                        notes: g.notes,
                                      });
                                    }
                                  }
                                  setReceiptItems(rows);
                                  setCreateReceiptOpen(true);
                                } catch {
                                  toast.error(t('error', { ns: 'common' }));
                                } finally {
                                  setDetailLoading(false);
                                }
                              }}
                              disabled={detailLoading}
                              className="p-1.5 rounded-lg text-gray-500 hover:bg-yellow-50 hover:text-yellow-600 transition-colors disabled:opacity-50"
                              title={t('edit')}
                            >
                              <FiEdit2 size={14} />
                            </button>
                          )}
                          {receipt.status === 'Pending' && (
                            <button
                              onClick={async () => {
                                if (!confirm(t('confirmCompleteReceipt'))) return;
                                setCompletingReceipt(true);
                                try {
                                  await stockReceiptService.complete(receipt.id);
                                  setReceipts((prev) =>
                                    prev.map((r) =>
                                      r.id === receipt.id ? { ...r, status: 'Completed' as const } : r,
                                    ),
                                  );
                                  toast.success(t('stockReceiptCompleted', { ns: 'toast' }));
                                } catch (err: any) {
                                  toast.error(err.response?.data?.message ?? t('stockReceiptCompleteFailed', { ns: 'toast' }));
                                } finally {
                                  setCompletingReceipt(false);
                                }
                              }}
                              disabled={completingReceipt}
                              className="p-1.5 rounded-lg text-gray-500 hover:bg-green-50 hover:text-green-600 transition-colors disabled:opacity-50"
                              title={t('complete')}
                            >
                              <FiCheck size={14} />
                            </button>
                          )}
                          {(receipt.status === 'Pending' || receipt.status === 'Completed') && (
                            <button
                              onClick={async () => {
                                const msg = receipt.status === 'Completed'
                                  ? t('confirmCancelCompleted')
                                  : t('confirmCancelReceipt');
                                if (!confirm(msg)) return;
                                setCancellingReceipt(true);
                                try {
                                  await stockReceiptService.cancel(receipt.id);
                                  setReceipts((prev) =>
                                    prev.map((r) =>
                                      r.id === receipt.id ? { ...r, status: 'Cancelled' as const } : r,
                                    ),
                                  );
                                  toast.success(t('stockReceiptCancelled', { ns: 'toast' }));
                                } catch (err: any) {
                                  toast.error(err.response?.data?.message ?? t('stockReceiptCancelFailed', { ns: 'toast' }));
                                } finally {
                                  setCancellingReceipt(false);
                                }
                              }}
                              disabled={cancellingReceipt}
                              className="p-1.5 rounded-lg text-gray-500 hover:bg-red-50 hover:text-red-600 transition-colors disabled:opacity-50"
                              title={t('cancel', { ns: 'common' })}
                            >
                              <FiX size={14} />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}

      {/* By-size inventory section */}
      {tab === 'bySize' && (
        <div className="mt-2">
          <div className="flex items-center gap-3 mb-4">
            <label className="text-sm font-medium text-gray-700 shrink-0">{t('inventoryProductLabel')}</label>
            {sizeProducts.length === 0 ? (
              <p className="text-sm text-gray-400">{t('inventoryNoSizesMsg')}</p>
            ) : (
              <select
                value={selectedSizeProductId}
                onChange={(e) => setSelectedSizeProductId(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400 bg-white"
              >
                {sizeProducts.map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            )}
          </div>

          <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
            {sizeInventoryLoading ? (
              <div className="p-8 text-center text-gray-400 text-sm">{t('inventoryLoadingSizes')}</div>
            ) : productSizes.length === 0 ? (
              <div className="p-8 text-center text-gray-400 text-sm">{t('inventorySelectProductMsg')}</div>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-gray-50">
                    <th className="text-left px-5 py-3 font-semibold text-gray-600">{t('inventorySizeCol')}</th>
                    <th className="text-center px-5 py-3 font-semibold text-gray-600">{t('inventoryStockCol')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {productSizes.map((sz) => {
                    const qty = sizeInventory[sz.id] ?? 0;
                    return (
                      <tr key={sz.id} className="hover:bg-gray-50">
                        <td className="px-5 py-3 font-medium text-gray-800">{sz.sizeLabel}</td>
                        <td className="px-5 py-3 text-center">
                          <span className="font-semibold text-gray-800">{qty}</span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}

      {/* Stock Receipt Detail Modal */}
      {selectedReceipt && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4 overflow-y-auto">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-3xl my-4">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <div>
                <h3 className="text-base font-semibold text-gray-800">{t('inventoryReceiptDetail')}</h3>
                <p className="text-xs text-gray-500 mt-0.5">{selectedReceipt.receiptNumber}</p>
              </div>
              <button
                onClick={() => {
                  setSelectedReceipt(null);
                  setCollapsedDetailProducts({});
                }}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
              >
                <FiX size={16} />
              </button>
            </div>

            <div className="px-6 py-4 space-y-3 max-h-96 overflow-y-auto">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-gray-500 text-xs">{t('inventoryReceiptDate')}</p>
                  <p className="font-medium text-gray-800">
                    {new Date(selectedReceipt.receiptDate).toLocaleDateString('vi-VN')}
                  </p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">{t('inventoryReceiptStatus')}</p>
                  <span
                    className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${
                      selectedReceipt.status === 'Pending'
                        ? 'bg-yellow-100 text-yellow-700'
                        : selectedReceipt.status === 'Completed'
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-700'
                    }`}
                  >
                    {selectedReceipt.status === 'Pending' ? t('pending') : selectedReceipt.status === 'Completed' ? t('completed') : t('cancelled')}
                  </span>
                </div>
              </div>

              {selectedReceipt.notes && (
                <div>
                  <p className="text-gray-500 text-xs mb-1">{t('notes')}</p>
                  <p className="text-sm text-gray-800">{selectedReceipt.notes}</p>
                </div>
              )}

              <div>
                <p className="text-gray-500 text-xs mb-2 font-medium">{t('inventoryProductList')}</p>
                <div className="space-y-2">
                  {detailProductGroups.map((group) => {
                    const hasSizes = group.items.some((item) => item.sizeLabel);
                    const isCollapsed = hasSizes && collapsedDetailProducts[group.productId];
                    return (
                      <div key={group.productId} className="overflow-hidden rounded-lg border border-gray-200 text-sm">
                        <div className="flex items-start justify-between gap-3 bg-gray-50 px-3 py-2.5">
                          {hasSizes ? (
                            <button
                              type="button"
                              onClick={() => setCollapsedDetailProducts((prev) => ({
                                ...prev,
                                [group.productId]: !prev[group.productId],
                              }))}
                              className="flex min-w-0 flex-1 items-start gap-2 rounded-md px-1 py-1 text-left hover:bg-gray-100"
                            >
                              {isCollapsed ? (
                                <FiChevronRight size={15} className="mt-0.5 shrink-0 text-gray-500" />
                              ) : (
                                <FiChevronDown size={15} className="mt-0.5 shrink-0 text-gray-500" />
                              )}
                              <span className="min-w-0">
                                <span className="block truncate font-semibold text-gray-800">{group.productName}</span>
                                <span className="mt-0.5 block text-xs text-gray-500">{group.items.length} {t('admin:inventorySizeCol').toLowerCase()}</span>
                              </span>
                            </button>
                          ) : (
                            <div className="min-w-0 flex-1 px-1 py-1">
                              <p className="truncate font-semibold text-gray-800">{group.productName}</p>
                              <p className="mt-0.5 text-xs text-gray-500">{t('inventoryNoSizeManaged')}</p>
                            </div>
                          )}
                          <div className="rounded-md bg-white px-2.5 py-1 text-right shadow-sm ring-1 ring-gray-200">
                            <p className="text-[11px] text-gray-500">{t('inventoryTotalQty')}</p>
                            <p className="text-sm font-semibold text-gray-800">{group.totalQuantity}</p>
                          </div>
                        </div>

                        {!isCollapsed && hasSizes ? (
                          <table className="w-full text-xs">
                            <thead className="bg-white text-gray-500">
                              <tr className="border-t border-gray-100">
                                <th className="px-3 py-2 text-left font-semibold">{t('inventorySizeHeader')}</th>
                                <th className="w-24 px-3 py-2 text-center font-semibold">{t('inventoryQtyHeader')}</th>
                                <th className="hidden px-3 py-2 text-left font-semibold sm:table-cell">{t('inventoryNotesHeader')}</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                              {group.items.map((item) => (
                                <tr key={item.id} className="hover:bg-gray-50">
                                  <td className="px-3 py-2 font-medium text-gray-800">{item.sizeLabel || '-'}</td>
                                  <td className="px-3 py-2 text-center">
                                    <span className="inline-flex min-w-10 justify-center rounded-md bg-rose-50 px-2 py-0.5 font-semibold text-rose-700">
                                      {item.quantity}
                                    </span>
                                  </td>
                                  <td className="hidden px-3 py-2 text-gray-500 sm:table-cell">{item.notes || '-'}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        ) : null}

                        {!isCollapsed && !hasSizes && group.items[0]?.notes && (
                          <div className="border-t border-gray-100 px-3 py-2 text-xs text-gray-500">
                            {t('notes')}: {group.items[0].notes}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>

            <div className="flex items-center justify-end gap-3 px-6 py-4 bg-gray-50 border-t rounded-b-2xl">
              <button
                onClick={() => {
                  setSelectedReceipt(null);
                  setCollapsedDetailProducts({});
                }}
                className="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 hover:bg-gray-100 rounded-lg transition-colors"
              >
                {t('close', { ns: 'common' })}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create / Edit Stock Receipt Modal */}
      {createReceiptOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4 overflow-y-auto">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl my-4">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h3 className="text-base font-semibold text-gray-800">
                {editReceiptId ? t('inventoryReceiptEditTitle') : t('inventoryReceiptCreateTitle')}
              </h3>
              <button
                onClick={() => { setCreateReceiptOpen(false); setEditReceiptId(null); }}
                disabled={creatingReceipt}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors disabled:opacity-50"
              >
                <FiX size={16} />
              </button>
            </div>

            <div className="px-6 py-4 space-y-4 max-h-[60vh] overflow-y-auto">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">{t('inventoryReceiptDateLabel')}</label>
                  <input
                    type="date"
                    value={receiptDate}
                    onChange={(e) => setReceiptDate(e.target.value)}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-rose-400 focus:outline-none"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('inventoryReceiptNotesLabel')}</label>
                <textarea
                  value={receiptNotes}
                  onChange={(e) => setReceiptNotes(e.target.value)}
                  rows={2}
                  placeholder={t('notePlaceholder')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-rose-400 focus:outline-none"
                />
              </div>

              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="block text-sm font-medium text-gray-700">{t('inventoryProductsInputLabel')}</label>
                  <button
                    type="button"
                    onClick={() =>
                      setReceiptItems([
                        ...receiptItems,
                        { productId: '', productName: '', hasSizes: false, quantity: '', sizeEntries: [], notes: '' },
                      ])
                    }
                    className="text-xs text-rose-600 hover:text-rose-700 font-medium flex items-center gap-1"
                  >
                    <FiPlus size={11} />
                    {t('inventoryAddProductBtn')}
                  </button>
                </div>

                <div className="space-y-3 max-h-72 overflow-y-auto pr-0.5">
                  {receiptItems.map((item, idx) => (
                    <div key={idx} className="border border-gray-200 rounded-xl overflow-hidden">
                      {/* Product selector row */}
                      <div className="flex items-center gap-2 px-3 py-2.5 bg-gray-50 border-b border-gray-100">
                        <select
                          value={item.productId}
                          onChange={async (e) => {
                            const pid = e.target.value;
                            const prod = allProducts.find((p) => p.id === pid);
                            const hasSizes = prod?.hasSizes ?? false;
                            if (hasSizes && pid) {
                              try {
                                const sizes = await sizeService.getProductSizes(pid);
                                const activeSizes = sizes.filter(s => s.isActive && s.sizeId);
                                setReceiptItems(items => items.map((it, i) =>
                                  i === idx ? { ...it, productId: pid, productName: prod?.name ?? '', hasSizes: true, quantity: '', sizeEntries: activeSizes.map(toReceiptSizeEntry) } : it,
                                ));
                              } catch {
                                toast.error(tToast('sizeLoadFailed'));
                              }
                            } else {
                              setReceiptItems(items => items.map((it, i) =>
                                i === idx ? { ...it, productId: pid, productName: prod?.name ?? '', hasSizes: false, quantity: '', sizeEntries: [] } : it,
                              ));
                            }
                          }}
                          className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:ring-1 focus:ring-rose-400 focus:outline-none bg-white"
                        >
                          <option value="">{t('inventorySelectProductOption')}</option>
                          {allProducts.map((p) => (
                            <option key={p.id} value={p.id}>
                              {p.name}{p.hasSizes ? t('inventoryProductWithSize') : ''}
                            </option>
                          ))}
                        </select>

                        {/* Quantity input — only for non-sized */}
                        {!item.hasSizes && (
                          <input
                            type="number"
                            min={1}
                            value={item.quantity}
                            onChange={(e) =>
                              setReceiptItems(items => items.map((it, i) => i === idx ? { ...it, quantity: e.target.value } : it))
                            }
                            placeholder={t('inventoryQtyPlaceholder')}
                            className="w-20 border border-gray-200 rounded-lg px-2 py-1.5 text-xs text-center focus:ring-1 focus:ring-rose-400 focus:outline-none bg-white"
                          />
                        )}

                        <button
                          type="button"
                          onClick={() => {
                            setReceiptItems(items => items.filter((_, i) => i !== idx));
                            setSizePopupIdx(null);
                          }}
                          className="p-1.5 rounded-lg text-gray-400 hover:bg-red-50 hover:text-red-500 transition-colors shrink-0"
                          title={t('inventoryDeleteRowTitle')}
                        >
                          <FiTrash2 size={13} />
                        </button>
                      </div>

                      {/* Size entry button — opens popup */}
                      {item.hasSizes && item.productId && (
                        <div className="px-3 py-2.5 bg-white border-t border-gray-100 flex items-center gap-3">
                          {item.sizeEntries.length === 0 ? (
                            <p className="text-xs text-orange-600">{t('inventoryNoValidSize')}</p>
                          ) : (
                            <>
                              <button
                                type="button"
                                onClick={() => setSizePopupIdx(idx)}
                                className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border transition-colors ${
                                  item.sizeEntries.some(se => parseInt(se.quantity, 10) > 0)
                                    ? 'border-rose-300 bg-rose-50 text-rose-700 hover:bg-rose-100'
                                    : 'border-gray-200 bg-white text-gray-600 hover:bg-gray-50'
                                }`}
                              >
                                <FiLayers size={12} />
                                {t('inventoryInputBySizeBtn')}
                              </button>
                              {item.sizeEntries.some(se => parseInt(se.quantity, 10) > 0) ? (
                                <span className="text-xs text-gray-500">
                                  {item.sizeEntries.filter(se => parseInt(se.quantity, 10) > 0).length}{t('inventoryQtyBySize')}{' '}
                                  <span className="font-semibold text-gray-800">
                                    {item.sizeEntries.reduce((sum, se) => sum + (parseInt(se.quantity, 10) || 0), 0)}{t('inventoryTotalProducts')}
                                  </span>
                                </span>
                              ) : (
                                <span className="text-xs text-amber-600 flex items-center gap-1">
                                  <FiAlertTriangle size={11} />
                                  {t('inventoryNotEnteredQty')}
                                </span>
                              )}
                            </>
                          )}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3 px-6 py-4 bg-gray-50 border-t rounded-b-2xl">
              <button
                onClick={() => { setCreateReceiptOpen(false); setEditReceiptId(null); }}
                disabled={creatingReceipt}
                className="flex-1 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
              >
                {t('inventoryCancelBtn')}
              </button>
              <button
                onClick={async () => {
                  // Validate: every item needs a product; non-sized needs qty; sized needs ≥1 size with qty
                  const invalid = receiptItems.some(it => {
                    if (!it.productId) return true;
                    if (!it.hasSizes) return !it.quantity || parseInt(it.quantity, 10) <= 0;
                    return !it.sizeEntries.some(se => se.quantity !== '' && parseInt(se.quantity, 10) > 0);
                  });
                  if (receiptItems.length === 0 || invalid) {
                    toast.error(tToast('receiptFormRequired'));
                    return;
                  }
                  setCreatingReceipt(true);
                  try {
                    // Flatten items to API shape
                    const mappedItems: Array<{ productId: string; sizeId: string | null; quantity: number; notes?: string }> = [];
                    for (const it of receiptItems) {
                      if (!it.hasSizes) {
                        mappedItems.push({ productId: it.productId, sizeId: null, quantity: parseInt(it.quantity, 10), notes: it.notes || undefined });
                      } else {
                        for (const se of it.sizeEntries) {
                          if (se.quantity !== '' && parseInt(se.quantity, 10) > 0) {
                            mappedItems.push({ productId: it.productId, sizeId: se.sizeId, quantity: parseInt(se.quantity, 10), notes: it.notes || undefined });
                          }
                        }
                      }
                    }
                    if (editReceiptId) {
                      const updated = await stockReceiptService.update(editReceiptId, {
                        receiptDate,
                        notes: receiptNotes || undefined,
                        items: mappedItems,
                      });
                      setReceipts((prev) => prev.map((r) => r.id === editReceiptId ? { ...r, receiptDate: updated.receiptDate, notes: updated.notes } : r));
                      toast.success(tToast('stockReceiptUpdated'));
                    } else {
                      const req: CreateStockReceiptRequest = {
                        storeId: selectedStoreId,
                        receiptDate,
                        notes: receiptNotes || undefined,
                        items: mappedItems,
                      };
                      const created = await stockReceiptService.create(req);
                      setReceipts((prev) => [created, ...prev]);
                      toast.success(tToast('stockReceiptCreated'));
                    }
                    setCreateReceiptOpen(false);
                    setEditReceiptId(null);
                  } catch (err: any) {
                    toast.error(err.response?.data?.message ?? (editReceiptId ? tToast('stockReceiptUpdateFailed') : tToast('stockReceiptCreateFailed')));
                  } finally {
                    setCreatingReceipt(false);
                  }
                }}
                disabled={creatingReceipt}
                className="flex-1 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors disabled:opacity-50"
              >
                {creatingReceipt ? t('inventorySavingBtn') : editReceiptId ? t('inventoryUpdateBtn') : t('inventoryCreateBtn')}
              </button>
            </div>
          </div>
        </div>
      )}
      {/* Size entry popup */}
      {sizePopupIdx !== null && receiptItems[sizePopupIdx]?.hasSizes && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm">
            <div className="flex items-center justify-between px-5 py-4 border-b">
              <div>
                <h4 className="text-sm font-semibold text-gray-800">{t('inventorySizePopupTitle')}</h4>
                <p className="text-xs text-gray-500 mt-0.5">{receiptItems[sizePopupIdx].productName}</p>
              </div>
              <button
                type="button"
                onClick={() => setSizePopupIdx(null)}
                className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
              >
                <FiX size={15} />
              </button>
            </div>
            <div className="px-5 py-4 space-y-2.5 max-h-72 overflow-y-auto">
              {receiptItems[sizePopupIdx].sizeEntries.map((se, si) => {
                const qty = parseInt(se.quantity, 10) || 0;
                const hasQty = qty > 0;
                return (
                  <div key={se.sizeId} className="flex items-center gap-3">
                    <span className="flex-1 text-sm font-medium text-gray-700">{se.sizeLabel}</span>
                    <input
                      type="number"
                      min={0}
                      value={se.quantity}
                      onChange={(e) => {
                        const val = e.target.value;
                        const popupIdx = sizePopupIdx;
                        setReceiptItems(items => items.map((it, i) =>
                          i === popupIdx
                            ? { ...it, sizeEntries: it.sizeEntries.map((s, j) => j === si ? { ...s, quantity: val } : s) }
                            : it,
                        ));
                      }}
                      placeholder="0"
                      className={`w-24 h-9 border rounded-lg px-3 text-center text-sm font-medium focus:outline-none focus:ring-2 focus:ring-rose-400 ${
                        hasQty ? 'border-rose-300 bg-rose-50 text-rose-700' : 'border-gray-200 text-gray-700'
                      }`}
                    />
                  </div>
                );
              })}
            </div>
            <div className="flex items-center justify-between px-5 py-3 border-t bg-gray-50 rounded-b-2xl">
              <span className="text-xs text-gray-500">
                {t('inventoryTotalLabel')}{' '}
                <span className="font-semibold text-gray-800">
                  {receiptItems[sizePopupIdx].sizeEntries.reduce((sum, se) => sum + (parseInt(se.quantity, 10) || 0), 0)}{t('inventoryTotalProducts')}
                </span>
              </span>
              <button
                type="button"
                onClick={() => setSizePopupIdx(null)}
                className="px-4 py-1.5 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg transition-colors"
              >
                {t('inventoryDoneBtn')}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
