import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiPackage, FiShoppingBag, FiClock, FiArrowRight } from 'react-icons/fi';
import AdminLayout from '../components/AdminLayout';
import { productService } from '../services/productService';
import { orderService } from '../services/orderService';
import { resolveOrderStatus } from '../types/order';

interface Stats {
  totalProducts: number;
  totalOrders: number;
  pendingOrders: number;
}

export default function AdminDashboardPage() {
  const [stats, setStats] = useState<Stats | null>(null);

  useEffect(() => {
    Promise.all([
      productService.getProducts({ page: 1, pageSize: 1 }),
      orderService.getAllOrders(1, 50),
    ])
      .then(([products, orders]) => {
        const pending = orders.items.filter(
          (o) => resolveOrderStatus(o.status) === 1
        ).length;
        setStats({
          totalProducts: products.totalCount,
          totalOrders: orders.totalCount,
          pendingOrders: pending,
        });
      })
      .catch(() => {});
  }, []);

  const statCards = [
    {
      label: 'Sản phẩm',
      value: stats?.totalProducts,
      icon: FiPackage,
      to: '/admin/products',
      iconCls: 'bg-blue-50 text-blue-600 border-blue-100',
    },
    {
      label: 'Đơn hàng',
      value: stats?.totalOrders,
      icon: FiShoppingBag,
      to: '/admin/orders',
      iconCls: 'bg-purple-50 text-purple-600 border-purple-100',
    },
    {
      label: 'Chờ xác nhận',
      value: stats?.pendingOrders,
      icon: FiClock,
      to: '/admin/orders',
      iconCls: 'bg-yellow-50 text-yellow-600 border-yellow-100',
    },
  ];

  return (
    <AdminLayout title="Tổng quan">
      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        {statCards.map((card) => (
          <Link
            key={card.label}
            to={card.to}
            className="bg-white rounded-xl border shadow-sm p-5 flex items-center gap-4 hover:shadow-md transition-shadow"
          >
            <div className={`p-3 rounded-xl border ${card.iconCls}`}>
              <card.icon size={22} />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-800">
                {card.value != null ? card.value : '—'}
              </p>
              <p className="text-sm text-gray-500">{card.label}</p>
            </div>
          </Link>
        ))}
      </div>

      {/* Quick nav cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Link
          to="/admin/products"
          className="group bg-white rounded-xl border shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center justify-between mb-2">
            <h3 className="font-semibold text-gray-800">Quản lý sản phẩm</h3>
            <FiArrowRight
              size={16}
              className="text-gray-400 group-hover:text-blue-600 transition-colors"
            />
          </div>
          <p className="text-sm text-gray-500">
            Thêm, chỉnh sửa, xóa sản phẩm. Quản lý tồn kho và trạng thái hiển thị.
          </p>
        </Link>

        <Link
          to="/admin/orders"
          className="group bg-white rounded-xl border shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center justify-between mb-2">
            <h3 className="font-semibold text-gray-800">Quản lý đơn hàng</h3>
            <FiArrowRight
              size={16}
              className="text-gray-400 group-hover:text-blue-600 transition-colors"
            />
          </div>
          <p className="text-sm text-gray-500">
            Xem tất cả đơn hàng, cập nhật trạng thái. Lọc theo tiến trình xử lý.
          </p>
        </Link>
      </div>
    </AdminLayout>
  );
}
