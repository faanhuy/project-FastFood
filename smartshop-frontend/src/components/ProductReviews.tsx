import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiTrash2, FiImage } from 'react-icons/fi';
import { useAuthStore } from '../store/authStore';
import { reviewService } from '../services/reviewService';
import { getApiError } from '../utils/errorHandler';
import { formatDate } from '../utils/formatters';
import type { ReviewDto } from '../types/review';
import StarRating from './common/StarRating';
import Pagination from './common/Pagination';

interface Props {
  productId: string;
}

const PAGE_SIZE = 5;

export default function ProductReviews({ productId }: Props) {
  const { t } = useTranslation(['product', 'toast', 'common']);
  const { isAuthenticated, user } = useAuthStore();

  const [reviews,    setReviews]    = useState<ReviewDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page,       setPage]       = useState(1);
  const [loading,    setLoading]    = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [avgRating,  setAvgRating]  = useState(0);

  const [rating,  setRating]  = useState(5);
  const [comment, setComment] = useState('');
  const [imageFiles, setImageFiles] = useState<File[]>([]);
  const [imagePreviews, setImagePreviews] = useState<string[]>([]);
  const [uploadingImages, setUploadingImages] = useState(false);

  const loadReviews = async (p: number) => {
    setLoading(true);
    try {
      const result = await reviewService.getProductReviews(productId, p, PAGE_SIZE);
      setReviews(result.items);
      setTotalCount(result.totalCount);
    } catch {
      // silent — reviews không critical
    } finally {
      setLoading(false);
    }
  };

  const loadAvgRating = async () => {
    try {
      const all = await reviewService.getProductReviews(productId, 1, 1000);
      if (all.items.length > 0) {
        const avg = all.items.reduce((s, r) => s + r.rating, 0) / all.items.length;
        setAvgRating(parseFloat(avg.toFixed(1)));
      } else {
        setAvgRating(0);
      }
    } catch {
      // silent
    }
  };

  useEffect(() => { loadReviews(page); }, [productId, page]);
  useEffect(() => { loadAvgRating(); }, [productId]);

  // userId trong ReviewDto là Guid; store chỉ có email → so sánh bằng userFullName
  const myReview = reviews.find(
    (r) => r.userFullName === `${user?.firstName} ${user?.lastName}`.trim()
  );

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files ?? []);
    const remaining = 5 - imageFiles.length;
    const toAdd = selected.slice(0, remaining);
    setImageFiles(prev => [...prev, ...toAdd]);
    toAdd.forEach(f => {
      const reader = new FileReader();
      reader.onload = (ev) => setImagePreviews(prev => [...prev, ev.target?.result as string]);
      reader.readAsDataURL(f);
    });
    e.target.value = '';
  };

  const removeImage = (index: number) => {
    setImageFiles(prev => prev.filter((_, i) => i !== index));
    setImagePreviews(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!comment.trim()) return;
    setSubmitting(true);
    try {
      const review = await reviewService.addReview({ productId, rating, comment: comment.trim() });

      // Upload images if any
      if (imageFiles.length > 0) {
        setUploadingImages(true);
        try {
          await reviewService.uploadReviewImages(review.id, imageFiles);
        } catch (imgErr) {
          toast.error(getApiError(imgErr, t('toast:imageUploadPartialFail')));
        } finally {
          setUploadingImages(false);
        }
      }

      setComment('');
      setRating(5);
      setImageFiles([]);
      setImagePreviews([]);
      setPage(1);
      await Promise.all([loadReviews(1), loadAvgRating()]);
      toast.success(t('toast:reviewSubmitted'));
    } catch (err) {
      toast.error(getApiError(err, t('toast:reviewSubmitFailed')));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (reviewId: string) => {
    setDeletingId(reviewId);
    try {
      await reviewService.deleteReview(reviewId);
      setPage(1);
      await Promise.all([loadReviews(1), loadAvgRating()]);
      toast.success(t('toast:reviewDeleted'));
    } catch {
      toast.error(t('toast:reviewDeleteFailed'));
    } finally {
      setDeletingId(null);
    }
  };

  const totalPages = Math.ceil(totalCount / PAGE_SIZE) || 1;

  return (
    <section className="mt-8 bg-white rounded-2xl shadow-sm p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <h2 className="text-base font-semibold text-gray-800">{t('product:reviewsTitle')}</h2>
          {totalCount > 0 && (
            <span className="text-xs text-gray-400 bg-gray-100 px-2 py-0.5 rounded-full">
              {t('product:reviewCount', { count: totalCount })}
            </span>
          )}
        </div>
        {avgRating > 0 && (
          <div className="flex items-center gap-1.5">
            <StarRating value={Math.round(avgRating)} />
            <span className="text-sm font-semibold text-gray-700">{avgRating.toFixed(1)}</span>
          </div>
        )}
      </div>

      {/* Form gửi đánh giá */}
      {isAuthenticated && !myReview && (
        <form onSubmit={handleSubmit} className="mb-6 p-4 bg-gray-50 rounded-xl border border-gray-100">
          <p className="text-sm font-medium text-gray-700 mb-3">{t('product:shareYourThoughts')}</p>

          <div className="flex items-center gap-2 mb-3">
            <span className="text-xs text-gray-500">{t('product:ratingLabel')}</span>
            <StarRating value={rating} onChange={setRating} />
            <span className="text-xs text-gray-400 ml-1">{rating}/5</span>
          </div>

          <textarea
            required
            rows={3}
            placeholder={t('product:reviewPlaceholder')}
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-rose-300 bg-white"
          />

          <div className="mt-2">
            <label className="flex items-center gap-2 cursor-pointer text-xs text-gray-500 hover:text-rose-500 transition-colors w-fit">
              <FiImage size={14} />
              <span>{t('product:addImages', { count: imageFiles.length })}</span>
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                multiple
                className="hidden"
                onChange={handleImageChange}
                disabled={submitting || uploadingImages}
              />
            </label>
            {imagePreviews.length > 0 && (
              <div className="flex flex-wrap gap-2 mt-2">
                {imagePreviews.map((src, i) => (
                  <div key={i} className="relative w-16 h-16">
                    <img src={src} alt={`Ảnh ${i + 1}`} className="w-full h-full object-cover rounded-lg border border-gray-200" />
                    <button
                      type="button"
                      onClick={() => removeImage(i)}
                      disabled={submitting || uploadingImages}
                      className="absolute -top-1 -right-1 bg-red-500 text-white rounded-full w-4 h-4 flex items-center justify-center text-[10px] leading-none hover:bg-red-600 disabled:opacity-50 transition-colors"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="flex justify-end mt-3">
            <button
              type="submit"
              disabled={submitting || uploadingImages || !comment.trim()}
              className="px-4 py-2 bg-rose-600 text-white text-sm rounded-lg hover:bg-rose-700 disabled:opacity-50 transition-colors"
            >
              {submitting ? t('product:sendingReview') : uploadingImages ? t('product:uploadingImages') : t('product:submitReview')}
            </button>
          </div>
        </form>
      )}

      {!isAuthenticated && (
        <p className="text-sm text-gray-400 mb-5 italic">
          <a href="/login" className="text-rose-600 hover:underline">{t('common:loggedInAs')}</a> {t('product:loginToReview')}
        </p>
      )}

      {/* Danh sách đánh giá */}
      {loading ? (
        <div className="text-center py-8 text-gray-400 text-sm">{t('product:loadingReviews')}</div>
      ) : reviews.length === 0 ? (
        <p className="text-center py-8 text-gray-400 text-sm">
          {t('product:noReviewsYet')}
        </p>
      ) : (
        <div className="divide-y divide-gray-100">
          {reviews.map((review) => {
            const isOwn = review.userFullName === `${user?.firstName} ${user?.lastName}`.trim();
            const isAdmin = user?.role === 'Admin';

            return (
              <div key={review.id} className="py-4 flex gap-3">
                <div className="w-9 h-9 rounded-full bg-rose-100 text-rose-600 flex items-center justify-center text-sm font-bold shrink-0">
                  {review.userFullName.charAt(0).toUpperCase()}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="text-sm font-medium text-gray-800">{review.userFullName}</span>
                      {isOwn && (
                        <span className="text-[10px] bg-rose-100 text-rose-600 px-1.5 py-0.5 rounded font-medium">
                          {t('product:youBadge')}
                        </span>
                      )}
                      <StarRating value={review.rating} />
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      <span className="text-xs text-gray-400">{formatDate(review.createdAt)}</span>
                      {(isOwn || isAdmin) && (
                        <button
                          onClick={() => handleDelete(review.id)}
                          disabled={deletingId === review.id}
                          className="text-gray-300 hover:text-red-500 transition-colors disabled:opacity-40"
                          title={t('product:deleteReview')}
                        >
                          <FiTrash2 size={14} />
                        </button>
                      )}
                    </div>
                  </div>
                  <p className="mt-1.5 text-sm text-gray-600 leading-relaxed">{review.comment}</p>
                  {review.imageUrls?.length > 0 && (
                    <div className="flex flex-wrap gap-2 mt-2">
                      {review.imageUrls.map((url, i) => (
                        <img
                          key={i}
                          src={url}
                          alt={`Ảnh ${i + 1}`}
                          className="w-16 h-16 object-cover rounded-lg border border-gray-100 cursor-pointer hover:opacity-90 transition-opacity"
                          onClick={() => window.open(url, '_blank')}
                        />
                      ))}
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </section>
  );
}
