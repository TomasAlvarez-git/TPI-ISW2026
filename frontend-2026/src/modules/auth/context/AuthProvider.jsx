import { createContext, useState } from 'react';
import { login } from '../services/login';

const AuthContext = createContext();

function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(() => {
    const token = localStorage.getItem('token');

    return Boolean(token);
  });

  const signOut = () => {
    localStorage.clear();
    setIsAuthenticated(false);
  };

  const signIn = async (username, password) => {
    const { data, error } = await login(username, password);

    if (error) {
      return { error };
    }

    localStorage.setItem('token', data.token);
    localStorage.setItem('customerId', data.customerId);
    localStorage.setItem('customerEmail', data.customerEmail);
    localStorage.setItem('userName', data.userName);
    setIsAuthenticated(true);
    return { error: null };
  };

  return (
    <AuthContext.Provider
      value={ {
        isAuthenticated,
        signIn,
        signOut,
      } }
    >
      {children}
    </AuthContext.Provider>
  );
};

export {
  AuthProvider,
  AuthContext,
};
