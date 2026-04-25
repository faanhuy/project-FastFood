import { Link } from 'react-router-dom';
import { FiShoppingBag, FiMail, FiPhone, FiMapPin, FiGithub } from 'react-icons/fi';

export default function Footer() {
  return (
    <footer className="bg-[#6C537A] text-gray-300 mt-auto">
      <div className="max-w-7xl mx-auto px-4 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">

          {/* Brand */}
          <div className="md:col-span-1">
            <div className="flex items-center gap-2 mb-3">
              <FiShoppingBag className="text-white" size={22} />
              <span className="text-white text-xl font-bold">SmartShop</span>
            </div>
            <p className="text-sm text-gray-400 leading-relaxed">
              Nền tảng mua sắm thông minh với AI — tìm kiếm, gợi ý và mua hàng nhanh chóng.
            </p>
          </div>

          {/* Shop */}
          <div>
            <h4 className="text-white font-semibold mb-4 text-sm uppercase tracking-wider">Mua sắm</h4>
            <ul className="space-y-2.5 text-sm">
              <li><Link to="/products" className="hover:text-blue-400 transition-colors">Tất cả sản phẩm</Link></li>
              <li><Link to="/products?sort=newest" className="hover:text-blue-400 transition-colors">Hàng mới về</Link></li>
              <li><Link to="/cart" className="hover:text-blue-400 transition-colors">Giỏ hàng</Link></li>
              <li><Link to="/checkout" className="hover:text-blue-400 transition-colors">Thanh toán</Link></li>
            </ul>
          </div>

          {/* Account */}
          <div>
            <h4 className="text-white font-semibold mb-4 text-sm uppercase tracking-wider">Tài khoản</h4>
            <ul className="space-y-2.5 text-sm">
              <li><Link to="/profile" className="hover:text-blue-400 transition-colors">Hồ sơ cá nhân</Link></li>
              <li><Link to="/orders" className="hover:text-blue-400 transition-colors">Đơn hàng của tôi</Link></li>
              <li><Link to="/login" className="hover:text-blue-400 transition-colors">Đăng nhập</Link></li>
              <li><Link to="/register" className="hover:text-blue-400 transition-colors">Đăng ký</Link></li>
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h4 className="text-white font-semibold mb-4 text-sm uppercase tracking-wider">Liên hệ</h4>
            <ul className="space-y-2.5 text-sm">
              <li className="flex items-center gap-2">
                <FiMail size={14} className="text-blue-400 shrink-0" />
                <span>huyp18062000@gmail.com</span>
              </li>
              <li className="flex items-center gap-2">
                <FiPhone size={14} className="text-blue-400 shrink-0" />
                <span>0355 609 145</span>
              </li>
              <li className="flex items-start gap-2">
                <FiMapPin size={14} className="text-blue-400 shrink-0 mt-0.5" />
                <span>Hồ Chí Minh, Việt Nam</span>
              </li>
              <li className="flex items-center gap-2 mt-1">
                <FiGithub size={14} className="text-blue-400 shrink-0" />
                <a
                  href="https://github.com/faanhuy/project-SmartShop"
                  target="_blank"
                  rel="noreferrer"
                  className="hover:text-blue-400 transition-colors"
                >
                  GitHub
                </a>
              </li>
            </ul>
          </div>
        </div>

        {/* Divider + Bottom bar */}
        <div className="border-t border-gray-700 mt-10 pt-6 flex flex-col sm:flex-row items-center justify-between gap-3 text-xs text-gray-500">
          <span>© {new Date().getFullYear()} SmartShop. All rights reserved.</span>
          <div className="flex items-center gap-1.5">
            <span className="w-2 h-2 rounded-full bg-green-400 animate-pulse" />
            <span>Powered by .NET 8 + React 19 + HuyPD</span>
          </div>
        </div>
      </div>
    </footer>
  );
}
