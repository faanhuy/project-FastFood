import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { categoryService, productService } from '../services/productService';
import { flashSaleService } from '../services/flashSaleService';
import type { CategoryDto, PagedResult, ProductDto } from '../types/product';
import type { FlashSaleDto } from '../types/flashSale';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import { SkeletonProductCard } from '../components/Skeleton';
import { FlashSaleBadge } from '../components/FlashSaleBadge';
import WishlistButton from '../components/WishlistButton';
import { formatPrice } from '../utils/formatters';
import { getImageUrl } from '../utils/imageUrl';
import { useFlashSaleStore } from '../store/useFlashSaleStore';

const CATEGORY_PALETTES = [
  { bg: '#fff1f2', icon: '#ffe4e6', text: '#be123c' },
  { bg: '#fff7ed', icon: '#ffedd5', text: '#c2410c' },
  { bg: '#fefce8', icon: '#fef9c3', text: '#a16207' },
  { bg: '#f0fdf4', icon: '#dcfce7', text: '#15803d' },
  { bg: '#eff6ff', icon: '#dbeafe', text: '#1d4ed8' },
  { bg: '#faf5ff', icon: '#f3e8ff', text: '#7e22ce' },
  { bg: '#fdf2f8', icon: '#fce7f3', text: '#be185d' },
  { bg: '#f0fdfa', icon: '#ccfbf1', text: '#0f766e' },
];

const EMOJI_MAP: [RegExp, string][] = [
  [/burger|hamburger|bò|beef/i, '🍔'],
  [/pizza/i, '🍕'],
  [/gà|chicken|fried/i, '🍗'],
  [/mì|noodle|pasta|phở|bún/i, '🍜'],
  [/cơm|rice|fried rice/i, '🍚'],
  [/uống|drink|juice|tea|coffee|cà phê|nước/i, '🥤'],
  [/bánh|cake|dessert|tráng miệng|sweet/i, '🍰'],
  [/salad|rau/i, '🥗'],
  [/hải sản|seafood|tôm|cua|fish|cá/i, '🦐'],
  [/heo|pork|sườn/i, '🥩'],
  [/sandwich|bánh mì|sub/i, '🥪'],
  [/soup|canh|lẩu|hot pot/i, '🍲'],
  [/sushi|japan|nhật/i, '🍱'],
  [/snack|ăn vặt|chips/i, '🍟'],
  [/ice cream|kem/i, '🍦'],
  [/fruit|trái cây/i, '🍓'],
];

function getCategoryEmoji(name: string): string {
  for (const [pattern, emoji] of EMOJI_MAP) {
    if (pattern.test(name)) return emoji;
  }
  return '🍽️';
}

export default function HomePage() {
  const { t } = useTranslation('common');
  const navigate = useNavigate();
  const { flashSaleMap } = useFlashSaleStore();

  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [featuredProducts, setFeaturedProducts] = useState<PagedResult<ProductDto> | null>(null);
  const [flashSales, setFlashSales] = useState<FlashSaleDto[]>([]);
  const [categoriesLoading, setCategoriesLoading] = useState(false);
  const [productsLoading, setProductsLoading] = useState(false);
  const [flashSalesLoading, setFlashSalesLoading] = useState(false);

  useEffect(() => {
    setCategoriesLoading(true);
    categoryService
      .getCategories()
      .then(setCategories)
      .catch(() => setCategories([]))
      .finally(() => setCategoriesLoading(false));
  }, []);

  useEffect(() => {
    setProductsLoading(true);
    productService
      .getProducts({ page: 1, pageSize: 8, sortBy: 0 })
      .then(setFeaturedProducts)
      .catch(() => setFeaturedProducts(null))
      .finally(() => setProductsLoading(false));
  }, []);

  useEffect(() => {
    setFlashSalesLoading(true);
    flashSaleService
      .getActive(1, 4)
      .then((result) => setFlashSales(result.items))
      .catch(() => setFlashSales([]))
      .finally(() => setFlashSalesLoading(false));
  }, []);

  const handleProductClick = (productSlug: string) => {
    navigate(`/products/${productSlug}`);
  };

  const handleCategoryClick = (categorySlug: string) => {
    navigate(`/products?categoryId=${categorySlug}`);
  };

  return (
    <div className="min-h-screen flex flex-col bg-white">
      <Navbar />

      <main className="flex-1">
        {/* Hero Section */}
        <section className="bg-gradient-to-br from-rose-600 via-rose-500 to-amber-500 text-white py-20 px-4 sm:py-24">
          <div className="max-w-4xl mx-auto text-center">
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold mb-4">
              {t('heroTitle')}
            </h1>
            <p className="text-lg md:text-xl opacity-90 mb-8 max-w-2xl mx-auto">
              {t('heroSubtitle')}
            </p>
            <button
              onClick={() => navigate('/products')}
              className="bg-white text-rose-600 font-semibold px-8 py-3 rounded-full hover:bg-rose-50 transition transform hover:scale-105 active:scale-95"
            >
              {t('heroCta')}
            </button>
          </div>
        </section>

        {/* Features Section */}
        <section className="py-16 px-4 bg-gray-50">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-3xl font-bold text-center mb-12 text-gray-900">
              {t('whyChooseUs')}
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {/* Feature 1 */}
              <div className="bg-white rounded-2xl p-8 text-center shadow-sm border border-gray-100 hover:shadow-md transition">
                <div className="text-4xl mb-4">⚡</div>
                <h3 className="text-xl font-bold mb-2 text-gray-900">
                  {t('feature1Title')}
                </h3>
                <p className="text-gray-600">{t('feature1Desc')}</p>
              </div>

              {/* Feature 2 */}
              <div className="bg-white rounded-2xl p-8 text-center shadow-sm border border-gray-100 hover:shadow-md transition">
                <div className="text-4xl mb-4">✨</div>
                <h3 className="text-xl font-bold mb-2 text-gray-900">
                  {t('feature2Title')}
                </h3>
                <p className="text-gray-600">{t('feature2Desc')}</p>
              </div>

              {/* Feature 3 */}
              <div className="bg-white rounded-2xl p-8 text-center shadow-sm border border-gray-100 hover:shadow-md transition">
                <div className="text-4xl mb-4">🎉</div>
                <h3 className="text-xl font-bold mb-2 text-gray-900">
                  {t('feature3Title')}
                </h3>
                <p className="text-gray-600">{t('feature3Desc')}</p>
              </div>
            </div>
          </div>
        </section>

        {/* Categories Section */}
        <section className="py-16 px-4">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-3xl font-bold mb-8 text-gray-900">
              {t('categoriesSection')}
            </h2>
            {categoriesLoading ? (
              <div className="grid grid-cols-3 sm:grid-cols-4 lg:grid-cols-6 gap-3">
                {[...Array(6)].map((_, i) => (
                  <div key={i} className="h-28 bg-gray-200 rounded-2xl animate-pulse" />
                ))}
              </div>
            ) : categories.length > 0 ? (
              <div className="grid grid-cols-3 sm:grid-cols-4 lg:grid-cols-6 gap-3">
                {categories.slice(0, 12).map((category, idx) => {
                  const emoji = getCategoryEmoji(category.name);
                  const palette = CATEGORY_PALETTES[idx % CATEGORY_PALETTES.length];
                  return (
                    <button
                      key={category.id}
                      onClick={() => handleCategoryClick(category.id)}
                      className="group flex flex-col items-center gap-2.5 py-5 px-2 rounded-2xl border border-gray-100 shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all duration-200"
                      style={{ background: palette.bg }}
                    >
                      <span
                        className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl shadow-sm group-hover:scale-110 transition-transform duration-200"
                        style={{ background: palette.icon }}
                      >
                        {emoji}
                      </span>
                      <span className="text-xs font-semibold text-center leading-tight line-clamp-2" style={{ color: palette.text }}>
                        {category.name}
                      </span>
                    </button>
                  );
                })}
              </div>
            ) : (
              <p className="text-gray-500 text-center py-8">{t('noData')}</p>
            )}
          </div>
        </section>

        {/* Featured Products Section */}
        <section className="py-16 px-4 bg-gray-50">
          <div className="max-w-6xl mx-auto">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-3xl font-bold text-gray-900">
                {t('featuredProducts')}
              </h2>
              <button
                onClick={() => navigate('/products')}
                className="text-rose-600 hover:text-rose-700 font-medium text-sm transition"
              >
                {t('viewAll')} →
              </button>
            </div>

            {productsLoading ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {[...Array(8)].map((_, i) => (
                  <SkeletonProductCard key={i} />
                ))}
              </div>
            ) : featuredProducts && featuredProducts.items.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {featuredProducts.items.map((product) => {
                  const isInFlashSale = flashSaleMap[product.id];
                  return (
                    <div
                      key={product.id}
                      className="bg-white rounded-2xl overflow-hidden shadow-sm border border-gray-100 hover:shadow-md hover:border-rose-200 transition group"
                    >
                      {/* Image */}
                      <div className="relative aspect-square overflow-hidden bg-gray-100">
                        <img
                          src={getImageUrl(product.imageUrl)}
                          alt={product.name}
                          className="w-full h-full object-cover group-hover:scale-105 transition cursor-pointer"
                          onClick={() => handleProductClick(product.slug)}
                        />
                        {isInFlashSale && (
                          <div className="absolute top-3 right-3">
                            <FlashSaleBadge item={isInFlashSale} />
                          </div>
                        )}
                        <button
                          className="absolute top-3 left-3 opacity-0 group-hover:opacity-100 transition"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <WishlistButton productId={product.id} />
                        </button>
                      </div>

                      {/* Content */}
                      <div className="p-4">
                        <h3
                          className="font-semibold text-gray-900 line-clamp-2 mb-2 cursor-pointer hover:text-rose-600 transition"
                          onClick={() => handleProductClick(product.slug)}
                        >
                          {product.name}
                        </h3>
                        <div className="flex items-center gap-2 mb-3">
                          <span className="text-lg font-bold text-rose-600">
                            {formatPrice(product.price)}
                          </span>
                          {product.originalPrice > product.price && (
                            <span className="text-sm text-gray-500 line-through">
                              {formatPrice(product.originalPrice)}
                            </span>
                          )}
                        </div>
                        <button
                          onClick={() => handleProductClick(product.slug)}
                          className="w-full bg-rose-600 text-white font-medium py-2 rounded-full hover:bg-rose-700 transition active:scale-95"
                        >
                          {t('selectNow')}
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className="text-gray-500 text-center py-8">{t('noData')}</p>
            )}
          </div>
        </section>

        {/* Flash Sales Section */}
        {flashSalesLoading || flashSales.length > 0 ? (
          <section className="py-16 px-4">
            <div className="max-w-6xl mx-auto">
              <div className="flex items-center justify-between mb-8">
                <h2 className="text-3xl font-bold text-gray-900">
                  {t('flashSalesSection')}
                </h2>
                <button
                  onClick={() => navigate('/flash-sales')}
                  className="text-rose-600 hover:text-rose-700 font-medium text-sm transition"
                >
                  {t('viewAll')} →
                </button>
              </div>

              {flashSalesLoading ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                  {[...Array(4)].map((_, i) => (
                    <div
                      key={i}
                      className="bg-gray-200 rounded-2xl aspect-square animate-pulse"
                    />
                  ))}
                </div>
              ) : flashSales.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                  {flashSales.map((flashSale) => (
                    <div
                      key={flashSale.id}
                      onClick={() => navigate('/flash-sales')}
                      className="bg-white rounded-2xl overflow-hidden shadow-sm border border-gray-100 hover:shadow-md hover:border-rose-200 transition cursor-pointer group"
                    >
                      <div className="bg-gradient-to-br from-rose-600 to-amber-500 p-6 text-white text-center">
                        <h3 className="font-bold text-lg mb-2 group-hover:scale-105 transition">
                          {flashSale.name}
                        </h3>
                        <p className="text-sm opacity-90 mb-3">
                          {flashSale.items.length} {t('common:itemsOnSale', {
                            defaultValue: 'items on sale',
                          })}
                        </p>
                        <div className="text-2xl font-bold">
                          {flashSale.remainingSeconds > 0
                            ? `${Math.floor(
                                flashSale.remainingSeconds / 3600
                              )}h ${Math.floor(
                                (flashSale.remainingSeconds % 3600) / 60
                              )}m`
                            : t('common:ended', { defaultValue: 'Ended' })}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : null}
            </div>
          </section>
        ) : null}
      </main>

      <Footer />
    </div>
  );
}
