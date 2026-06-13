import { useTranslation } from 'react-i18next';
import { FiX, FiMail, FiShoppingBag, FiDollarSign, FiCalendar, FiSlash } from 'react-icons/fi';
import type { UserDto } from '../../services/adminUserService';

interface Props {
  user: UserDto;
  onClose: () => void;
}

export default function AdminUserDetailModal({ user, onClose }: Props) {
  const { t } = useTranslation('admin');
  const { t: tCommon } = useTranslation('common');

  const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase();

  return (
    <div
      className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[85vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-start justify-between gap-3 p-5 border-b border-gray-100">
          <div className="flex items-center gap-3 min-w-0">
            <div className="w-12 h-12 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-lg shrink-0">
              {initials || '?'}
            </div>
            <div className="min-w-0">
              <h2 className="text-base font-bold text-gray-900 truncate">
                {user.firstName} {user.lastName}
              </h2>
              <div className="flex items-center gap-1.5 text-sm text-gray-500 truncate">
                <FiMail size={13} className="shrink-0" />
                <span className="truncate">{user.email}</span>
              </div>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition shrink-0"
            aria-label={tCommon('close')}
          >
            <FiX size={22} />
          </button>
        </div>

        {/* Badges */}
        <div className="flex flex-wrap items-center gap-2 px-5 pt-4">
          <span
            className={`inline-flex px-2.5 py-1 rounded-full text-xs font-medium ${
              user.role === 'Admin'
                ? 'bg-indigo-100 text-indigo-700'
                : 'bg-gray-100 text-gray-700'
            }`}
          >
            {user.role === 'Admin' ? t('adminRole') : t('customerRole')}
          </span>
          <span
            className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${
              user.isBanned ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'
            }`}
          >
            {user.isBanned && <FiSlash size={11} />}
            {user.isBanned ? t('bannedStatus') : t('activeStatus')}
          </span>
        </div>

        {/* Detail grid */}
        <div className="grid grid-cols-2 gap-3 p-5">
          <div className="bg-gray-50 rounded-xl p-3">
            <div className="flex items-center gap-1.5 text-xs text-gray-500 mb-1">
              <FiShoppingBag size={13} />
              {t('userTotalOrders')}
            </div>
            <p className="text-lg font-semibold text-gray-900">{user.orderCount}</p>
          </div>

          <div className="bg-gray-50 rounded-xl p-3">
            <div className="flex items-center gap-1.5 text-xs text-gray-500 mb-1">
              <FiDollarSign size={13} />
              {t('userTotalSpent')}
            </div>
            <p className="text-lg font-semibold text-gray-900">
              {user.totalSpent.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })}
            </p>
          </div>

          <div className="bg-gray-50 rounded-xl p-3">
            <div className="flex items-center gap-1.5 text-xs text-gray-500 mb-1">
              <FiCalendar size={13} />
              {t('registeredAt')}
            </div>
            <p className="text-sm font-medium text-gray-900">
              {new Date(user.createdAt).toLocaleDateString('vi-VN')}
            </p>
          </div>

          {user.isBanned && user.bannedAt && (
            <div className="bg-red-50 rounded-xl p-3">
              <div className="flex items-center gap-1.5 text-xs text-red-500 mb-1">
                <FiSlash size={13} />
                {t('bannedStatus')}
              </div>
              <p className="text-sm font-medium text-red-700">
                {new Date(user.bannedAt).toLocaleDateString('vi-VN')}
              </p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 px-5 pb-5">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-gray-100 text-gray-800 rounded-lg text-sm font-medium hover:bg-gray-200 transition"
          >
            {tCommon('close')}
          </button>
        </div>
      </div>
    </div>
  );
}
