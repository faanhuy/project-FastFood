import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import AdminLayout from '../../components/AdminLayout';
import { productImportService, type CsvPreviewResult } from '../../services/productImportService';
import { getApiError } from '../../utils/errorHandler';

export default function AdminCsvImportPage() {
  const { t } = useTranslation('admin');
  const { t: tToast } = useTranslation('toast');
  const { t: tCommon } = useTranslation('common');

  const [file, setFile] = useState<File | null>(null);
  const [previewResult, setPreviewResult] = useState<CsvPreviewResult | null>(null);
  const [step, setStep] = useState<'upload' | 'preview'>('upload');
  const [loading, setLoading] = useState(false);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      if (selectedFile.type !== 'text/csv' && !selectedFile.name.endsWith('.csv')) {
        toast.error(tToast('csvPreviewFailed'));
        return;
      }
      setFile(selectedFile);
      setPreviewResult(null);
      setStep('upload');
    }
  };

  const handlePreview = async () => {
    if (!file) {
      toast.error(tToast('csvPreviewFailed'));
      return;
    }

    setLoading(true);
    try {
      const result = await productImportService.previewCsv(file);
      setPreviewResult(result);
      setStep('preview');
    } catch (error: any) {
      toast.error(getApiError(error, tToast('csvPreviewFailed')));
    } finally {
      setLoading(false);
    }
  };

  const handleImport = async () => {
    if (!file) {
      toast.error(tToast('csvImportFailed'));
      return;
    }

    setLoading(true);
    try {
      const result = await productImportService.importCsv(file);
      toast.success(tToast('csvImportSuccess', { count: result.created }));
      if (result.failed > 0) {
        toast.error(tToast('importFailed'));
      }
      setFile(null);
      setPreviewResult(null);
      setStep('upload');
    } catch (error: any) {
      toast.error(getApiError(error, tToast('csvImportFailed')));
    } finally {
      setLoading(false);
    }
  };

  const downloadTemplate = () => {
    const csv = 'Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\nSample Product,Description here,100000,120000,{category-guid-here},,';
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'products_template.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <AdminLayout title={t('csvImportTitle')}>
      <div className="p-6">
        <h1 className="text-2xl font-bold mb-6">{t('csvImportTitle')}</h1>

        {step === 'upload' && (
          <div className="bg-white rounded-lg border border-gray-200 p-8 max-w-2xl">
            <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center mb-6">
              <input
                type="file"
                accept=".csv"
                onChange={handleFileChange}
                className="hidden"
                id="csv-file-input"
              />
              <label htmlFor="csv-file-input" className="block cursor-pointer">
                <p className="text-gray-600 mb-2">{t('csvSelectFile')}</p>
                {file && (
                  <p className="text-sm text-blue-600 font-medium">{file.name}</p>
                )}
              </label>
            </div>

            <div className="flex gap-3 justify-between mb-6">
              <button
                onClick={downloadTemplate}
                className="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg font-medium hover:bg-gray-300 transition"
              >
                {t('csvDownloadTemplate')}
              </button>
              <button
                onClick={handlePreview}
                disabled={!file || loading}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                {loading ? tCommon('loading') : t('csvPreview')}
              </button>
            </div>
          </div>
        )}

        {step === 'preview' && previewResult && (
          <div className="bg-white rounded-lg border border-gray-200 p-8">
            <div className="grid grid-cols-4 gap-4 mb-8">
              <div className="bg-blue-50 rounded-lg p-4">
                <p className="text-sm text-gray-600 mb-1">{t('csvTotalRows')}</p>
                <p className="text-2xl font-bold text-blue-600">{previewResult.totalRows}</p>
              </div>
              <div className="bg-green-50 rounded-lg p-4">
                <p className="text-sm text-gray-600 mb-1">{t('csvValidRows')}</p>
                <p className="text-2xl font-bold text-green-600">{previewResult.validRows}</p>
              </div>
              <div className="bg-red-50 rounded-lg p-4">
                <p className="text-sm text-gray-600 mb-1">{t('csvInvalidRows')}</p>
                <p className="text-2xl font-bold text-red-600">{previewResult.invalidRows}</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-sm text-gray-600 mb-1">{tCommon('success')}</p>
                <p className="text-2xl font-bold text-gray-600">
                  {((previewResult.validRows / previewResult.totalRows) * 100).toFixed(0)}%
                </p>
              </div>
            </div>

            {previewResult.errors.length > 0 && (
              <div className="mb-8">
                <h3 className="text-lg font-bold text-gray-900 mb-4">{tCommon('errors')}</h3>
                <div className="overflow-x-auto rounded-lg border border-gray-200">
                  <table className="w-full text-sm">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('csvRowNumber')}</th>
                        <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('csvFieldError')}</th>
                        <th className="px-4 py-3 text-left font-semibold text-gray-700">{tCommon('message')}</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                      {previewResult.errors.map((error, idx) => (
                        <tr key={idx} className="hover:bg-gray-50">
                          <td className="px-4 py-3 text-gray-800">{error.rowNumber}</td>
                          <td className="px-4 py-3 text-gray-800">{error.field}</td>
                          <td className="px-4 py-3 text-gray-800">{error.message}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            <div className="mb-8">
              <h3 className="text-lg font-bold text-gray-900 mb-4">{t('csvPreview')}</h3>
              <div className="overflow-x-auto rounded-lg border border-gray-200">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('csvRowNumber')}</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('productName')}</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('sellingPrice')}</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-700">{t('categoryGroup')}</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-700">{tCommon('status')}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {previewResult.preview.slice(0, 10).map((row, idx) => (
                      <tr key={idx} className={row.isValid ? 'bg-white hover:bg-gray-50' : 'bg-red-50 hover:bg-red-100'}>
                        <td className="px-4 py-3 text-gray-800">{row.rowNumber}</td>
                        <td className="px-4 py-3 text-gray-800">{row.name}</td>
                        <td className="px-4 py-3 text-gray-800">
                          {row.price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })}
                        </td>
                        <td className="px-4 py-3 text-gray-800">{row.categoryId}</td>
                        <td className="px-4 py-3">
                          <span
                            className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${
                              row.isValid
                                ? 'bg-green-100 text-green-700'
                                : 'bg-red-100 text-red-700'
                            }`}
                          >
                            {row.isValid ? t('valid') : t('invalid')}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {previewResult.preview.length > 10 && (
                <p className="text-sm text-gray-500 mt-2">
                  {tCommon('and')} {previewResult.preview.length - 10} {tCommon('more')}
                </p>
              )}
            </div>

            <div className="flex gap-3 justify-end">
              <button
                onClick={() => {
                  setFile(null);
                  setPreviewResult(null);
                  setStep('upload');
                }}
                className="px-6 py-2 bg-gray-200 text-gray-800 rounded-lg font-medium hover:bg-gray-300 transition"
              >
                {tCommon('back')}
              </button>
              <button
                onClick={handleImport}
                disabled={loading || previewResult.validRows === 0}
                className="px-6 py-2 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                {loading ? tCommon('loading') : t('csvImportConfirm')}
              </button>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
