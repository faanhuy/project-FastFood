import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { FiEye, FiLock, FiUnlock, FiLogOut, FiKey } from 'react-icons/fi';
import { adminUserService, type UserDto } from '../../services/adminUserService';
import AdminLayout from '../../components/AdminLayout';
import AdminUserDetailModal from '../../components/admin/AdminUserDetailModal';
import Pagination from '../../components/common/Pagination';
import { getApiError } from '../../utils/errorHandler';
import { BulkActionToolbar, SortableColumnHeader, AdminTableCheckbox } from '../../components/admin';

const PAGE_SIZE = 20;

export default function AdminUsersPage() {
  const { t } = useTranslation('admin');
  const { t: tToast } = useTranslation('toast');
  const { t: tCommon } = useTranslation('common');

  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [searchEmail, setSearchEmail] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [bannedFilter, setBannedFilter] = useState('');

  // Bulk actions state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [bulkLoading, setBulkLoading] = useState(false);
  const [sortBy, setSortBy] = useState('createdAt');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');

  // Detail modal state
  const [detailUser, setDetailUser] = useState<UserDto | null>(null);
  const [_detailLoading, setDetailLoading] = useState(false);

  // Confirm modal state
  const [confirmModal, setConfirmModal] = useState<{ type: 'ban' | 'unban'; user: UserDto } | null>(null);

  const loadUsers = async (p: number) => {
    setLoading(true);
    try {
      const bannedParam =
        bannedFilter === 'banned' ? true : bannedFilter === 'active' ? false : undefined;
      const result = await adminUserService.getUsers(
        p,
        PAGE_SIZE,
        roleFilter || undefined,
        bannedParam,
        searchEmail || undefined,
        sortBy,
        sortDirection
      );
      setUsers(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('loadFailed')));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers(page);
  }, [page, searchEmail, roleFilter, bannedFilter, sortBy, sortDirection]);

  const handleBan = (user: UserDto) => {
    setConfirmModal({ type: 'ban', user });
  };

  const handleUnban = (user: UserDto) => {
    setConfirmModal({ type: 'unban', user });
  };

  const handleConfirmAction = async () => {
    if (!confirmModal) return;
    const { type, user } = confirmModal;
    try {
      if (type === 'ban') {
        await adminUserService.banUser(user.id);
        toast.success(tToast('userBanSuccess'));
      } else {
        await adminUserService.unbanUser(user.id);
        toast.success(tToast('userUnbanSuccess'));
      }
      await loadUsers(page);
      setConfirmModal(null);
    } catch (error: any) {
      toast.error(getApiError(error, tToast(type === 'ban' ? 'userBanFailed' : 'userUnbanFailed')));
    }
  };

  const handleRoleChange = async (user: UserDto, newRole: string) => {
    if (newRole === user.role) return;
    if (!window.confirm(`${t('confirmRoleChange')} ${newRole}?`)) return;
    try {
      await adminUserService.updateUserRole(user.id, newRole);
      toast.success(tToast('userRoleUpdateSuccess'));
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('userRoleUpdateFailed')));
    }
  };

  const handleSearch = (value: string) => {
    setSearchEmail(value);
    setPage(1);
  };

  const handleRoleFilterChange = (value: string) => {
    setRoleFilter(value);
    setPage(1);
  };

  const handleBannedFilterChange = (value: string) => {
    setBannedFilter(value);
    setPage(1);
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedIds(new Set(users.map((u) => u.id)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (checked) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  const handleBulkAction = async (actionId: string) => {
    const ids = Array.from(selectedIds);
    if (!window.confirm(t('bulkConfirmMessage', { count: ids.length }))) return;

    setBulkLoading(true);
    try {
      const result = await adminUserService.bulkUpdateUsers(ids, actionId as any);
      toast.success(t('bulkResultSuccess', { count: result.succeeded }));
      if (result.failed > 0) {
        toast.error(t('bulkResultFailed', { count: result.failed }));
      }
      setSelectedIds(new Set());
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('loadFailed')));
    } finally {
      setBulkLoading(false);
    }
  };

  const handleViewDetail = async (user: UserDto) => {
    setDetailLoading(true);
    try {
      const detail = await adminUserService.getUserDetail(user.id);
      setDetailUser(detail);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('loadFailed')));
    } finally {
      setDetailLoading(false);
    }
  };

  const handleForceLogout = async (user: UserDto) => {
    if (!window.confirm(t('forceLogoutConfirm'))) return;
    try {
      await adminUserService.forceLogout(user.id);
      toast.success(tToast('forceLogoutSuccess'));
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('forceLogoutFailed')));
    }
  };

  const handleResetPassword = async (user: UserDto) => {
    if (!window.confirm(t('resetPasswordConfirm'))) return;
    try {
      await adminUserService.resetPassword(user.id);
      toast.success(tToast('resetPasswordSuccess'));
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('resetPasswordFailed')));
    }
  };

  const bulkActions = [
    { id: 'ban', label: t('bulkBan'), variant: 'warning' as const },
    { id: 'unban', label: t('bulkUnban'), variant: 'default' as const },
  ];

  return (
    <AdminLayout title={t('manageUsers')}>
      <div className="p-6">
        <h1 className="text-2xl font-bold mb-6">{t('manageUsers')}</h1>

        {/* Filters */}
        <div className="flex flex-wrap gap-3 mb-6">
          <input
            type="text"
            placeholder={t('searchByEmail')}
            value={searchEmail}
            onChange={(e) => handleSearch(e.target.value)}
            className="border rounded-lg px-3 py-2 text-sm flex-1 min-w-60 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <select
            value={roleFilter}
            onChange={(e) => handleRoleFilterChange(e.target.value)}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">{t('allRoles')}</option>
            <option value="Customer">{t('customerRole')}</option>
            <option value="Admin">{t('adminRole')}</option>
          </select>
          <select
            value={bannedFilter}
            onChange={(e) => handleBannedFilterChange(e.target.value)}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">{t('allStatus')}</option>
            <option value="active">{t('activeStatus')}</option>
            <option value="banned">{t('bannedStatus')}</option>
          </select>
        </div>

        {/* Table */}
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
          </div>
        ) : (
          <>
            <div className="overflow-x-auto rounded-xl border border-gray-200">
              <table className="w-full text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700 w-12">
                      <AdminTableCheckbox
                        checked={selectedIds.size > 0 && selectedIds.size === users.length}
                        indeterminate={selectedIds.size > 0 && selectedIds.size < users.length}
                        onChange={(checked) => handleSelectAll(checked)}
                      />
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">
                      <SortableColumnHeader label={t('userEmail')} field="email" currentSortBy={sortBy} currentSortDirection={sortDirection}
                        onSort={(field, dir) => {
                          setSortBy(field);
                          setSortDirection(dir);
                          setPage(1);
                        }} />
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userName')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userRole')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userStatus')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userOrders')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">
                      <SortableColumnHeader label={t('registeredAt')} field="createdAt" currentSortBy={sortBy} currentSortDirection={sortDirection}
                        onSort={(field, dir) => {
                          setSortBy(field);
                          setSortDirection(dir);
                          setPage(1);
                        }} />
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userActions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="px-4 py-8 text-center text-gray-500">
                        {tCommon('noData')}
                      </td>
                    </tr>
                  ) : (
                    users.map((user) => (
                      <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                        <td className="px-4 py-3 text-gray-800">
                          <AdminTableCheckbox
                            checked={selectedIds.has(user.id)}
                            onChange={(checked) => handleSelectOne(user.id, checked)}
                          />
                        </td>
                        <td className="px-4 py-3 text-gray-800">{user.email}</td>
                        <td className="px-4 py-3 text-gray-800">
                          {user.firstName} {user.lastName}
                        </td>
                        <td className="px-4 py-3">
                          <select
                            value={user.role}
                            onChange={(e) => handleRoleChange(user, e.target.value)}
                            className="border rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                          >
                            <option value="Customer">{t('customerRole')}</option>
                            <option value="Admin">{t('adminRole')}</option>
                          </select>
                        </td>
                        <td className="px-4 py-3">
                          <span
                            className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${
                              user.isBanned
                                ? 'bg-red-100 text-red-700'
                                : 'bg-green-100 text-green-700'
                            }`}
                          >
                            {user.isBanned ? t('bannedStatus') : t('activeStatus')}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-700">{user.orderCount}</td>
                        <td className="px-4 py-3 text-gray-600 text-xs">
                          {new Date(user.createdAt).toLocaleDateString('vi-VN')}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex flex-wrap items-center gap-1.5">
                            <button
                              onClick={() => handleViewDetail(user)}
                              title={t('userDetailTitle')}
                              className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 transition-colors"
                            >
                              <FiEye size={14} />
                              {t('userDetailTitle')}
                            </button>
                            {user.isBanned ? (
                              <button
                                onClick={() => handleUnban(user)}
                                title={t('unbanUser')}
                                className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-green-700 bg-green-50 hover:bg-green-100 transition-colors"
                              >
                                <FiUnlock size={14} />
                                {t('unbanUser')}
                              </button>
                            ) : (
                              <button
                                onClick={() => handleBan(user)}
                                title={t('banUser')}
                                className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-red-700 bg-red-50 hover:bg-red-100 transition-colors"
                              >
                                <FiLock size={14} />
                                {t('banUser')}
                              </button>
                            )}
                            <button
                              onClick={() => handleForceLogout(user)}
                              title={t('forceLogout')}
                              className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-orange-700 bg-orange-50 hover:bg-orange-100 transition-colors"
                            >
                              <FiLogOut size={14} />
                              {t('forceLogout')}
                            </button>
                            <button
                              onClick={() => handleResetPassword(user)}
                              title={t('resetPassword')}
                              className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 transition-colors"
                            >
                              <FiKey size={14} />
                              {t('resetPassword')}
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            <div className="mt-4 flex items-center justify-between">
              <p className="text-sm text-gray-600">
                {t('totalUsers')}: <span className="font-semibold">{totalCount}</span>
              </p>
              <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
            </div>

            {/* Bulk Action Toolbar */}
            {selectedIds.size > 0 && (
              <BulkActionToolbar
                selectedCount={selectedIds.size}
                actions={bulkActions}
                onAction={handleBulkAction}
                isLoading={bulkLoading}
              />
            )}
          </>
        )}

        {detailUser && (
          <AdminUserDetailModal user={detailUser} onClose={() => setDetailUser(null)} />
        )}

        {/* Ban/Unban Confirm Modal */}
        {confirmModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="bg-white rounded-lg max-w-sm w-full p-6">
              <h2 className="text-lg font-semibold text-gray-800 mb-3">
                {confirmModal.type === 'ban' ? t('confirmBanUser') : t('confirmUnbanUser')}
              </h2>
              <p className="text-sm text-gray-600 mb-6">
                {confirmModal.type === 'ban'
                  ? `${t('confirmBanUser')} ${confirmModal.user.email}?`
                  : `${t('confirmUnbanUser')} ${confirmModal.user.email}?`}
              </p>
              <div className="flex gap-3 justify-end">
                <button
                  onClick={() => setConfirmModal(null)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
                >
                  {tCommon('cancel')}
                </button>
                <button
                  onClick={handleConfirmAction}
                  className={`px-4 py-2 text-sm font-medium text-white rounded-lg transition-colors ${
                    confirmModal.type === 'ban'
                      ? 'bg-red-600 hover:bg-red-700'
                      : 'bg-green-600 hover:bg-green-700'
                  }`}
                >
                  {confirmModal.type === 'ban' ? t('banUser') : t('unbanUser')}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
