import { FiArrowUp, FiArrowDown } from 'react-icons/fi';

interface SortableColumnHeaderProps {
  label: string;
  field: string;
  currentSortBy: string;
  currentSortDirection: 'asc' | 'desc';
  onSort: (field: string, direction: 'asc' | 'desc') => void;
}

export default function SortableColumnHeader({
  label,
  field,
  currentSortBy,
  currentSortDirection,
  onSort,
}: SortableColumnHeaderProps) {
  const isActive = currentSortBy === field;

  const handleClick = () => {
    if (isActive) {
      // Toggle direction
      const newDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
      onSort(field, newDirection);
    } else {
      // Default to ascending
      onSort(field, 'asc');
    }
  };

  return (
    <button
      onClick={handleClick}
      className={`flex items-center gap-1 font-medium hover:text-gray-900 transition-colors ${
        isActive ? 'text-gray-900' : 'text-gray-600'
      }`}
    >
      <span>{label}</span>
      {isActive ? (
        currentSortDirection === 'asc' ? (
          <FiArrowUp size={14} className="flex-shrink-0" />
        ) : (
          <FiArrowDown size={14} className="flex-shrink-0" />
        )
      ) : (
        <span className="flex flex-col opacity-40" style={{ fontSize: 8, lineHeight: 1, gap: 1 }}>
          <FiArrowUp size={8} />
          <FiArrowDown size={8} />
        </span>
      )}
    </button>
  );
}
