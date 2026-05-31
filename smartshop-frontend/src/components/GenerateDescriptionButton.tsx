import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FiCpu } from 'react-icons/fi';
import { aiService } from '../services/aiService';

interface GenerateDescriptionButtonProps {
  productName: string;
  categoryName: string;
  onGenerated: (description: string) => void;
}

export default function GenerateDescriptionButton({
  productName,
  categoryName,
  onGenerated,
}: GenerateDescriptionButtonProps) {
  const { t } = useTranslation(['toast']);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleGenerate = async () => {
    if (!productName.trim() || !categoryName.trim()) {
      setError(t('fillProductAndCategory'));
      return;
    }
    setGenerating(true);
    setError(null);
    try {
      const description = await aiService.generateDescription({
        productName: productName.trim(),
        categoryName: categoryName.trim(),
      });
      onGenerated(description);
    } catch {
      setError(t('descriptionGenerationFailed'));
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="flex flex-col gap-1">
      <button
        type="button"
        onClick={handleGenerate}
        disabled={generating}
        className="flex items-center gap-2 text-xs px-3 py-1.5 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-60 transition-colors"
      >
        <FiCpu size={13} />
        {generating ? t('descriptionGenerating') : t('generateDescriptionAI')}
      </button>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  );
}
