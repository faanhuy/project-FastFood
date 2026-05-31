import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { geographyService } from '@/services/geographyService';
import type { Province, Ward } from '@/types/geography';

export interface AddressSelection {
  provinceId: number;
  provinceName: string;
  wardId: number;
  wardName: string;
  street: string;
}

interface AddressSelectorProps {
  value?: Partial<AddressSelection>;
  onChange: (selection: AddressSelection) => void;
  disabled?: boolean;
}

export function AddressSelector({ value, onChange, disabled = false }: AddressSelectorProps) {
  const { t } = useTranslation('cart');
  const [provinces, setProvinces] = useState<Province[]>([]);
  const [wards, setWards] = useState<Ward[]>([]);
  const [wardFilter, setWardFilter] = useState('');
  const [selectedProvinceId, setSelectedProvinceId] = useState<number | ''>(value?.provinceId ?? '');
  const [selectedWardId, setSelectedWardId] = useState<number | ''>(value?.wardId ?? '');
  const [street, setStreet] = useState(value?.street ?? '');

  // Giữ wardId cần bảo toàn qua lần load wards tiếp theo (khởi đầu từ value ban đầu)
  const wardToPreserve = useRef<number | ''>(value?.wardId ?? '');
  const isFirstMount = useRef(true);

  useEffect(() => {
    geographyService.getProvinces().then(setProvinces);
  }, []);

  // Sync state khi value prop thay đổi (edit địa chỉ khác mà không unmount)
  useEffect(() => {
    if (isFirstMount.current) {
      isFirstMount.current = false;
      return;
    }
    wardToPreserve.current = value?.wardId ?? '';
    setSelectedProvinceId(value?.provinceId ?? '');
    setSelectedWardId(value?.wardId ?? '');
    setStreet(value?.street ?? '');
  }, [value]);

  useEffect(() => {
    if (!selectedProvinceId) {
      setWards([]);
      return;
    }
    geographyService.getWardsByProvince(selectedProvinceId).then(setWards);
    // Nếu đang load từ value có sẵn (edit mode), giữ nguyên ward đã chọn
    if (wardToPreserve.current) {
      wardToPreserve.current = '';
    } else {
      setSelectedWardId('');
      setWardFilter('');
    }
  }, [selectedProvinceId]);

  const handleProvinceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const id = e.target.value ? Number(e.target.value) : '';
    setSelectedProvinceId(id);
  };

  const handleWardChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const id = e.target.value ? Number(e.target.value) : '';
    setSelectedWardId(id);
    if (id && selectedProvinceId) {
      const province = provinces.find((p) => p.id === selectedProvinceId);
      const ward = wards.find((w) => w.id === id);
      if (province && ward) {
        onChange({
          provinceId: province.id,
          provinceName: province.name,
          wardId: ward.id,
          wardName: ward.name,
          street,
        });
      }
    }
  };

  const handleStreetChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setStreet(e.target.value);
    if (selectedProvinceId && selectedWardId) {
      const province = provinces.find((p) => p.id === selectedProvinceId);
      const ward = wards.find((w) => w.id === selectedWardId);
      if (province && ward) {
        onChange({
          provinceId: province.id,
          provinceName: province.name,
          wardId: ward.id as number,
          wardName: ward.name,
          street: e.target.value,
        });
      }
    }
  };

  const filteredWards = wardFilter
    ? wards.filter((w) => w.name.toLowerCase().includes(wardFilter.toLowerCase()))
    : wards;

  return (
    <div className="space-y-3">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">{t('addressProvince')}</label>
        <select
          value={selectedProvinceId}
          onChange={handleProvinceChange}
          disabled={disabled}
          className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">{t('selectProvince')}</option>
          {provinces.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>
      </div>

      {selectedProvinceId && (
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">{t('addressWard')}</label>
          <input
            type="text"
            placeholder={t('searchWard')}
            value={wardFilter}
            onChange={(e) => setWardFilter(e.target.value)}
            disabled={disabled}
            className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm mb-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <select
            value={selectedWardId}
            onChange={handleWardChange}
            disabled={disabled}
            className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">{t('selectWard')}</option>
            {filteredWards.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name}
              </option>
            ))}
          </select>
        </div>
      )}

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">{t('addressStreet')}</label>
        <input
          type="text"
          placeholder={t('streetPlaceholder')}
          value={street}
          onChange={handleStreetChange}
          disabled={disabled}
          className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );
}
