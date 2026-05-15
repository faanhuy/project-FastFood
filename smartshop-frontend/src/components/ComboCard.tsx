import { useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import type { CatalogItemDto } from '../types/catalog';
import { formatPrice } from '../utils/formatters';
import { getImageUrl } from '../utils/imageUrl';
import { cartService } from '../services/cartService';
import { getApiError } from '../utils/errorHandler';

interface ComboCardProps {
  combo: CatalogItemDto;
}

export default function ComboCard({ combo }: ComboCardProps) {
  const [adding, setAdding] = useState(false);

  const handleAddToCart = async (e: React.MouseEvent) => {
    e.preventDefault();
    setAdding(true);
    try {
      await cartService.addComboToCart(combo.id, 1);
      toast.success(`Đã thêm combo "${combo.name}" vào giỏ hàng`);
    } catch (err) {
      toast.error(getApiError(err, 'Thêm combo thất bại.'));
    } finally {
      setAdding(false);
    }
  };

  const discountPercent = combo.discountPercent ? Math.round(combo.discountPercent * 10) / 10 : null;

  const hasEndDate = combo.endsAt && new Date(combo.endsAt) > new Date();
  let endDateText = '';
  if (hasEndDate) {
    const endDate = new Date(combo.endsAt!);
    const today = new Date();
    const daysLeft = Math.ceil((endDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    if (daysLeft > 0) {
      endDateText = `Còn ${daysLeft} ngày`;
    }
  }

  return (
    <Link
      to="#"
      onClick={(e) => e.preventDefault()}
      className="relative bg-white rounded-xl shadow-sm hover:shadow-lg hover:-translate-y-1 hover:border-orange-200 border border-transparent transition-all duration-200 p-3 flex flex-col group cursor-pointer"
    >
      {/* COMBO Badge */}
      <span className="absolute left-2 top-2 rounded-full bg-orange-500 px-2.5 py-0.5 text-[11px] font-bold text-white uppercase tracking-wide">
        Combo
      </span>

      {/* Discount Badge */}
      {discountPercent && discountPercent > 0 && (
        <span className="absolute right-2 top-2 rounded-full bg-red-500 px-2 py-0.5 text-[11px] font-bold text-white">
          -{discountPercent}%
        </span>
      )}

      {/* Image */}
      <div className="bg-gray-100 rounded-lg h-36 flex items-center justify-center mb-3 overflow-hidden">
        {combo.imageUrl ? (
          <img src={getImageUrl(combo.imageUrl)} alt={combo.name} className="h-full w-full object-contain" />
        ) : (
          <span className="text-gray-300 text-4xl">📦</span>
        )}
      </div>

      {/* Combo Name */}
      <p className="text-sm font-medium text-gray-800 line-clamp-2 flex-1">{combo.name}</p>

      {/* Combo Item Count */}
      {combo.comboItemCount && (
        <p className="text-xs text-gray-500 mt-0.5">{combo.comboItemCount} món</p>
      )}

      {/* Price Section */}
      <div className="mt-2">
        <p className="text-rose-600 font-bold text-sm">{formatPrice(combo.price)}</p>
        {combo.originalPrice && combo.originalPrice > combo.price && (
          <p className="text-gray-400 text-xs line-through">{formatPrice(combo.originalPrice)}</p>
        )}
      </div>

      {/* End Date */}
      {endDateText && <p className="text-xs text-orange-600 font-medium mt-1">{endDateText}</p>}

      {/* Add to Cart Button */}
      <button
        onClick={handleAddToCart}
        disabled={adding}
        className="mt-2 w-full text-xs bg-orange-500 hover:bg-orange-600 disabled:bg-gray-300 disabled:text-gray-500 text-white rounded-lg py-1.5 transition-colors font-semibold"
      >
        {adding ? 'Đang thêm...' : 'Thêm vào giỏ'}
      </button>
    </Link>
  );
}
