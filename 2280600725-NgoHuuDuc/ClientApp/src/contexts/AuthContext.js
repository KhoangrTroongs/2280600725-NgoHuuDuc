import React, { createContext, useState, useEffect, useMemo } from 'react';
import axios from 'axios';

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (token) {
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      fetchCurrentUser();
    } else {
      setLoading(false);
    }
  }, [token]);

  const fetchCurrentUser = async () => {
    try {
      const response = await axios.get('/api/Users/current');
      if (response.data.isSuccess) {
        setCurrentUser(response.data.data);
      } else {
        logout();
      }
    } catch (error) {
      console.error('Error fetching current user:', error);
      logout();
    } finally {
      setLoading(false);
    }
  };

  const login = async (email, password, rememberMe) => {
    try {
      const response = await axios.post('/api/Auth/login', {
        email,
        password,
        rememberMe
      });

      if (response.data.isSuccess) {
        const { token } = response.data.data;
        localStorage.setItem('token', token);
        setToken(token);
        return { success: true };
      } else {
        return { success: false, message: response.data.message };
      }
    } catch (error) {
      console.error('Login error:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Đăng nhập thất bại. Vui lòng thử lại.'
      };
    }
  };

  const register = async (userData) => {
    try {
      const response = await axios.post('/api/Auth/register', userData);

      if (response.data.isSuccess) {
        const { token } = response.data.data;
        localStorage.setItem('token', token);
        setToken(token);
        return { success: true };
      } else {
        return { success: false, message: response.data.message };
      }
    } catch (error) {
      console.error('Register error:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Đăng ký thất bại. Vui lòng thử lại.'
      };
    }
  };

  const logout = async () => {
    try {
      if (token) {
        await axios.post('/api/Auth/logout');
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('token');
      delete axios.defaults.headers.common['Authorization'];
      setToken(null);
      setCurrentUser(null);
    }
  };

  const updateProfile = async (userId, userData) => {
    try {
      const response = await axios.put(`/api/Users/${userId}`, userData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      if (response.data.isSuccess) {
        setCurrentUser(response.data.data);
        return { success: true };
      } else {
        return { success: false, message: response.data.message };
      }
    } catch (error) {
      console.error('Update profile error:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Cập nhật thông tin thất bại. Vui lòng thử lại.'
      };
    }
  };

  const externalLogin = async (provider, providerKey, email, name, photoUrl) => {
    try {
      const response = await axios.post('/api/Auth/external-login', {
        provider,
        providerKey,
        email,
        name,
        photoUrl
      });

      if (response.data.isSuccess) {
        const { token } = response.data.data;
        localStorage.setItem('token', token);
        setToken(token);
        return { success: true };
      } else {
        return { success: false, message: response.data.message };
      }
    } catch (error) {
      console.error('External login error:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Đăng nhập bằng tài khoản ngoài thất bại. Vui lòng thử lại.'
      };
    }
  };

  const getExternalLoginUrl = async (provider) => {
    try {
      const response = await axios.get(`/api/Auth/external-login-token/${provider}`);
      if (response.data.isSuccess) {
        return { success: true, url: response.data.data };
      } else {
        return { success: false, message: response.data.message };
      }
    } catch (error) {
      console.error('Get external login URL error:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Không thể lấy URL đăng nhập bằng tài khoản ngoài.'
      };
    }
  };

  const value = useMemo(() => ({
    currentUser,
    loading,
    login,
    register,
    logout,
    updateProfile,
    externalLogin,
    getExternalLoginUrl,
    isAuthenticated: !!token
  }), [currentUser, loading, token]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
