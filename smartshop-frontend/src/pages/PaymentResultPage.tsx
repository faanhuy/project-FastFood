import { useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FiCheckCircle, FiXCircle, FiShoppingBag, FiRefreshCw } from 'react-icons/fi';
import toast from 'react-hot-toast';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import { paymentService } from '../services/paymentService';

export default function PaymentResultPage() {
  const { t } = useTranslation('cart');
  const { t: tToast } = useTranslation('toast');
  const [params] = useSearchParams();
  const [retrying, setRetrying] = useState(false);

  const successParam = params.get('success');
  const rawRef = params.get('orderId') ?? params.get('vnp_TxnRef') ?? '';
  const orderId = rawRef.includes('_') ? rawRef.substring(0, rawRef.lastIndexOf('_')) : rawRef || null;
  const transactionNo = params.get('vnp_TransactionNo');
  const isSuccess = successParam?.toLowerCase() === 'true';

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="max-w-lg mx-auto px-4 py-16 text-center">
        {isSuccess ? (
          <>
            <div className="flex justify-center mb-6">
              <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center">
                <FiCheckCircle size={40} className="text-green-500" />
              </div>
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">{t('paymentSuccess')}</h1>
            <p className="text-gray-500 mb-1 text-sm">{t('paymentSuccessDesc')}</p>
            {transactionNo && (
              <p className="text-xs text-gray-400 mb-6">
                {t('vnpayTxn')}: <span className="font-medium text-gray-600">{transactionNo}</span>
              </p>
            )}
            <div className="bg-white rounded-2xl shadow-sm p-6 mb-6 text-left space-y-2">
              {orderId && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">{t('orderId')}</span>
                  <span className="font-medium text-gray-800 font-mono text-xs">{orderId}</span>
                </div>
              )}
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">{t('paymentStatus')}</span>
                <span className="font-medium text-green-600">{t('paymentPaid')}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">{t('paymentMethod')}</span>
                <span className="font-medium text-gray-800">{t('paymentVNPay')}</span>
              </div>
            </div>
            <div className="flex gap-3 justify-center">
              {orderId && (
                <Link
                  to={`/orders/${orderId}`}
                  className="flex items-center gap-2 bg-rose-600 text-white px-5 py-2.5 rounded-xl font-medium text-sm hover:bg-rose-700 transition-colors"
                >
                  <FiShoppingBag size={16} />
                  {t('viewOrder')}
                </Link>
              )}
              <Link
                to="/orders"
                className="flex items-center gap-2 border border-gray-300 text-gray-700 px-5 py-2.5 rounded-xl font-medium text-sm hover:bg-gray-50 transition-colors"
              >
                {t('orderHistory')}
              </Link>
            </div>
          </>
        ) : (
          <>
            <div className="flex justify-center mb-6">
              <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center">
                <FiXCircle size={40} className="text-red-500" />
              </div>
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">{t('paymentFailed')}</h1>
            <p className="text-gray-500 mb-1 text-sm">{t('paymentFailedDesc')}</p>
            {orderId && (
              <p className="text-xs text-gray-400 mb-6">
                {t('orderCode')}: <span className="font-medium text-gray-600 font-mono">{orderId}</span>
              </p>
            )}
            <div className="bg-white rounded-2xl shadow-sm p-6 mb-6 text-left">
              <p className="text-sm text-gray-600">
                {t('paymentFailedNote')}
                {t('checkOrderHistory')}{' '}
                <Link to="/orders" className="text-rose-600 hover:underline">{t('orderHistory')}</Link>{' '}
                {t('retryPayment')}
              </p>
            </div>
            <div className="flex gap-3 justify-center">
              {orderId && (
                <button
                  onClick={async () => {
                    setRetrying(true);
                    try {
                      const url = await paymentService.createVNPayUrl(orderId);
                      window.location.href = url;
                    } catch {
                      toast.error(tToast('paymentLinkFailed'));
                      setRetrying(false);
                    }
                  }}
                  disabled={retrying}
                  className="flex items-center gap-2 bg-rose-600 text-white px-5 py-2.5 rounded-xl font-medium text-sm hover:bg-rose-700 transition-colors disabled:opacity-60"
                >
                  <FiRefreshCw size={16} className={retrying ? 'animate-spin' : ''} />
                  {retrying ? t('processingPayment') : t('retryPaymentBtn')}
                </button>
              )}
              <Link
                to="/orders"
                className="flex items-center gap-2 border border-gray-300 text-gray-700 px-5 py-2.5 rounded-xl font-medium text-sm hover:bg-gray-50 transition-colors"
              >
                {t('orderHistory')}
              </Link>
            </div>
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}
