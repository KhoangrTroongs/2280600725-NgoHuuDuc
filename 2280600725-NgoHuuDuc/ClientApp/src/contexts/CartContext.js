import React, { createContext, useState, useEffect, useContext } from 'react';
import axios from 'axios';
import { AuthContext } from './AuthContext';

export const CartContext = createContext();

export const CartProvider = ({ children }) => {
  const [cart, setCart] = useState({ items: [], totalPrice: 0 });
  const [loading, setLoading] = useState(false);
  const { isAuthenticated } = useContext(AuthContext);

  useEffect(() => {
    if (isAuthenticated) {
      fetchCart();
    } else {
      // If not authenticated, use local storage cart
      const localCart = localStorage.getItem('cart');
      if (localCart) {
        setCart(JSON.parse(localCart));
      } else {
        setCart({ items: [], totalPrice: 0 });
      }
    }
  }, [isAuthenticated]);

  const fetchCart = async () => {
    setLoading(true);
    try {
      const response = await axios.get('/api/Cart');
      if (response.data.isSuccess) {
        setCart(response.data.data);
      }
    } catch (error) {
      console.error('Error fetching cart:', error);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = async (productId, quantity) => {
    setLoading(true);
    try {
      if (isAuthenticated) {
        // Add to server cart if authenticated
        const response = await axios.post('/api/Cart', {
          productId,
          quantity
        });
        if (response.data.isSuccess) {
          setCart(response.data.data);
          return { success: true };
        } else {
          return { success: false, message: response.data.message };
        }
      } else {
        // Add to local cart if not authenticated
        // First, fetch product details
        const productResponse = await axios.get(`/api/Products/${productId}`);
        if (productResponse.data.isSuccess) {
          const product = productResponse.data.data;
          
          // Check if product is already in cart
          const existingItemIndex = cart.items.findIndex(item => item.productId === productId);
          
          let newItems;
          if (existingItemIndex >= 0) {
            // Update quantity if product already in cart
            newItems = [...cart.items];
            newItems[existingItemIndex].quantity += quantity;
          } else {
            // Add new item if product not in cart
            newItems = [...cart.items, {
              productId: product.id,
              productName: product.name,
              price: product.price,
              quantity: quantity,
              imageUrl: product.imageUrl
            }];
          }
          
          // Calculate new total price
          const newTotalPrice = newItems.reduce((total, item) => total + (item.price * item.quantity), 0);
          
          const newCart = {
            items: newItems,
            totalPrice: newTotalPrice
          };
          
          setCart(newCart);
          localStorage.setItem('cart', JSON.stringify(newCart));
          return { success: true };
        } else {
          return { success: false, message: 'Không thể thêm sản phẩm vào giỏ hàng.' };
        }
      }
    } catch (error) {
      console.error('Error adding to cart:', error);
      return { 
        success: false, 
        message: error.response?.data?.message || 'Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.' 
      };
    } finally {
      setLoading(false);
    }
  };

  const updateCartItem = async (cartItemId, quantity) => {
    setLoading(true);
    try {
      if (isAuthenticated) {
        // Update server cart if authenticated
        const response = await axios.put('/api/Cart', {
          cartItemId,
          quantity
        });
        if (response.data.isSuccess) {
          setCart(response.data.data);
          return { success: true };
        } else {
          return { success: false, message: response.data.message };
        }
      } else {
        // Update local cart if not authenticated
        const newItems = cart.items.map(item => 
          item.id === cartItemId ? { ...item, quantity } : item
        );
        
        // Remove item if quantity is 0
        const filteredItems = newItems.filter(item => item.quantity > 0);
        
        // Calculate new total price
        const newTotalPrice = filteredItems.reduce((total, item) => total + (item.price * item.quantity), 0);
        
        const newCart = {
          items: filteredItems,
          totalPrice: newTotalPrice
        };
        
        setCart(newCart);
        localStorage.setItem('cart', JSON.stringify(newCart));
        return { success: true };
      }
    } catch (error) {
      console.error('Error updating cart item:', error);
      return { 
        success: false, 
        message: error.response?.data?.message || 'Không thể cập nhật giỏ hàng. Vui lòng thử lại.' 
      };
    } finally {
      setLoading(false);
    }
  };

  const removeCartItem = async (cartItemId) => {
    setLoading(true);
    try {
      if (isAuthenticated) {
        // Remove from server cart if authenticated
        const response = await axios.delete(`/api/Cart/${cartItemId}`);
        if (response.data.isSuccess) {
          fetchCart(); // Refresh cart after removal
          return { success: true };
        } else {
          return { success: false, message: response.data.message };
        }
      } else {
        // Remove from local cart if not authenticated
        const newItems = cart.items.filter(item => item.id !== cartItemId);
        
        // Calculate new total price
        const newTotalPrice = newItems.reduce((total, item) => total + (item.price * item.quantity), 0);
        
        const newCart = {
          items: newItems,
          totalPrice: newTotalPrice
        };
        
        setCart(newCart);
        localStorage.setItem('cart', JSON.stringify(newCart));
        return { success: true };
      }
    } catch (error) {
      console.error('Error removing cart item:', error);
      return { 
        success: false, 
        message: error.response?.data?.message || 'Không thể xóa sản phẩm khỏi giỏ hàng. Vui lòng thử lại.' 
      };
    } finally {
      setLoading(false);
    }
  };

  const clearCart = async () => {
    setLoading(true);
    try {
      if (isAuthenticated) {
        // Clear server cart if authenticated
        const response = await axios.delete('/api/Cart');
        if (response.data.isSuccess) {
          setCart({ items: [], totalPrice: 0 });
          return { success: true };
        } else {
          return { success: false, message: response.data.message };
        }
      } else {
        // Clear local cart if not authenticated
        setCart({ items: [], totalPrice: 0 });
        localStorage.removeItem('cart');
        return { success: true };
      }
    } catch (error) {
      console.error('Error clearing cart:', error);
      return { 
        success: false, 
        message: error.response?.data?.message || 'Không thể xóa giỏ hàng. Vui lòng thử lại.' 
      };
    } finally {
      setLoading(false);
    }
  };

  const value = {
    cart,
    loading,
    addToCart,
    updateCartItem,
    removeCartItem,
    clearCart,
    refreshCart: fetchCart
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};
