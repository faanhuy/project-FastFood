interface SkeletonProps {
  className?: string;
}

export function Skeleton({ className = '' }: SkeletonProps) {
  return <div className={`animate-pulse bg-gray-200 rounded ${className}`} />;
}

export function SkeletonProductCard() {
  return (
    <div className="bg-white rounded-2xl overflow-hidden shadow-sm border border-gray-100 p-3">
      <Skeleton className="w-full aspect-square mb-3" />
      <Skeleton className="h-4 w-3/4 mb-2" />
      <Skeleton className="h-3 w-1/2 mb-3" />
      <Skeleton className="h-8 w-full rounded-full" />
    </div>
  );
}

export function SkeletonCartSummary() {
  return (
    <div className="bg-white rounded-2xl shadow-sm p-6 space-y-3">
      <Skeleton className="h-5 w-1/3 mb-4" />
      {[1, 2, 3].map((i) => (
        <div key={i} className="flex items-center gap-3">
          <Skeleton className="w-10 h-10 rounded-lg shrink-0" />
          <div className="flex-1 space-y-1.5">
            <Skeleton className="h-3 w-3/4" />
            <Skeleton className="h-3 w-1/2" />
          </div>
          <Skeleton className="h-4 w-14 shrink-0" />
        </div>
      ))}
      <div className="border-t pt-3 space-y-2">
        <Skeleton className="h-3 w-full" />
        <Skeleton className="h-3 w-full" />
        <Skeleton className="h-5 w-full" />
      </div>
    </div>
  );
}

export function SkeletonAddressList() {
  return (
    <div className="space-y-2">
      {[1, 2].map((i) => (
        <div key={i} className="p-3 rounded-xl border-2 border-gray-100">
          <div className="flex items-start gap-3">
            <Skeleton className="w-4 h-4 rounded-full mt-0.5 shrink-0" />
            <div className="flex-1 space-y-1.5">
              <Skeleton className="h-3 w-1/2" />
              <Skeleton className="h-3 w-3/4" />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
