import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { adminUserService, type UserDto } from '../../services/adminUserService';
import AdminLayout from '../../components/AdminLayout';
import Pagination from '../../components/common/Pagination';
import { getApiError } from '../../utils/errorHandler';

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
        searchEmail || undefined
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
  }, [page, searchEmail, roleFilter, bannedFilter]);

  const handleBan = async (user: UserDto) => {
    if (!window.confirm(`${t('confirmBanUser')} ${user.email}?`)) return;
    try {
      await adminUserService.banUser(user.id);
      toast.success(tToast('userBanSuccess'));
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('userBanFailed')));
    }
  };

  const handleUnban = async (user: UserDto) => {
    if (!window.confirm(`${t('confirmUnbanUser')} ${user.email}?`)) return;
    try {
      await adminUserService.unbanUser(user.id);
      toast.success(tToast('userUnbanSuccess'));
      await loadUsers(page);
    } catch (error: any) {
      toast.error(getApiError(error, tToast('userUnbanFailed')));
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
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userEmail')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userName')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userRole')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userStatus')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userOrders')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('registeredAt')}</th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('userActions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                        {tCommon('noData')}
                      </td>
                    </tr>
                  ) : (
                    users.map((user) => (
                      <tr key={user.id} className="hover:bg-gray-50 transition-colors">
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
                          {user.isBanned ? (
                            <button
                              onClick={() => handleUnban(user)}
                              className="text-green-600 hover:text-green-800 font-medium text-xs underline"
                            >
                              {t('unbanUser')}
                            </button>
                          ) : (
                            <button
                              onClick={() => handleBan(user)}
                              className="text-red-600 hover:text-red-800 font-medium text-xs underline"
                            >
                              {t('banUser')}
                            </button>
                          )}
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
          </>
        )}
      </div>
    </AdminLayout>
  );
}
