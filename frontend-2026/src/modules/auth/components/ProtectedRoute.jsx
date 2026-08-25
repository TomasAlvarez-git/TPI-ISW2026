import { Navigate } from 'react-router-dom';
import useAuth from '../hook/useAuth';

function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth();
  const token = localStorage.getItem('token');
  const customer = localStorage.getItem('customerId');
  if (!isAuthenticated) {
    return <Navigate to='/login' />;
  }

  if (customer!= 'null') {
    return <Navigate to='/' />;
  }

  return children;
};

export default ProtectedRoute;
