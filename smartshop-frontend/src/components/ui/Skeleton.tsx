import React from 'react';

export interface SkeletonProps {
  className?: string;
  width?: string | number;
  height?: string | number;
  rounded?: boolean;
  count?: number;
}

const formatDimension = (value?: string | number): string | undefined => {
  if (typeof value === 'number') return `${value}px`;
  return value;
};

export const Skeleton: React.FC<SkeletonProps> = ({
  className = '',
  width,
  height,
  rounded = false,
  count = 1,
}) => {
  const style: React.CSSProperties = {
    width: formatDimension(width),
    height: formatDimension(height),
  };

  const baseClass = 'animate-pulse bg-gray-200 dark:bg-gray-700';
  const roundedClass = rounded ? 'rounded-full' : 'rounded-md';

  return (
    <div className="space-y-2">
      {Array.from({ length: count }).map((_, i) => (
        <div
          key={i}
          style={style}
          className={`${baseClass} ${roundedClass} ${className}`}
        />
      ))}
    </div>
  );
};

export const SkeletonTable: React.FC<{
  rows?: number;
  cols?: number;
}> = ({ rows = 5, cols = 5 }) => {
  return (
    <div className="space-y-3">
      {Array.from({ length: rows }).map((_, rowIdx) => (
        <div key={rowIdx} className="flex gap-3">
          {Array.from({ length: cols }).map((_, colIdx) => (
            <Skeleton
              key={`${rowIdx}-${colIdx}`}
              width="100%"
              height={32}
              className="flex-1"
            />
          ))}
        </div>
      ))}
    </div>
  );
};

export const SkeletonCard: React.FC<{ count?: number }> = ({
  count = 3,
}) => {
  return (
    <div className="space-y-4">
      {Array.from({ length: count }).map((_, idx) => (
        <div key={idx} className="bg-white dark:bg-gray-900 rounded-lg p-4 space-y-3">
          <Skeleton width="100%" height={200} className="rounded-lg" />
          <Skeleton width="60%" height={20} />
          <Skeleton width="100%" height={16} />
          <Skeleton width="80%" height={16} />
        </div>
      ))}
    </div>
  );
};
