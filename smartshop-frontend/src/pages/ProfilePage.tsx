import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { FiUser, FiMail, FiCalendar, FiSave } from 'react-icons/fi';
import Navbar from '../components/Navbar';
import { userService, type UserProfileDto } from '../services/userService';
import { useAuthStore } from '../store/authStore';

export default function ProfilePage() {
  const { updateUser } = useAuthStore();
  const [profile, setProfile] = useState<UserProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName]   = useState('');

  useEffect(() => {
    userService.getMyProfile()
      .then((p) => {
        setProfile(p);
        setFirstName(p.firstName);
        setLastName(p.lastName);
      })
      .catch(() => toast.error('Không thể tải thông tin profile.'))
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!firstName.trim() || !lastName.trim()) return;
    setSaving(true);
    try {
      const updated = await userService.updateMyProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
      });
      setProfile(updated);
      updateUser({ firstName: updated.firstName, lastName: updated.lastName });
      toast.success('Đã cập nhật thông tin.');
    } catch {
      toast.error('Cập nhật thất bại.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="p-8 text-center text-gray-400">Đang tải...</div>
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="max-w-lg mx-auto px-4 py-8">
        <h1 className="text-2xl font-bold mb-6">Thông tin tài khoản</h1>

        {/* Info card */}
        <div className="bg-white rounded-2xl shadow-sm p-6 mb-6 space-y-3">
          <div className="flex items-center gap-3 text-sm text-gray-600">
            <FiMail size={16} className="text-gray-400 shrink-0" />
            <span>{profile?.email}</span>
          </div>
          <div className="flex items-center gap-3 text-sm text-gray-600">
            <FiUser size={16} className="text-gray-400 shrink-0" />
            <span className="capitalize">{profile?.role === 'Admin' ? 'Quản trị viên' : 'Khách hàng'}</span>
          </div>
          <div className="flex items-center gap-3 text-sm text-gray-600">
            <FiCalendar size={16} className="text-gray-400 shrink-0" />
            <span>Tham gia {profile ? new Date(profile.createdAt).toLocaleDateString('vi-VN') : '—'}</span>
          </div>
        </div>

        {/* Edit form */}
        <div className="bg-white rounded-2xl shadow-sm p-6">
          <h2 className="text-base font-semibold text-gray-800 mb-4">Chỉnh sửa thông tin</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Họ</label>
                <input
                  required
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tên</label>
                <input
                  required
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                />
              </div>
            </div>
            <button
              type="submit"
              disabled={saving}
              className="w-full bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors flex items-center justify-center gap-2"
            >
              <FiSave size={15} />
              {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
