import { useEffect, useState } from 'react';

interface Props {
  remainingSeconds: number;
  onExpire?: () => void;
  className?: string;
}

export function CountdownTimer({ remainingSeconds, onExpire, className = '' }: Props) {
  const [seconds, setSeconds] = useState(remainingSeconds);

  useEffect(() => {
    setSeconds(remainingSeconds);
  }, [remainingSeconds]);

  useEffect(() => {
    if (seconds <= 0) {
      onExpire?.();
      return;
    }
    const id = setInterval(() => setSeconds((s) => {
      if (s <= 1) { clearInterval(id); onExpire?.(); return 0; }
      return s - 1;
    }), 1000);
    return () => clearInterval(id);
  }, [seconds, onExpire]);

  const d = Math.floor(seconds / 86400);
  const h = Math.floor((seconds % 86400) / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  const pad = (n: number) => String(n).padStart(2, '0');

  return (
    <span className={`font-mono font-bold ${className}`}>
      {d > 0 && `${d}d `}{pad(h)}:{pad(m)}:{pad(s)}
    </span>
  );
}
