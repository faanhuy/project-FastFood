import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { orderService } from '../services/orderService';
import toast from 'react-hot-toast';
import type { OrderTimelineEventDto } from '../types/order';

interface Props {
  orderId: string;
}

export function OrderTimeline({ orderId }: Props) {
  const { t } = useTranslation(['order', 'common']);
  const [events, setEvents] = useState<OrderTimelineEventDto[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const data = await orderService.getOrderTimeline(orderId);
        setEvents(data);
      } catch {
        toast.error(t('toast:timelineLoadFailed'));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [orderId, t]);

  if (loading) return <div className="text-sm text-gray-500">{t('common:loading')}</div>;
  if (!events.length) return <div className="text-sm text-gray-400">{t('order:timelineEmpty')}</div>;

  return (
    <div className="flow-root">
      <ul className="-mb-8">
        {events.map((event, idx) => (
          <li key={event.id}>
            <div className="relative pb-8">
              {idx < events.length - 1 && (
                <span className="absolute top-4 left-4 -ml-px h-full w-0.5 bg-gray-200" />
              )}
              <div className="relative flex space-x-3">
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100 ring-8 ring-white text-blue-600 text-xs font-bold">
                  {event.eventType === 'Created' ? '📦' : event.eventType === 'Refunded' ? '💰' : '→'}
                </div>
                <div className="flex min-w-0 flex-1 justify-between space-x-4 pt-1.5">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{event.title}</p>
                    {event.description && <p className="text-xs text-gray-500">{event.description}</p>}
                    {event.amount != null && (
                      <p className="text-xs text-green-600 font-medium">
                        {t('order:refundAmountLabel')}: {event.amount.toLocaleString('vi-VN')}
                      </p>
                    )}
                  </div>
                  <div className="whitespace-nowrap text-right text-xs text-gray-500">
                    <div>{new Date(event.occurredAt).toLocaleDateString('vi-VN')}</div>
                    <div>{new Date(event.occurredAt).toLocaleTimeString('vi-VN')}</div>
                    {event.actorName && <div className="text-gray-400">{event.actorName}</div>}
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
