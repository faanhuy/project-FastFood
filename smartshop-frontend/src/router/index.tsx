import { lazy, Suspense } from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../store/authStore';

// User-facing pages
const LoginPage           = lazy(() => import('../pages/LoginPage'));
const RegisterPage        = lazy(() => import('../pages/RegisterPage'));
const ProductListPage     = lazy(() => import('../pages/ProductListPage'));
const ProductDetailPage   = lazy(() => import('../pages/ProductDetailPage'));
const ComboDetailPage     = lazy(() => import('../pages/ComboDetailPage'));
const FlashSalePage       = lazy(() => import('../pages/FlashSalePage'));
const CartPage            = lazy(() => import('../pages/CartPage'));
const CheckoutPage        = lazy(() => import('../pages/CheckoutPage'));
const OrderHistoryPage    = lazy(() => import('../pages/OrderHistoryPage'));
const OrderDetailPage     = lazy(() => import('../pages/OrderDetailPage'));
const ReturnRequestsPage  = lazy(() => import('../pages/ReturnRequestsPage'));
const ProfilePage         = lazy(() => import('../pages/ProfilePage'));
const WishlistPage        = lazy(() => import('../pages/WishlistPage'));
const PaymentResultPage   = lazy(() => import('../pages/PaymentResultPage'));

// Admin pages (separate chunk — never loaded by regular users)
const AdminDashboardPage       = lazy(() => import('../pages/admin/AdminDashboardPage'));
const AdminProductPage         = lazy(() => import('../pages/admin/AdminProductPage'));
const AdminOrderPage           = lazy(() => import('../pages/admin/AdminOrderPage'));
const AdminCouponsPage         = lazy(() => import('../pages/admin/AdminCouponsPage'));
const AdminFlashSalesPage      = lazy(() => import('../pages/admin/AdminFlashSalesPage'));
const InventoryManagementPage  = lazy(() => import('../pages/admin/InventoryManagementPage'));
const AdminStoresPage          = lazy(() => import('../pages/admin/AdminStoresPage'));
const AdminPromotionalPricePage = lazy(() => import('../pages/admin/AdminPromotionalPricePage'));
const SizeManagementPage       = lazy(() => import('../pages/admin/SizeManagementPage'));
const AdminComboPage           = lazy(() => import('../pages/admin/AdminComboPage'));
const AdminReturnRequestsPage  = lazy(() => import('../pages/admin/AdminReturnRequestsPage'));
const AdminUsersPage           = lazy(() => import('../pages/admin/AdminUsersPage'));
const AdminCsvImportPage       = lazy(() => import('../pages/admin/AdminCsvImportPage'));
const AdminHealthPage          = lazy(() => import('../pages/admin/AdminHealthPage'));

function PageLoader() {
  const { t } = useTranslation('common');
  return (
    <div className="min-h-screen flex items-center justify-center text-gray-400 text-sm">
      {t('loading')}
    </div>
  );
}

function withSuspense(element: React.ReactElement) {
  return <Suspense fallback={<PageLoader />}>{element}</Suspense>;
}

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function AdminRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, user } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (user?.role !== 'Admin') return <Navigate to="/products" replace />;
  return <>{children}</>;
}

const router = createBrowserRouter([
  { path: '/login',    element: withSuspense(<LoginPage />) },
  { path: '/register', element: withSuspense(<RegisterPage />) },
  { path: '/products', element: withSuspense(<ProductListPage />) },
  { path: '/products/:slug', element: withSuspense(<ProductDetailPage />) },
  { path: '/combos/:id',     element: withSuspense(<ComboDetailPage />) },
  { path: '/flash-sales',    element: withSuspense(<FlashSalePage />) },
  {
    path: '/cart',
    element: withSuspense(
      <PrivateRoute><CartPage /></PrivateRoute>
    ),
  },
  {
    path: '/checkout',
    element: withSuspense(
      <PrivateRoute><CheckoutPage /></PrivateRoute>
    ),
  },
  {
    path: '/orders',
    element: withSuspense(
      <PrivateRoute><OrderHistoryPage /></PrivateRoute>
    ),
  },
  {
    path: '/orders/return-requests',
    element: withSuspense(
      <PrivateRoute><ReturnRequestsPage /></PrivateRoute>
    ),
  },
  {
    path: '/orders/:id',
    element: withSuspense(
      <PrivateRoute><OrderDetailPage /></PrivateRoute>
    ),
  },
  {
    path: '/admin',
    element: withSuspense(
      <AdminRoute><AdminDashboardPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/products',
    element: withSuspense(
      <AdminRoute><AdminProductPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/orders',
    element: withSuspense(
      <AdminRoute><AdminOrderPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/coupons',
    element: withSuspense(
      <AdminRoute><AdminCouponsPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/flash-sales',
    element: withSuspense(
      <AdminRoute><AdminFlashSalesPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/inventory',
    element: withSuspense(
      <AdminRoute><InventoryManagementPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/stores',
    element: withSuspense(
      <AdminRoute><AdminStoresPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/promotional-prices',
    element: withSuspense(
      <AdminRoute><AdminPromotionalPricePage /></AdminRoute>
    ),
  },
  {
    path: '/admin/sizes',
    element: withSuspense(
      <AdminRoute><SizeManagementPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/combos',
    element: withSuspense(
      <AdminRoute><AdminComboPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/return-requests',
    element: withSuspense(
      <AdminRoute><AdminReturnRequestsPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/users',
    element: withSuspense(
      <AdminRoute><AdminUsersPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/products/import',
    element: withSuspense(
      <AdminRoute><AdminCsvImportPage /></AdminRoute>
    ),
  },
  {
    path: '/admin/health',
    element: withSuspense(
      <AdminRoute><AdminHealthPage /></AdminRoute>
    ),
  },
  {
    path: '/profile',
    element: withSuspense(
      <PrivateRoute><ProfilePage /></PrivateRoute>
    ),
  },
  {
    path: '/wishlist',
    element: withSuspense(
      <PrivateRoute><WishlistPage /></PrivateRoute>
    ),
  },
  { path: '/payment/result', element: withSuspense(<PaymentResultPage />) },
  { path: '/', element: <Navigate to="/products" replace /> },
  { path: '*', element: <Navigate to="/products" replace /> },
]);

export default router;
