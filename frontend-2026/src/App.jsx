import { createBrowserRouter, Outlet, RouterProvider } from 'react-router-dom';
import { AuthProvider } from './modules/auth/context/AuthProvider';
import LoginPage from './modules/auth/pages/LoginPage';
import Dashboard from './modules/templates/components/Dashboard';
import DashboardGeneral from './modules/templates/components/DashboardGeneral';
import ProtectedRoute from './modules/auth/components/ProtectedRoute';
import ListOrdersPage from './modules/orders/pages/ListOrdersPage';
import Home from './modules/home/pages/Home';
import ListProductsPage from './modules/products/pages/ListProductsPage';
import ListGeneralProductsPage from './modules/products/pages/ListGeneralProductsPage';
import CreateProductPage from './modules/products/pages/CreateProductPage';
import RegisterPage from './modules/auth/pages/RegisterPage';
import ShoppingCartPage from './modules/products/pages/ShoppingCartPage';

function App() {
  const router = createBrowserRouter([
    {
      path: '/',
      element: <DashboardGeneral />,
      children: [
        {
          path: '',
          element: <ListGeneralProductsPage />,
        },
        {
          path: '/cart',
          element: <ShoppingCartPage />,
        },
      ],
    },
    {
      path: '/login',
      element: <LoginPage />,
    },
    {
      path: '/signup',
      element: (
        <ProtectedRoute>
          <RegisterPage />
        </ProtectedRoute>
      ),
    },
    {
      path: '/admin',
      element: (
        <ProtectedRoute>
          <Dashboard />
        </ProtectedRoute>
      ),
      children: [
        {
          path: '/admin/home',
          element: <Home />,
        },
        {
          path: '/admin/products',
          element: <ListProductsPage />,
        },
        {
          path: '/admin/products/create',
          element: <CreateProductPage />,
        },
        {
          path: '/admin/orders',
          element: <ListOrdersPage />,
        },
      ],
    },
  ]);

  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}

export default App;
