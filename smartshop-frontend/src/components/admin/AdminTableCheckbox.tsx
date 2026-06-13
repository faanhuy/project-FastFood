import { useRef, useEffect } from 'react';

interface AdminTableCheckboxProps {
  checked: boolean;
  indeterminate?: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
}

export default function AdminTableCheckbox({
  checked,
  indeterminate = false,
  onChange,
  disabled = false,
}: AdminTableCheckboxProps) {
  const checkboxRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.indeterminate = indeterminate;
    }
  }, [indeterminate]);

  return (
    <input
      ref={checkboxRef}
      type="checkbox"
      checked={checked}
      onChange={e => onChange(e.target.checked)}
      disabled={disabled}
      className="w-4 h-4 accent-rose-600 rounded border-gray-300 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
    />
  );
}
