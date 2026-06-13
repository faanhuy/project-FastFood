import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { orderService } from '../../services/orderService';
import type { OrderItemDto } from '../../types/order';

interface Props {
  orderId: string;
  orderItems: OrderItemDto[];
  totalAmount: number;
  onSuccess: () => void;
  onClose: () => void;
}

export function AdminRefundModal({ orderId, orderItems, totalAmount, onSuccess, onClose }: Props) {
  const { t } = useTranslation(['admin', 'order', 'toast', 'common']);
  const [isFullRefund, setIsFullRefund] = useState(true);
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [refundNote, setRefundNote] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const selectedItemsArray = Array.from(selectedItems);
  const refundAmount = isFullRefund
    ? totalAmount
    : orderItems
        .filter((item) => selectedItemsArray.includes(item.productId || item.comboId || ''))
        .reduce((sum, item) => sum + item.subTotal, 0);

  const toggleItem = (id: string) => {
    const newSelected = new Set(selectedItems);
    if (newSelected.has(id)) {
      newSelected.delete(id);
    } else {
      newSelected.add(id);
    }
    setSelectedItems(newSelected);
  };

  const handleRefund = async () => {
    if (refundAmount <= 0) {
      toast.error(t('toast:refundFailed'));
      return;
    }

    setIsLoading(true);
    try {
      if (isFullRefund) {
        await orderService.refundOrder(orderId, { refundNote: refundNote || undefined });
        toast.success(t('toast:refundSuccess'));
      } else {
        await orderService.partialRefundOrder(orderId, {
          selectedItemIds: selectedItemsArray,
          refundNote: refundNote || undefined,
        });
        toast.success(t('toast:partialRefundSuccess'));
      }
      onSuccess();
      onClose();
    } catch {
      toast.error(t('toast:refundFailed'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center bg-black bg-opacity-50 p-4">
        <div className="relative w-full max-w-2xl rounded-lg bg-white shadow-xl">
          <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
            <h3 className="text-lg font-semibold text-gray-900">{t('admin:refundOrder')}</h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <span className="text-2xl leading-none">&times;</span>
            </button>
          </div>

          <div className="px-6 py-4">
            <div className="mb-6 space-y-4">
              <div className="flex gap-4">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    checked={isFullRefund}
                    onChange={() => setIsFullRefund(true)}
                    className="h-4 w-4"
                  />
                  <span className="text-sm font-medium text-gray-700">{t('order:fullRefund')}</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    checked={!isFullRefund}
                    onChange={() => setIsFullRefund(false)}
                    className="h-4 w-4"
                  />
                  <span className="text-sm font-medium text-gray-700">{t('admin:partialRefundBtn')}</span>
                </label>
              </div>
            </div>

            {!isFullRefund && (
              <div className="mb-6">
                <h4 className="mb-3 text-sm font-medium text-gray-900">{t('order:selectItemsToRefund')}</h4>
                <div className="space-y-2 rounded-lg border border-gray-200 bg-gray-50 p-3 max-h-48 overflow-y-auto">
                  {orderItems.map((item) => {
                    const itemId = item.productId || item.comboId || '';
                    const itemName = item.productName;
                    return (
                      <label key={itemId} className="flex items-start gap-3 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={selectedItems.has(itemId)}
                          onChange={() => toggleItem(itemId)}
                          className="mt-1 h-4 w-4"
                        />
                        <div className="flex-1 min-w-0">
                          <div className="text-sm font-medium text-gray-900">{itemName}</div>
                          <div className="text-xs text-gray-500">
                            {item.quantity} × {item.unitPrice.toLocaleString('vi-VN')} = {item.subTotal.toLocaleString('vi-VN')}
                          </div>
                        </div>
                      </label>
                    );
                  })}
                </div>
              </div>
            )}

            <div className="mb-6 rounded-lg bg-blue-50 p-3">
              <p className="text-sm font-medium text-blue-900">
                {t('admin:refundAmountCalc', { amount: refundAmount.toLocaleString('vi-VN') })}
              </p>
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                {t('admin:refundNoteInputLabel')}
              </label>
              <textarea
                value={refundNote}
                onChange={(e) => setRefundNote(e.target.value)}
                placeholder={t('common:notesLabel')}
                className="w-full h-24 rounded-lg border border-gray-300 px-3 py-2 text-sm placeholder-gray-400 focus:border-blue-500 focus:outline-none"
              />
            </div>

            <div className="flex justify-end gap-3 border-t border-gray-200 pt-4">
              <button
                onClick={onClose}
                disabled={isLoading}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
              >
                {t('common:cancel')}
              </button>
              <button
                onClick={handleRefund}
                disabled={isLoading || (refundAmount <= 0 && !isFullRefund)}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {isLoading ? t('common:processing') : t('admin:confirmRefund')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
