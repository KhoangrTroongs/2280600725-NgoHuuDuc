import React, { useContext, useState } from 'react';
import { Container, Table, Button, Image, Form, Alert, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { CartContext } from '../../contexts/CartContext';
import { AuthContext } from '../../contexts/AuthContext';

const Cart = () => {
  const { cart, loading, updateCartItem, removeCartItem, clearCart } = useContext(CartContext);
  const { isAuthenticated } = useContext(AuthContext);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  const handleQuantityChange = async (cartItemId, quantity) => {
    setError('');
    setSuccess('');
    
    if (quantity < 1) {
      return;
    }
    
    const result = await updateCartItem(cartItemId, quantity);
    if (!result.success) {
      setError(result.message || 'Không thể cập nhật số lượng. Vui lòng thử lại.');
    }
  };

  const handleRemoveItem = async (cartItemId) => {
    setError('');
    setSuccess('');
    
    if (window.confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
      const result = await removeCartItem(cartItemId);
      if (result.success) {
        setSuccess('Đã xóa sản phẩm khỏi giỏ hàng.');
      } else {
        setError(result.message || 'Không thể xóa sản phẩm. Vui lòng thử lại.');
      }
    }
  };

  const handleClearCart = async () => {
    setError('');
    setSuccess('');
    
    if (window.confirm('Bạn có chắc chắn muốn xóa tất cả sản phẩm khỏi giỏ hàng?')) {
      const result = await clearCart();
      if (result.success) {
        setSuccess('Đã xóa tất cả sản phẩm khỏi giỏ hàng.');
      } else {
        setError(result.message || 'Không thể xóa giỏ hàng. Vui lòng thử lại.');
      }
    }
  };

  const handleCheckout = () => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: '/checkout' } });
    } else {
      navigate('/checkout');
    }
  };

  if (loading) {
    return (
      <Container className="text-center my-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </Container>
    );
  }

  return (
    <Container className="py-4">
      <h1 className="mb-4">Giỏ hàng của bạn</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      
      {cart.items.length > 0 ? (
        <>
          <Table responsive>
            <thead>
              <tr>
                <th>Sản phẩm</th>
                <th>Giá</th>
                <th>Số lượng</th>
                <th>Tổng</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {cart.items.map(item => (
                <tr key={item.id}>
                  <td>
                    <div className="d-flex align-items-center">
                      <Image 
                        src={item.imageUrl || "https://via.placeholder.com/80x80?text=No+Image"} 
                        alt={item.productName} 
                        className="cart-item-image me-3" 
                      />
                      <div>
                        <Link to={`/products/${item.productId}`} className="text-decoration-none">
                          {item.productName}
                        </Link>
                      </div>
                    </div>
                  </td>
                  <td>{item.price.toLocaleString('vi-VN')} đ</td>
                  <td>
                    <div className="quantity-control">
                      <Button 
                        variant="outline-secondary" 
                        size="sm"
                        onClick={() => handleQuantityChange(item.id, item.quantity - 1)}
                        disabled={item.quantity <= 1}
                      >
                        -
                      </Button>
                      <Form.Control
                        type="number"
                        min="1"
                        value={item.quantity}
                        onChange={(e) => handleQuantityChange(item.id, parseInt(e.target.value) || 1)}
                        className="mx-2"
                        style={{ width: '60px' }}
                      />
                      <Button 
                        variant="outline-secondary" 
                        size="sm"
                        onClick={() => handleQuantityChange(item.id, item.quantity + 1)}
                      >
                        +
                      </Button>
                    </div>
                  </td>
                  <td>{(item.price * item.quantity).toLocaleString('vi-VN')} đ</td>
                  <td>
                    <Button 
                      variant="danger" 
                      size="sm"
                      onClick={() => handleRemoveItem(item.id)}
                    >
                      Xóa
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
          
          <div className="d-flex justify-content-between align-items-center mb-4">
            <Button variant="outline-danger" onClick={handleClearCart}>
              Xóa giỏ hàng
            </Button>
            <div className="text-end">
              <h4>Tổng cộng: {cart.totalPrice.toLocaleString('vi-VN')} đ</h4>
            </div>
          </div>
          
          <div className="d-flex justify-content-between">
            <Button as={Link} to="/products" variant="outline-primary">
              Tiếp tục mua sắm
            </Button>
            <Button variant="primary" onClick={handleCheckout}>
              Thanh toán
            </Button>
          </div>
        </>
      ) : (
        <div className="text-center my-5">
          <h3>Giỏ hàng của bạn đang trống</h3>
          <p>Hãy thêm sản phẩm vào giỏ hàng để tiếp tục.</p>
          <Button as={Link} to="/products" variant="primary" className="mt-3">
            Tiếp tục mua sắm
          </Button>
        </div>
      )}
    </Container>
  );
};

export default Cart;
