import { createBrowserRouter, Navigate } from 'react-router-dom';
import LoginPage from '../pages/LoginPage';
import RegisterPage from '../pages/RegisterPage';
import ProductListPage from '../pages/ProductListPage';
import ProductDetailPage from '../pages/ProductDetailPage';
import AdminProductPage from '../pages/AdminProductPage';
import CartPage from '../pages/CartPage';
import CheckoutPage from '../pages/CheckoutPage';
import OrderHistoryPage from '../pages/OrderHistoryPage';
import OrderDetailPage from '../pages/OrderDetailPage';
import { useAuthStore } from '../store/authStore';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },
  { path: '/products', element: <ProductListPage /> },
  { path: '/products/:slug', element: <ProductDetailPage /> },
  {
    path: '/cart',
    element: (
      <PrivateRoute>
        <CartPage />
      </PrivateRoute>
    ),
  },
  {
    path: '/checkout',
    element: (
      <PrivateRoute>
        <CheckoutPage />
      </PrivateRoute>
    ),
  },
  {
    path: '/orders',
    element: (
      <PrivateRoute>
        <OrderHistoryPage />
      </PrivateRoute>
    ),
  },
  {
    path: '/orders/:id',
    element: (
      <PrivateRoute>
        <OrderDetailPage />
      </PrivateRoute>
    ),
  },
  {
    path: '/admin/products',
    element: (
      <PrivateRoute>
        <AdminProductPage />
      </PrivateRoute>
    ),
  },
  { path: '/', element: <Navigate to="/products" replace /> },
  { path: '*', element: <Navigate to="/products" replace /> },
]);

export default router;
