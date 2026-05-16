import { useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { FiUploadCloud, FiX } from 'react-icons/fi';
import { imageService } from '../../services/imageService';
import { getImageUrl } from '../../utils/imageUrl';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
const MAX_SIZE_MB = 5;

interface ImageUploadFieldProps {
  currentUrl: string | null | undefined;
  onUploaded: (url: string | null) => void;
  uploadFn?: (file: File) => Promise<string>; // NEW — default: imageService.upload
}

export default function ImageUploadField({ currentUrl, onUploaded, uploadFn }: ImageUploadFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(getImageUrl(currentUrl) || null);
  const [uploading, setUploading] = useState(false);

  const handleFile = async (file: File) => {
    if (!ALLOWED_TYPES.includes(file.type)) {
      toast.error('Chỉ chấp nhận JPG, PNG, WebP, GIF');
      return;
    }
    if (file.size > MAX_SIZE_MB * 1024 * 1024) {
      toast.error(`Kích thước không được vượt quá ${MAX_SIZE_MB} MB`);
      return;
    }
    setPreview(URL.createObjectURL(file));
    setUploading(true);
    try {
      const upload = uploadFn ?? imageService.upload;
      const url = await upload(file);
      onUploaded(url);
      toast.success('Tải ảnh lên thành công');
    } catch {
      toast.error('Tải ảnh lên thất bại');
      setPreview(getImageUrl(currentUrl) || null);
    } finally {
      setUploading(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  };

  const handleRemove = () => {
    setPreview(null);
    onUploaded(null);
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">Hình ảnh</label>
      <input
        ref={inputRef}
        type="file"
        accept={ALLOWED_TYPES.join(',')}
        className="hidden"
        onChange={(e) => { const f = e.target.files?.[0]; if (f) handleFile(f); }}
      />

      {preview ? (
        <div className="relative w-full h-36 rounded-xl border border-gray-200 overflow-hidden bg-gray-50 group">
          <img src={preview} alt="preview" className="w-full h-full object-contain" />
          {uploading && (
            <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
              <span className="text-white text-xs font-medium animate-pulse">Đang tải lên...</span>
            </div>
          )}
          {!uploading && (
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center gap-2 opacity-0 group-hover:opacity-100">
              <button
                type="button"
                onClick={() => inputRef.current?.click()}
                className="bg-white text-gray-700 text-xs px-3 py-1.5 rounded-lg shadow hover:bg-gray-100"
              >
                Đổi ảnh
              </button>
              <button
                type="button"
                onClick={handleRemove}
                className="bg-red-500 text-white text-xs px-2 py-1.5 rounded-lg shadow hover:bg-red-600"
              >
                <FiX size={12} />
              </button>
            </div>
          )}
        </div>
      ) : (
        <div
          onDrop={handleDrop}
          onDragOver={(e) => e.preventDefault()}
          onClick={() => inputRef.current?.click()}
          className="w-full h-28 rounded-xl border-2 border-dashed border-gray-300 hover:border-rose-400 bg-gray-50 hover:bg-rose-50 transition-colors cursor-pointer flex flex-col items-center justify-center gap-1.5"
        >
          <FiUploadCloud size={22} className="text-gray-400" />
          <p className="text-xs text-gray-500">Kéo thả hoặc <span className="text-rose-600 font-medium">chọn file</span></p>
          <p className="text-[10px] text-gray-400">JPG, PNG, WebP, GIF · tối đa {MAX_SIZE_MB} MB</p>
        </div>
      )}
    </div>
  );
}
