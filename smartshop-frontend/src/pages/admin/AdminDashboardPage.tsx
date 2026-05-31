import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import {
  FiPackage,
  FiShoppingBag,
  FiClock,
  FiArrowRight,
  FiTrendingUp,
  FiTrendingDown,
  FiUsers,
  FiDollarSign,
  FiAlertTriangle,
} from 'react-icons/fi';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import AdminLayout from '../../components/AdminLayout';
import { productService } from '../../services/productService';
import { orderService } from '../../services/orderService';
import { storeService } from '../../services/storeService';
import { resolveOrderStatus } from '../../types/order';
import {
  analyticsService,
  type RevenueSummaryDto,
  type RevenueByDateDto,
  type TopProductDto,
  type OrderStatusBreakdownDto,
} from '../../services/analyticsService';

// ─── Types ───────────────────────────────────────────────────────────────────

interface Stats {
  totalProducts: number;
  totalOrders: number;
  pendingOrders: number;
}

type PeriodDays = 7 | 30 | 90;

// ─── Constants ───────────────────────────────────────────────────────────────

// STATUS_LABELS thay thế bằng getStatusLabel function dùng i18next

const STATUS_COLORS: Record<string, string> = {
  Pending: '#f59e0b',
  Confirmed: '#3b82f6',
  Shipped: '#8b5cf6',
  Delivered: '#10b981',
  Cancelled: '#ef4444',
};

// PERIOD_OPTIONS thay thế dùng translation keys
const getPeriodOptions = (t: any) => [
  { label: t('period7d'), days: 7 as PeriodDays },
  { label: t('period30d'), days: 30 as PeriodDays },
  { label: t('period90d'), days: 90 as PeriodDays },
];

// ─── Helpers ─────────────────────────────────────────────────────────────────

const vnd = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

function formatVnd(value: number): string {
  return vnd.format(value);
}

function formatVndShort(value: number): string {
  if (value >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(1)}B`;
  if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
  if (value >= 1_000) return `${(value / 1_000).toFixed(0)}K`;
  return String(value);
}

function getDateRange(days: PeriodDays): { from: string; to: string } {
  const to = new Date();
  const from = new Date();
  from.setDate(from.getDate() - (days - 1));
  const fmt = (d: Date) => d.toISOString().slice(0, 10);
  return { from: fmt(from), to: fmt(to) };
}

function formatXAxisDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}/${d.getMonth() + 1}`;
}

// ─── Sub-components (inline per spec) ────────────────────────────────────────

function SkeletonCard() {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-5 animate-pulse">
      <div className="h-4 bg-gray-200 rounded w-1/2 mb-3" />
      <div className="h-7 bg-gray-200 rounded w-3/4" />
    </div>
  );
}

function SkeletonChart({ height = 280 }: { height?: number }) {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-5 animate-pulse">
      <div className="h-4 bg-gray-200 rounded w-1/3 mb-4" />
      <div className="bg-gray-100 rounded" style={{ height }} />
    </div>
  );
}

interface KpiCardProps {
  label: string;
  value: string;
  icon: React.ElementType;
  iconCls: string;
  growth?: number;
}

interface KpiCardComponentProps extends Omit<KpiCardProps, 'label'> {
  label: string;
}

function KpiCard({ label, value, icon: Icon, iconCls, growth }: KpiCardComponentProps) {
  const { t } = useTranslation('admin');
  const isPositive = growth !== undefined && growth >= 0;
  return (
    <div className="bg-white rounded-xl border shadow-sm p-5 flex items-start gap-4">
      <div className={`p-3 rounded-xl border shrink-0 ${iconCls}`}>
        <Icon size={22} />
      </div>
      <div className="min-w-0">
        <p className="text-2xl font-bold text-gray-800 truncate">{value}</p>
        <p className="text-sm text-gray-500">{label}</p>
        {growth !== undefined && (
          <p
            className={`text-xs font-medium mt-1 flex items-center gap-1 ${
              isPositive ? 'text-emerald-600' : 'text-red-500'
            }`}
          >
            {isPositive ? <FiTrendingUp size={12} /> : <FiTrendingDown size={12} />}
            {isPositive ? '+' : ''}
            {growth.toFixed(1)}% {t('growthVsPrev')}
          </p>
        )}
      </div>
    </div>
  );
}

// ─── Custom Tooltip for LineChart ─────────────────────────────────────────────

function RevenueTooltip({
  active,
  payload,
  label,
}: {
  active?: boolean;
  payload?: { value: number }[];
  label?: string;
}) {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white border rounded-lg shadow-md px-3 py-2 text-sm">
      <p className="text-gray-500 mb-1">{label}</p>
      <p className="font-semibold text-rose-600">{formatVnd(payload[0].value)}</p>
      {payload[1] && (
        <p className="text-gray-600">{t('orderCount', { count: payload[1].value })}</p>
      )}
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function AdminDashboardPage() {
  const { t } = useTranslation(['admin', 'common']);
  const { t: tToast } = useTranslation('toast');
  const [stats, setStats] = useState<Stats | null>(null);
  const [period, setPeriod] = useState<PeriodDays>(30);
  const [lowStockCount, setLowStockCount] = useState<number | null>(null);

  const [summary, setSummary] = useState<RevenueSummaryDto | null>(null);
  const [revenueData, setRevenueData] = useState<RevenueByDateDto[]>([]);
  const [topProducts, setTopProducts] = useState<TopProductDto[]>([]);
  const [orderStatus, setOrderStatus] = useState<OrderStatusBreakdownDto[]>([]);

  const [analyticsLoading, setAnalyticsLoading] = useState(true);
  const [exporting, setExporting] = useState<'csv' | 'pdf' | null>(null);

  // Static stats (products / total orders / pending)
  useEffect(() => {
    Promise.all([
      productService.getProducts({ page: 1, pageSize: 1 }),
      orderService.getAllOrders(1, 50),
    ])
      .then(([products, orders]) => {
        const pending = orders.items.filter(
          (o) => resolveOrderStatus(o.status) === 1,
        ).length;
        setStats({
          totalProducts: products.totalCount,
          totalOrders: orders.totalCount,
          pendingOrders: pending,
        });
      })
      .catch(() => {});

    // Load low stock count from the first store
    storeService
      .getStores()
      .then((stores) => {
        if (stores.length === 0) return;
        return storeService.getLowStockProducts(stores[0].id);
      })
      .then((items) => {
        if (items) setLowStockCount(items.length);
      })
      .catch(() => {});
  }, []);

  // Analytics fetch — re-runs whenever period changes
  const fetchAnalytics = useCallback(async (days: PeriodDays) => {
    setAnalyticsLoading(true);
    const { from, to } = getDateRange(days);
    try {
      const [summaryRes, revenueRes, topRes, statusRes] = await Promise.all([
        analyticsService.getSummary(from, to),
        analyticsService.getRevenueByDate(from, to),
        analyticsService.getTopProducts(from, to, 5),
        analyticsService.getOrderStatusBreakdown(),
      ]);
      setSummary(summaryRes);
      setRevenueData(revenueRes);
      setTopProducts(topRes);
      setOrderStatus(statusRes);
    } catch {
      // silently ignore — keep previous data or empty state
    } finally {
      setAnalyticsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAnalytics(period);
  }, [period, fetchAnalytics]);

  const handleExportCsv = async () => {
    setExporting('csv');
    try {
      const { from, to } = getDateRange(period);
      await analyticsService.exportCsv(new Date(from), new Date(to));
      toast.success(tToast('exportSuccess'));
    } catch {
      toast.error(tToast('exportFailed'));
    } finally {
      setExporting(null);
    }
  };

  const handleExportPdf = async () => {
    setExporting('pdf');
    try {
      const { from, to } = getDateRange(period);
      await analyticsService.exportPdf(new Date(from), new Date(to));
      toast.success(tToast('exportSuccess'));
    } catch {
      toast.error(tToast('exportFailed'));
    } finally {
      setExporting(null);
    }
  };

  // ── Derived data ────────────────────────────────────────────────────────────

  const getStatusLabel = (status: string) => t(`status_${status}`, { ns: 'order' });

  const statCards = [
    {
      label: t('statFood'),
      value: stats?.totalProducts,
      icon: FiPackage,
      to: '/admin/products',
      iconCls: 'bg-rose-50 text-rose-600 border-rose-100',
    },
    {
      label: t('statOrders'),
      value: stats?.totalOrders,
      icon: FiShoppingBag,
      to: '/admin/orders',
      iconCls: 'bg-purple-50 text-purple-600 border-purple-100',
    },
    {
      label: t('statPending'),
      value: stats?.pendingOrders,
      icon: FiClock,
      to: '/admin/orders',
      iconCls: 'bg-yellow-50 text-yellow-600 border-yellow-100',
    },
  ];

  const pieData = orderStatus.map((s) => ({
    name: getStatusLabel(s.status),
    value: s.count,
    color: STATUS_COLORS[s.status] ?? '#94a3b8',
  }));

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <AdminLayout title={t('overview')}>

      {/* ── Static stat cards ──────────────────────────────────────────────── */}
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-4 mb-6">
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

        {/* Low Stock Alert widget */}
        <Link
          to="/admin/inventory"
          className="bg-white rounded-xl border shadow-sm p-5 flex items-center gap-4 hover:shadow-md transition-shadow"
        >
          <div className={`p-3 rounded-xl border ${lowStockCount && lowStockCount > 0 ? 'bg-red-50 text-red-600 border-red-100' : 'bg-gray-50 text-gray-400 border-gray-100'}`}>
            <FiAlertTriangle size={22} />
          </div>
          <div>
            <p className={`text-2xl font-bold ${lowStockCount && lowStockCount > 0 ? 'text-red-600' : 'text-gray-800'}`}>
              {lowStockCount != null ? lowStockCount : '—'}
            </p>
            <p className="text-sm text-gray-500">{t('statLowStock')}</p>
          </div>
        </Link>
      </div>

      {/* ── Period selector + Export buttons ────────────────────────────────── */}
      <div className="flex items-center gap-2 mb-5 flex-wrap">
        <span className="text-sm text-gray-500 mr-1">{t('periodLabel')}</span>
        {getPeriodOptions(t).map(({ label, days }) => (
          <button
            key={days}
            onClick={() => setPeriod(days)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
              period === days
                ? 'bg-rose-600 text-white border-rose-600'
                : 'bg-white text-gray-600 border-gray-200 hover:border-rose-400 hover:text-rose-600'
            }`}
          >
            {label}
          </button>
        ))}
        <div className="flex-1" />
        <button
          onClick={handleExportCsv}
          disabled={exporting !== null}
          className="px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border bg-white text-gray-600 border-gray-200 hover:border-blue-400 hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          {exporting === 'csv' ? t('exportingCsv') : t('exportCsv')}
        </button>
        <button
          onClick={handleExportPdf}
          disabled={exporting !== null}
          className="px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border bg-white text-gray-600 border-gray-200 hover:border-red-400 hover:text-red-600 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          {exporting === 'pdf' ? t('exportingPdf') : t('exportPdf')}
        </button>
      </div>

      {/* ── KPI cards ─────────────────────────────────────────────────────── */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        {analyticsLoading ? (
          Array.from({ length: 4 }).map((_, i) => <SkeletonCard key={i} />)
        ) : (
          <>
            <KpiCard
              label={t('kpiRevenue')}
              value={summary ? formatVnd(summary.totalRevenue) : '—'}
              icon={FiDollarSign}
              iconCls="bg-emerald-50 text-emerald-600 border-emerald-100"
              growth={summary?.revenueGrowthPercent}
            />
            <KpiCard
              label={t('kpiOrderCount')}
              value={summary ? String(summary.totalOrders) : '—'}
              icon={FiShoppingBag}
              iconCls="bg-rose-50 text-rose-600 border-rose-100"
            />
            <KpiCard
              label={t('kpiNewCustomers')}
              value={summary ? String(summary.newCustomers) : '—'}
              icon={FiUsers}
              iconCls="bg-violet-50 text-violet-600 border-violet-100"
            />
            <KpiCard
              label={t('kpiAvgOrder')}
              value={summary ? formatVnd(summary.averageOrderValue) : '—'}
              icon={FiTrendingUp}
              iconCls="bg-orange-50 text-orange-600 border-orange-100"
            />
          </>
        )}
      </div>

      {/* ── Revenue line chart ─────────────────────────────────────────────── */}
      {analyticsLoading ? (
        <SkeletonChart height={280} />
      ) : (
        <div className="bg-white rounded-xl border shadow-sm p-5 mb-6">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">{t('chartRevByDay')}</h2>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={revenueData} margin={{ top: 4, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis
                dataKey="date"
                tickFormatter={formatXAxisDate}
                tick={{ fontSize: 11, fill: '#9ca3af' }}
                axisLine={false}
                tickLine={false}
              />
              <YAxis
                tickFormatter={formatVndShort}
                tick={{ fontSize: 11, fill: '#9ca3af' }}
                axisLine={false}
                tickLine={false}
                width={52}
              />
              <Tooltip content={<RevenueTooltip />} />
              <Line
                type="monotone"
                dataKey="revenue"
                stroke="#e11d48"
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 4, fill: '#e11d48' }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* ── Bottom row: top products + order status ────────────────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">

        {/* Top 5 products horizontal bar chart */}
        {analyticsLoading ? (
          <SkeletonChart height={260} />
        ) : (
          <div className="bg-white rounded-xl border shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-4">{t('chartTop5')}</h2>
            {topProducts.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-16">{t('noData', { ns: 'common' })}</p>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart
                  layout="vertical"
                  data={topProducts}
                  margin={{ top: 0, right: 16, left: 0, bottom: 0 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" horizontal={false} />
                  <XAxis
                    type="number"
                    tick={{ fontSize: 11, fill: '#9ca3af' }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <YAxis
                    type="category"
                    dataKey="productName"
                    width={130}
                    tick={{ fontSize: 11, fill: '#374151' }}
                    axisLine={false}
                    tickLine={false}
                    tickFormatter={(v: string) =>
                      v.length > 18 ? v.slice(0, 17) + '…' : v
                    }
                  />
                  <Tooltip contentStyle={{ fontSize: 12 }} />
                  <Bar dataKey="totalQuantity" fill="#9a0718" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        )}

        {/* Order status donut chart */}
        {analyticsLoading ? (
          <SkeletonChart height={260} />
        ) : (
          <div className="bg-white rounded-xl border shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-4">{t('chartOrderStatus')}</h2>
            {pieData.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-16">{t('noData', { ns: 'common' })}</p>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <PieChart>
                  <Pie
                    data={pieData}
                    cx="50%"
                    cy="45%"
                    innerRadius={65}
                    outerRadius={100}
                    paddingAngle={3}
                    dataKey="value"
                  >
                    {pieData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={{ fontSize: 12 }} />
                  <Legend
                    iconType="circle"
                    iconSize={8}
                    wrapperStyle={{ fontSize: 11 }}
                  />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>
        )}
      </div>

      {/* ── Quick nav cards ────────────────────────────────────────────────── */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Link
          to="/admin/products"
          className="group bg-white rounded-xl border shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center justify-between mb-2">
            <h3 className="font-semibold text-gray-800">{t('manageFood')}</h3>
            <FiArrowRight
              size={16}
              className="text-gray-400 group-hover:text-rose-600 transition-colors"
            />
          </div>
          <p className="text-sm text-gray-500">
            {t('dashboardTaskManageFood')}
          </p>
        </Link>

        <Link
          to="/admin/orders"
          className="group bg-white rounded-xl border shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center justify-between mb-2">
            <h3 className="font-semibold text-gray-800">{t('manageDeliveries')}</h3>
            <FiArrowRight
              size={16}
              className="text-gray-400 group-hover:text-rose-600 transition-colors"
            />
          </div>
          <p className="text-sm text-gray-500">
            {t('dashboardTaskManageOrders')}
          </p>
        </Link>

        <Link
          to="/admin/stores"
          className="group bg-white rounded-xl border shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center justify-between mb-2">
            <h3 className="font-semibold text-gray-800">{t('manageBranches')}</h3>
            <FiArrowRight
              size={16}
              className="text-gray-400 group-hover:text-rose-600 transition-colors"
            />
          </div>
          <p className="text-sm text-gray-500">
            {t('dashboardTaskManageBranches')}
          </p>
        </Link>
      </div>

    </AdminLayout>
  );
}
