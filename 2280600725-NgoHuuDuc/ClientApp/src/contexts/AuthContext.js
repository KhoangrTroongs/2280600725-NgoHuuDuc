import React, { createContext, useState, useEffect } from 'react';
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
        const { token, userId, userName } = response.data.data;
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
        const { token, userId, userName } = response.data.data;
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

  const value = {
    currentUser,
    loading,
    login,
    register,
    logout,
    updateProfile,
    isAuthenticated: !!token
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
