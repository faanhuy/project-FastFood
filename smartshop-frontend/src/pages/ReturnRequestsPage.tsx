import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { FiArrowLeft } from 'react-icons/fi';
import returnRequestService from '../services/returnRequestService';
import type { ReturnRequestDto } from '../types/returnRequest';
import {
  RETURN_REASON_LABELS,
  RETURN_STATUS_LABELS,
  RETURN_STATUS_COLORS,
} from '../types/returnRequest';
import { formatPrice, formatDateTime } from '../utils/formatters';
import { getApiError } from '../utils/errorHandler';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';

export default function ReturnRequestsPage() {
  const { t } = useTranslation('order');
  const { t: tToast } = useTranslation('toast');
  const [returnRequests, setReturnRequests] = useState<ReturnRequestDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const loadData = async () => {
      try {
        const data = await returnRequestService.getMyReturnRequests();
        setReturnRequests(data);
      } catch (err) {
        setError(getApiError(err, tToast('returnLoadFailed')));
        toast.error(tToast('returnLoadFailed'));
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [tToast]);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-8 text-center text-gray-400">{t('loading')}</div>
        <Footer />
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-8 text-center text-red-500">{error}</div>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="max-w-4xl mx-auto p-6">
        <button
          onClick={() => navigate('/orders')}
          className="text-rose-600 hover:text-rose-800 mb-4 flex items-center gap-1.5 text-sm"
        >
          <FiArrowLeft size={16} /> {t('back')}
        </button>

        <h1 className="text-2xl font-bold mb-6">{t('myReturnRequests')}</h1>

        {returnRequests.length === 0 ? (
          <div className="bg-white rounded-lg p-8 text-center text-gray-500">
            <p>{t('noReturnRequests')}</p>
          </div>
        ) : (
          <div className="space-y-4">
            {returnRequests.map((request) => (
              <div key={request.id} className="bg-white rounded-lg border p-4">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <p className="font-semibold text-lg">
                      {t('orderNumber')}{request.orderNumber || request.orderId.slice(0, 8).toUpperCase()}
                    </p>
                    <p className="text-sm text-gray-500">
                      {formatDateTime(request.createdAt)}
                    </p>
                  </div>
                  <span
                    className={`text-xs px-3 py-1 rounded-full font-medium ${
                      RETURN_STATUS_COLORS[request.status]
                    }`}
                  >
                    {RETURN_STATUS_LABELS[request.status]}
                  </span>
                </div>

                <div className="grid grid-cols-2 gap-4 mb-3 text-sm">
                  <div>
                    <p className="text-gray-600">{t('reasonLabel')}</p>
                    <p className="font-medium">
                      {RETURN_REASON_LABELS[request.reason]}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600">{t('refundLabel')}</p>
                    <p className="font-medium text-rose-600">
                      {formatPrice(request.refundAmount)}
                    </p>
                  </div>
                </div>

                {request.description && (
                  <div className="mb-3 text-sm bg-gray-50 p-2 rounded">
                    <p className="text-gray-600 font-medium mb-1">{t('descriptionLabel')}</p>
                    <p className="text-gray-700">{request.description}</p>
                  </div>
                )}

                {request.evidenceImageUrl && (
                  <div className="mb-3">
                    <p className="text-sm text-gray-600 font-medium mb-1">
                      {t('evidenceImageLabel')}
                    </p>
                    <a
                      href={request.evidenceImageUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-xs text-blue-600 hover:underline"
                    >
                      {request.evidenceImageUrl}
                    </a>
                  </div>
                )}

                {request.status !== 'Pending' && request.adminNote && (
                  <div className="bg-yellow-50 border border-yellow-200 rounded p-2 text-sm">
                    <p className="text-gray-600 font-medium mb-1">{t('adminNoteLabel')}</p>
                    <p className="text-gray-700">{request.adminNote}</p>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
      <Footer />
    </div>
  );
}
