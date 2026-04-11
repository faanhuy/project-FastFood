import axios from 'axios';

/** Trả về error message đầu tiên từ Axios response, hoặc fallback nếu không parse được. */
export function getApiError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err) && err.response?.data) {
    const body = err.response.data as { errors?: string[]; message?: string };
    if (body.errors?.length) return body.errors[0];
    if (body.message) return body.message;
  }
  return fallback;
}

/** Trả về mảng errors từ Axios response — dùng cho setErrors(). */
export function getApiErrors(err: unknown, fallback: string): string[] {
  if (axios.isAxiosError(err) && err.response?.data) {
    const body = err.response.data as { errors?: string[]; message?: string };
    if (body.errors?.length) return body.errors;
    if (body.message) return [body.message];
  }
  return [fallback];
}
