import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiX } from 'react-icons/fi';
import returnRequestService from '@/services/returnRequestService';
import type {
  ReturnReason,
  CreateReturnRequestRequest,
} from '@/types/returnRequest';
import { RETURN_REASON_LABELS, ReturnReason as ReturnReasonEnum } from '@/types/returnRequest';
import { formatPrice } from '@/utils/formatters';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  orderId: string;
  orderTotal: number;
  onSuccess: () => void;
}

export default function CreateReturnRequestModal({
  isOpen,
  onClose,
  orderId,
  orderTotal,
  onSuccess,
}: Props) {
  const { t } = useTranslation(['order', 'common', 'toast']);
  const [reason, setReason] = useState<ReturnReason | ''>(
    ReturnReasonEnum.Defective
  );
  const [description, setDescription] = useState('');
  const [evidenceImageUrl, setEvidenceImageUrl] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      setReason(ReturnReasonEnum.Defective);
      setDescription('');
      setEvidenceImageUrl('');
    }
  }, [isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!reason) {
      toast.error(t('toast:returnReasonRequired'));
      return;
    }

    setLoading(true);
    try {
      const payload: CreateReturnRequestRequest = {
        reason: reason as ReturnReason,
        description: description || undefined,
        evidenceImageUrl: evidenceImageUrl || undefined,
      };

      await returnRequestService.create(orderId, payload);
      toast.success(t('toast:returnRequestSuccess'));
      onSuccess();
      onClose();
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? t('toast:returnRequestFailed'));
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-md w-full max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-lg font-bold">{t('order:returnRequestTitle')}</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
            disabled={loading}
          >
            <FiX size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {/* Lý do trả hàng */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t('order:returnReasonLabel')}
            </label>
            <select
              value={reason}
              onChange={(e) => setReason(e.target.value as ReturnReason)}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500"
            >
              {Object.entries(RETURN_REASON_LABELS).map(([key, label]) => (
                <option key={key} value={key}>
                  {label}
                </option>
              ))}
            </select>
          </div>

          {/* Mô tả chi tiết */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t('order:returnDescription')}
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value.slice(0, 1000))}
              placeholder={t('order:returnDescPlaceholder')}
              maxLength={1000}
              className="w-full border rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-rose-500"
              rows={3}
            />
            <p className="text-xs text-gray-500 mt-1">
              {t('order:charCount', { count: description.length })}
            </p>
          </div>

          {/* Ảnh bằng chứng */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t('order:returnEvidenceUrl')}
            </label>
            <input
              type="url"
              value={evidenceImageUrl}
              onChange={(e) => setEvidenceImageUrl(e.target.value)}
              placeholder="https://..."
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500"
            />
          </div>

          {/* Refund amount */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <p className="text-sm text-gray-700">
              <span className="font-medium">{t('order:refundAmount')}</span> {formatPrice(orderTotal)}
            </p>
            <p className="text-xs text-gray-500 mt-1">
              {t('order:refundNote')}
            </p>
          </div>

          {/* Buttons */}
          <div className="flex gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="flex-1 px-4 py-2 border rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
            >
              {t('common:cancel')}
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex-1 px-4 py-2 bg-rose-600 text-white rounded-lg text-sm font-medium hover:bg-rose-700 disabled:opacity-60 transition-colors"
            >
              {loading ? t('order:sendingRequest') : t('order:submitRequest')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
