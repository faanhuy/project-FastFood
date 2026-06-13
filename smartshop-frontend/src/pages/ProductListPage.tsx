import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { categoryService, productService } from '../services/productService';
import { catalogService } from '../services/catalogService';
import { cartService } from '../services/cartService';
import { storeService } from '../services/storeService';
import { flashSaleService } from '../services/flashSaleService';
import { useAuthStore } from '../store/authStore';
import { useStoreSelectionStore } from '../store/useStoreSelectionStore';
import type { CategoryDto, PagedResult, ProductDto } from '../types/product';
import type { CatalogItemDto } from '../types/catalog';
import type { FlashSaleItemDto } from '../types/flashSale';
import { FiSearch, FiCpu } from 'react-icons/fi';
import AISearchBar from '../components/AISearchBar';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import { SkeletonProductCard } from '../components/Skeleton';
import WishlistButton from '../components/WishlistButton';
import ComboCard from '../components/ComboCard';
import { FlashSaleBadge } from '../components/FlashSaleBadge';

import { formatPrice } from '../utils/formatters';
import { getImageUrl } from '../utils/imageUrl';

export default function ProductListPage() {
  const { t } = useTranslation('product');
  const { t: tCommon } = useTranslation('common');
  const { t: tToast } = useTranslation('toast');
  const navigate = useNavigate();
  const { isAuthenticated, refreshCartCount } = useAuthStore();
  const { selectedStore } = useStoreSelectionStore();

  const [products, setProducts] = useState<PagedResult<ProductDto> | null>(null);
  const [combos, setCombos] = useState<CatalogItemDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [aiMode, setAiMode] = useState(false);
  const [categoryId, setCategoryId] = useState<string | undefined>(undefined);
  const [sortBy, setSortBy] = useState<number>(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [combosLoading, setCombosLoading] = useState(false);
  const [stockLoading, setStockLoading] = useState(false);
  const [stockByProductId, setStockByProductId] = useState<Record<string, number>>({});
  const [addingId, setAddingId] = useState<string | null>(null);
  const [showComboView, setShowComboView] = useState(false);
  const [comboSearch, setComboSearch] = useState('');
  const [flashSaleMap, setFlashSaleMap] = useState<Record<string, FlashSaleItemDto & { flashSaleName: string; remainingSeconds: number }>>(
    {}
  );

  useEffect(() => {
    flashSaleService
      .getActive(1, 100)
      .then((result) => {
        const map: Record<string, FlashSaleItemDto & { flashSaleName: string; remainingSeconds: number }> = {};
        for (const fs of result.items) {
          for (const item of fs.items) {
            const existing = map[item.productId];
            if (!existing || item.salePrice < existing.salePrice) {
              map[item.productId] = { ...item, flashSaleName: fs.name, remainingSeconds: fs.remainingSeconds };
            }
          }
        }
        setFlashSaleMap(map);
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    categoryService.getCategories().then(setCategories).catch(console.error);
    setCombosLoading(true);
    catalogService
      .getCatalog(1, 50)
      .then((result) => setCombos(result.combos))
      .catch((error) => {
        console.warn('Failed to fetch combos:', error);
        setCombos([]);
      })
      .finally(() => setCombosLoading(false));
  }, []);

  useEffect(() => {
    setLoading(true);
    productService
      .getProducts({ page, pageSize: 12, categoryId, search: search || undefined, sortBy, storeId: selectedStore?.id })
      .then(setProducts)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [page, categoryId, search, sortBy, selectedStore]);

  useEffect(() => {
    const visibleProducts = products?.items ?? [];
    if (!selectedStore || visibleProducts.length === 0) {
      setStockByProductId({});
      return;
    }

    let cancelled = false;
    setStockLoading(true);
    storeService
      .getBulkStock(selectedStore.id, visibleProducts.map((p) => p.id))
      .then((stock) => {
        if (!cancelled) setStockByProductId(stock);
      })
      .catch(() => {
        if (!cancelled) setStockByProductId({});
      })
      .finally(() => {
        if (!cancelled) setStockLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [products, selectedStore]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    setSearch(searchInput.trim());
  };

  const handleCategoryChange = (id: string | undefined) => {
    setCategoryId(id);
    setPage(1);
    setShowComboView(false);
  };

  const handleToggleComboView = () => {
    setShowComboView((v) => !v);
    if (!showComboView) {
      setCategoryId(undefined);
      setComboSearch('');
    }
  };

  const handleSortChange = (value: number) => {
    setSortBy(value);
    setPage(1);
  };

  const handleQuickAdd = async (e: React.MouseEvent, product: ProductDto) => {
    e.preventDefault(); // không navigate vào detail page
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    if (product.hasSizes) {
      toast(tToast('selectSizeFirst'));
      navigate(`/products/${product.slug}`);
      return;
    }
    if (selectedStore && stockByProductId[product.id] === 0) {
      toast.error(tToast('outOfStockAtSelectedBranch'));
      return;
    }
    setAddingId(product.id);
    try {
      await cartService.addToCart(product.id, 1);
      refreshCartCount();
      toast.success(tToast('addedToCart', { name: product.name }));
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors?.[0];
      toast.error(msg ?? tToast('addToCartFailed'));
    } finally {
      setAddingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar>
        <div className="flex items-center gap-2">
          {aiMode ? (
            <AISearchBar onClose={() => setAiMode(false)} />
          ) : (
            <form onSubmit={handleSearch} className="flex-1 relative">
              <input
                className="w-full border border-gray-300 rounded-lg pl-4 pr-10 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-400"
                placeholder={t('product:aiSearchPlaceholder')}
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
              />
              <button
                type="submit"
                className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-rose-600"
              >
                <FiSearch size={18} />
              </button>
            </form>
          )}
          <button
            onClick={() => setAiMode(!aiMode)}
            title={aiMode ? t('product:aiSearchPlaceholder') : t('product:aiSearching')}
            className={`shrink-0 flex items-center gap-1 px-2.5 py-2 rounded-lg text-xs font-medium border transition-colors ${
              aiMode
                ? 'bg-rose-600 text-white border-rose-600'
                : 'bg-white text-rose-600 border-rose-300 hover:bg-rose-50'
            }`}
          >
            <FiCpu size={14} />
            AI
          </button>
        </div>
      </Navbar>

      <div className="max-w-7xl mx-auto px-4 py-6 flex gap-6">
        {/* Sidebar Categories */}
        <aside className="w-48 shrink-0">
          <h3 className="font-semibold text-gray-700 mb-3">{t('categoryHeader')}</h3>
          <ul className="space-y-1">
            <li>
              <button
                onClick={() => handleCategoryChange(undefined)}
                className={`w-full text-left px-3 py-1.5 rounded text-sm ${!categoryId && !showComboView ? 'bg-rose-100 text-rose-700 font-medium' : 'text-gray-600 hover:bg-gray-100'}`}
              >
                {tCommon('all')}
              </button>
            </li>
            {categories.map((cat) => (
              <li key={cat.id}>
                <button
                  onClick={() => handleCategoryChange(cat.id)}
                  className={`w-full text-left px-3 py-1.5 rounded text-sm ${categoryId === cat.id && !showComboView ? 'bg-rose-100 text-rose-700 font-medium' : 'text-gray-600 hover:bg-gray-100'}`}
                >
                  {cat.name}
                </button>
              </li>
            ))}
          </ul>

          {combos.length > 0 && (
            <div className="mt-5">
              <div className="border-t border-gray-100 pt-4">
                <h3 className="font-semibold text-gray-700 mb-2">{t('specialCombos')}</h3>
                <button
                  onClick={handleToggleComboView}
                  className={`w-full text-left px-3 py-1.5 rounded text-sm flex items-center gap-2 transition-colors ${
                    showComboView
                      ? 'bg-amber-100 text-amber-700 font-medium'
                      : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  <span>🎁</span>
                  <span>{t('viewAllCombos', { count: combos.length })}</span>
                </button>
              </div>
            </div>
          )}
        </aside>

        {/* Main */}
        <main className="flex-1">
          <div className="mb-6 rounded-3xl bg-gradient-to-r from-rose-600 via-red-500 to-amber-500 px-6 py-7 text-white shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-rose-100">{t('bannerTag')}</p>
            <h1 className="mt-2 text-2xl font-bold sm:text-3xl">{t('bannerTitle')}</h1>
            <p className="mt-2 max-w-2xl text-sm text-rose-50 sm:text-base">{t('bannerSub')}</p>
          </div>

          {/* Toolbar: result count + sort */}
          <div className="mb-4 flex items-center justify-between gap-4 flex-wrap">
            <span className="text-sm text-gray-500">
              {products && t('showingItems', { current: products.items.length, total: products.totalCount })}
            </span>
            <div className="flex items-center gap-1.5 flex-wrap justify-end">
              <span className="text-sm text-gray-500 hidden sm:inline mr-1">{tCommon('sortBy')}</span>

              {/* Pill: Mới */}
              <button
                onClick={() => handleSortChange(0)}
                className={`px-3 py-1 rounded-full text-xs font-medium border transition-colors
                  ${sortBy === 0
                    ? 'bg-rose-600 text-white border-rose-600'
                    : 'bg-white text-gray-600 border-gray-300 hover:border-rose-400 hover:text-rose-600'}`}
              >
                {t('sortNewest')}
              </button>

              {/* Pill: Bán chạy */}
              <button
                onClick={() => handleSortChange(5)}
                className={`px-3 py-1 rounded-full text-xs font-medium border transition-colors
                  ${sortBy === 5
                    ? 'bg-rose-600 text-white border-rose-600'
                    : 'bg-white text-gray-600 border-gray-300 hover:border-rose-400 hover:text-rose-600'}`}
              >
                {t('sortBestSeller')}
              </button>

              {/* Combo: Giá */}
              <select
                value={[1, 2].includes(sortBy) ? sortBy : ''}
                onChange={(e) => e.target.value !== '' && handleSortChange(Number(e.target.value))}
                className={`px-2.5 py-1 rounded-full text-xs font-medium border transition-colors cursor-pointer
                  ${[1, 2].includes(sortBy)
                    ? 'border-rose-600 text-rose-600 bg-rose-50'
                    : 'border-gray-300 text-gray-600 bg-white hover:border-rose-400 hover:text-rose-600'}`}
              >
                <option value="" disabled>{t('sortPrice')}</option>
                <option value={1}>{t('sortPriceAsc')}</option>
                <option value={2}>{t('sortPriceDesc')}</option>
              </select>

              {/* Combo: Tên */}
              <select
                value={[3, 4].includes(sortBy) ? sortBy : ''}
                onChange={(e) => e.target.value !== '' && handleSortChange(Number(e.target.value))}
                className={`px-2.5 py-1 rounded-full text-xs font-medium border transition-colors cursor-pointer
                  ${[3, 4].includes(sortBy)
                    ? 'border-rose-600 text-rose-600 bg-rose-50'
                    : 'border-gray-300 text-gray-600 bg-white hover:border-rose-400 hover:text-rose-600'}`}
              >
                <option value="" disabled>{t('sortName')}</option>
                <option value={3}>{t('sortNameAZ')}</option>
                <option value={4}>{t('sortNameZA')}</option>
              </select>
            </div>
          </div>

          {/* Combo View Mode */}
          {showComboView ? (
            <div>
              <div className="mb-5 flex items-center gap-3 flex-wrap">
                <h2 className="text-xl font-bold text-gray-900">{t('specialCombos')}</h2>
                <span className="text-sm text-gray-400">({combos.length} combo)</span>
                <div className="ml-auto relative">
                  <input
                    type="text"
                    value={comboSearch}
                    onChange={(e) => setComboSearch(e.target.value)}
                    placeholder={t('searchCombo')}
                    className="border border-gray-300 rounded-lg pl-3 pr-8 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-400 w-52"
                  />
                  {comboSearch && (
                    <button
                      onClick={() => setComboSearch('')}
                      className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 text-xs"
                    >✕</button>
                  )}
                </div>
              </div>
              {combosLoading ? (
                <div className="flex items-center justify-center h-40 text-gray-400">{tCommon('loading')}</div>
              ) : (
                <>
                  {(() => {
                    const filtered = combos.filter((c) =>
                      c.name.toLowerCase().includes(comboSearch.toLowerCase())
                    );
                    return filtered.length > 0 ? (
                      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                        {filtered.map((combo) => (
                          <ComboCard key={combo.id} combo={combo} />
                        ))}
                      </div>
                    ) : (
                      <div className="flex flex-col items-center justify-center h-40 text-gray-400 gap-2">
                        <span className="text-3xl">🎁</span>
                        <p className="text-sm">{t('noComboFound')}</p>
                      </div>
                    );
                  })()}
                </>
              )}
            </div>
          ) : (
            <>
              {/* Combos Preview Section */}
              {!combosLoading && combos.length > 0 && (
                <div className="mb-10">
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-xl font-bold text-gray-900">{t('specialCombos')}</h2>
                    <button
                      onClick={handleToggleComboView}
                      className="text-sm text-amber-600 hover:text-amber-700 font-medium hover:underline"
                    >
                      {t('viewAllCombos', { count: combos.length })} →
                    </button>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                    {combos.slice(0, 4).map((combo) => (
                      <ComboCard key={combo.id} combo={combo} />
                    ))}
                  </div>
                </div>
              )}

              {loading ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                  {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => <SkeletonProductCard key={i} />)}
                </div>
              ) : (
                <>
                  <h2 className="text-xl font-bold text-gray-900 mb-4">{t('allProducts')}</h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                {products?.items.map((product) => {
                  const stock = selectedStore ? stockByProductId[product.id] : null;
                  const outOfStock = stock === 0;
                  const quickAddDisabled = addingId === product.id || outOfStock || stockLoading;
                  const flashSale = flashSaleMap[product.id];
                  const displayPrice = flashSale ? flashSale.salePrice : (product.effectivePrice ?? product.price);
                  const hasDiscount = displayPrice < product.price;

                  return (
                  <Link
                    key={product.id}
                    to={`/products/${product.slug}`}
                    className={`relative bg-white rounded-xl shadow-sm hover:shadow-lg hover:-translate-y-1 hover:border-rose-200 border border-transparent transition-all duration-200 p-3 flex flex-col group cursor-pointer ${
                      outOfStock ? 'opacity-75' : ''
                    } ${flashSale ? 'border-orange-200 ring-1 ring-orange-200' : ''}`}
                  >
                    <div className="absolute top-2 right-2">
                      <WishlistButton productId={product.id} />
                    </div>
                    {outOfStock && (
                      <span className="absolute left-2 top-2 rounded-full bg-gray-900/80 px-2 py-0.5 text-[11px] font-medium text-white">
                        {t('outOfStock')}
                      </span>
                    )}
                    <div className="bg-gray-100 rounded-lg h-36 flex items-center justify-center mb-3 overflow-hidden">
                      {product.imageUrl ? (
                        <img src={getImageUrl(product.imageUrl)} alt={product.name} className="h-full w-full object-contain" />
                      ) : (
                        <span className="text-gray-300 text-4xl">🍔</span>
                      )}
                    </div>
                    <p className="text-sm font-medium text-gray-800 line-clamp-2 flex-1">{product.name}</p>
                    <div className="mt-2">
                      {flashSale ? (
                        <div className="mb-1.5">
                          <FlashSaleBadge
                            item={flashSale}
                            onExpire={() => setFlashSaleMap((prev) => { const n = { ...prev }; delete n[product.id]; return n; })}
                          />
                        </div>
                      ) : null}
                      <div className="flex items-center gap-1.5 flex-wrap">
                        <span className="text-rose-600 font-bold text-sm">{formatPrice(displayPrice)}</span>
                        {hasDiscount && (
                          <span className="rounded-full bg-red-100 text-red-600 px-1.5 py-0.5 text-[10px] font-bold">
                            -{Math.round((1 - displayPrice / product.price) * 100)}%
                          </span>
                        )}
                      </div>
                      {hasDiscount && (
                        <p className="text-gray-400 text-xs line-through">{formatPrice(product.price)}</p>
                      )}
                      {selectedStore && (
                        <p className={`mt-1 text-xs ${outOfStock ? 'text-red-500' : 'text-green-600'}`}>
                          {stockLoading && stock === undefined
                            ? t('checkingStock')
                            : outOfStock
                            ? t('outOfStockAtBranch')
                            : t('stockLeft', { count: stock ?? 0 })}
                        </p>
                      )}
                    </div>
                    <button
                      onClick={(e) => handleQuickAdd(e, product)}
                      disabled={quickAddDisabled}
                      className="mt-2 w-full text-xs bg-rose-600 text-white rounded-lg py-1.5 hover:bg-rose-700 disabled:opacity-50 transition-colors"
                    >
                      {addingId === product.id
                        ? t('adding')
                        : outOfStock
                        ? t('outOfStock')
                        : product.hasSizes
                        ? t('selectSize')
                        : t('addItem')}
                    </button>
                  </Link>
                  );
                })}
              </div>

              {/* Pagination */}
              {products && products.totalPages > 1 && (
                <div className="mt-8 flex justify-center gap-2">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
                  >
                    {tCommon('prev')}
                  </button>
                  {Array.from({ length: products.totalPages }, (_, i) => i + 1).map((p) => (
                    <button
                      key={p}
                      onClick={() => setPage(p)}
                      className={`px-3 py-1.5 rounded border text-sm ${p === page ? 'bg-rose-600 text-white border-rose-600' : 'hover:bg-gray-100'}`}
                    >
                      {p}
                    </button>
                  ))}
                  <button
                    onClick={() => setPage((p) => Math.min(products.totalPages, p + 1))}
                    disabled={page === products.totalPages}
                    className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
                  >
                    {tCommon('next')}
                  </button>
                </div>
              )}
            </>
          )}
            </>
          )}
        </main>
      </div>
      <Footer />
    </div>
  );
}
