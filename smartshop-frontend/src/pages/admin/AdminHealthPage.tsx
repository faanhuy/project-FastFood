import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { FiRefreshCw, FiToggleRight, FiActivity } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import api from '../../services/api';

interface HealthCheckEntry {
  name: string;
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  duration: string;
  description: string | null;
  error: string | null;
}

interface HealthDetailResponse {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  timestamp: string;
  uptime: string;
  checks: HealthCheckEntry[];
  system: {
    memoryMb: number;
    processorCount: number;
  };
}

export default function AdminHealthPage() {
  const { t } = useTranslation(['admin', 'toast']);
  const [health, setHealth] = useState<HealthDetailResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [autoRefresh, setAutoRefresh] = useState(false);

  const loadHealth = async () => {
    try {
      setIsLoading(true);
      const { data } = await api.get<HealthDetailResponse>('/health/detail');
      setHealth(data);
    } catch (error) {
      toast.error(t('toast:healthLoadFailed'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefresh = async () => {
    await loadHealth();
    if (!isLoading) {
      toast.success(t('toast:healthRefreshed'));
    }
  };

  useEffect(() => {
    loadHealth();
  }, []);

  useEffect(() => {
    if (!autoRefresh) return;

    const interval = setInterval(() => {
      loadHealth();
    }, 30000);

    return () => clearInterval(interval);
  }, [autoRefresh]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Healthy':
        return 'bg-green-100 text-green-800 border-green-300';
      case 'Degraded':
        return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'Unhealthy':
        return 'bg-red-100 text-red-800 border-red-300';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'Healthy':
        return t('healthStatusHealthy');
      case 'Degraded':
        return t('healthStatusDegraded');
      case 'Unhealthy':
        return t('healthStatusUnhealthy');
      default:
        return status;
    }
  };

  const getCheckDisplayName = (name: string) => {
    const map: Record<string, string> = {
      database: t('healthCheckDatabase'),
      redis: t('healthCheckRedis'),
      hangfire: t('healthCheckHangfire'),
    };
    return map[name.toLowerCase()] ?? name;
  };

  const formatDate = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleString();
    } catch {
      return dateStr;
    }
  };

  if (isLoading && !health) {
    return (
      <AdminLayout title={t('healthStatus')}>
        <div className="flex items-center justify-center h-64">
          <p className="text-gray-600">{t('healthLoading')}</p>
        </div>
      </AdminLayout>
    );
  }

  return (
    <AdminLayout title={t('healthStatus')}>
      <div className="space-y-6">
        {/* Overall Status Card */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <FiActivity size={24} className="text-gray-700" />
              <h2 className="text-lg font-semibold text-gray-900">{t('healthStatus')}</h2>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={handleRefresh}
                disabled={isLoading}
                className="flex items-center gap-2 px-3 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
              >
                <FiRefreshCw size={16} />
                {t('healthRefreshBtn')}
              </button>
              <button
                onClick={() => setAutoRefresh(!autoRefresh)}
                className={`flex items-center gap-2 px-3 py-2 text-sm rounded-lg ${
                  autoRefresh
                    ? 'bg-green-600 text-white hover:bg-green-700'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
                title={t('healthAutoRefresh')}
              >
                <FiToggleRight size={16} />
              </button>
            </div>
          </div>

          {health && (
            <div className="space-y-3">
              <div className="flex items-center gap-3">
                <span className="text-sm text-gray-600">{t('healthCheckStatus')}:</span>
                <span
                  className={`px-3 py-1 rounded-full text-sm font-semibold border ${getStatusColor(
                    health.status
                  )}`}
                >
                  {getStatusText(health.status)}
                </span>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-gray-600">{t('healthLastCheck')}:</p>
                  <p className="font-mono text-xs text-gray-800 mt-1">{formatDate(health.timestamp)}</p>
                </div>
                <div>
                  <p className="text-gray-600">{t('healthUptime')}:</p>
                  <p className="font-mono text-xs text-gray-800 mt-1">{health.uptime}</p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Health Checks */}
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            {t('admin:healthCheckStatus')}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {health?.checks.map((check) => (
              <div key={check.name} className="bg-white rounded-lg shadow p-4">
                <div className="flex items-start justify-between mb-3">
                  <h4 className="font-semibold text-gray-900">{getCheckDisplayName(check.name)}</h4>
                  <span
                    className={`px-2 py-1 rounded text-xs font-semibold border ${getStatusColor(
                      check.status
                    )}`}
                  >
                    {getStatusText(check.status)}
                  </span>
                </div>

                <div className="space-y-2 text-sm">
                  <div>
                    <p className="text-gray-600">{t('healthCheckDuration')}:</p>
                    <p className="font-mono text-gray-800">{check.duration}</p>
                  </div>

                  {check.description && (
                    <div>
                      <p className="text-gray-600">{t('healthCheckDescription')}:</p>
                      <p className="text-gray-800 break-words">{check.description}</p>
                    </div>
                  )}

                  {check.error && (
                    <div>
                      <p className="text-gray-600">{t('healthErrorDetail')}:</p>
                      <p className="text-red-700 text-xs break-words bg-red-50 p-2 rounded mt-1">
                        {check.error}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* System Metrics */}
        {health && (
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">{t('healthSystemMetrics')}</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <p className="text-gray-600 text-sm">{t('healthMemoryUsage')}</p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold text-gray-900">{health.system.memoryMb}</p>
                  <p className="text-gray-600 text-sm">MB</p>
                </div>
              </div>
              <div className="space-y-2">
                <p className="text-gray-600 text-sm">{t('healthProcessorCount')}</p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold text-gray-900">{health.system.processorCount}</p>
                  <p className="text-gray-600 text-sm">{t('healthCoresUnit')}</p>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
