import React, { useState, useCallback, useRef } from 'react';
import { Confirm } from '@/components/ui/Confirm';

interface ConfirmOptions {
  title: string;
  message: string;
  variant?: 'danger' | 'warning' | 'info';
  confirmLabel?: string;
  cancelLabel?: string;
}

interface UseConfirmReturn {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
  ConfirmDialog: React.FC;
}

export const useConfirm = (): UseConfirmReturn => {
  const [state, setState] = useState<{
    isOpen: boolean;
    options: ConfirmOptions | null;
  }>({
    isOpen: false,
    options: null,
  });

  const resolveRef = useRef<((value: boolean) => void) | null>(null);
  const [loading, setLoading] = useState(false);

  const confirm = useCallback(
    (options: ConfirmOptions): Promise<boolean> => {
      return new Promise((resolve) => {
        resolveRef.current = resolve;
        setState({
          isOpen: true,
          options,
        });
      });
    },
    []
  );

  const handleConfirm = useCallback(async () => {
    setLoading(true);
    try {
      if (resolveRef.current) {
        resolveRef.current(true);
      }
    } finally {
      setLoading(false);
      setState({ isOpen: false, options: null });
    }
  }, []);

  const handleCancel = useCallback(() => {
    if (resolveRef.current) {
      resolveRef.current(false);
    }
    setState({ isOpen: false, options: null });
  }, []);

  const ConfirmDialog = useCallback(() => {
    if (!state.options) return null;

    return (
      <Confirm
        isOpen={state.isOpen}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
        title={state.options.title}
        message={state.options.message}
        variant={state.options.variant}
        confirmLabel={state.options.confirmLabel}
        cancelLabel={state.options.cancelLabel}
        loading={loading}
      />
    );
  }, [state.isOpen, state.options, handleConfirm, handleCancel, loading]);

  return { confirm, ConfirmDialog };
};
