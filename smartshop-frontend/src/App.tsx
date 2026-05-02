import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import router from './router';
import GlobalSignalR from './components/GlobalSignalR';

export default function App() {
  return (
    <>
      <GlobalSignalR />
      <RouterProvider router={router} />
      <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
    </>
  );
}
