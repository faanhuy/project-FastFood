import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import router from './router';
import GlobalSignalR from './components/GlobalSignalR';
import ChatWidget from './components/ChatWidget';

export default function App() {
  return (
    <>
      <GlobalSignalR />
      <RouterProvider router={router} />
      <ChatWidget />
      <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
    </>
  );
}
