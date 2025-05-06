import React, { useContext, useState } from 'react';
import { Container, Row, Col, Form, Button, Card, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { CartContext } from '../../contexts/CartContext';
import { AuthContext } from '../../contexts/AuthContext';
import axios from 'axios';

const Checkout = () => {
  const { cart, loading: cartLoading, clearCart } = useContext(CartContext);
  const { currentUser } = useContext(AuthContext);
  const [shippingAddress, setShippingAddress] = useState(currentUser?.address || '');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    
    if (cart.items.length === 0) {
      setError('Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán.');
      return;
    }
    
    if (!shippingAddress.trim()) {
      setError('Vui lòng nhập địa chỉ giao hàng.');
      return;
    }
    
    setLoading(true);
    
    try {
      const response = await axios.post('/api/Orders', {
        shippingAddress,
        notes
      });
      
      if (response.data.isSuccess) {
        setSuccess('Đặt hàng thành công!');
        await clearCart();
        setTimeout(() => {
          navigate(`/orders/${response.data.data.id}`);
        }, 2000);
      } else {
        setError(response.data.message || 'Đặt hàng thất bại. Vui lòng thử lại.');
      }
    } catch (error) {
      console.error('Checkout error:', error);
      setError(error.response?.data?.message || 'Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  if (cartLoading) {
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
      <h1 className="mb-4">Thanh toán</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      
      {cart.items.length > 0 ? (
        <Row>
          <Col md={8}>
            <Card className="mb-4">
              <Card.Body>
                <h4 className="mb-3">Thông tin giao hàng</h4>
                <Form onSubmit={handleSubmit}>
                  <Form.Group className="mb-3" controlId="fullName">
                    <Form.Label>Họ và tên</Form.Label>
                    <Form.Control
                      type="text"
                      value={currentUser?.fullName || ''}
                      disabled
                    />
                  </Form.Group>
                  
                  <Form.Group className="mb-3" controlId="email">
                    <Form.Label>Email</Form.Label>
                    <Form.Control
                      type="email"
                      value={currentUser?.email || ''}
                      disabled
                    />
                  </Form.Group>
                  
                  <Form.Group className="mb-3" controlId="phoneNumber">
                    <Form.Label>Số điện thoại</Form.Label>
                    <Form.Control
                      type="tel"
                      value={currentUser?.phoneNumber || ''}
                      disabled
                    />
                  </Form.Group>
                  
                  <Form.Group className="mb-3" controlId="shippingAddress">
                    <Form.Label>Địa chỉ giao hàng <span className="text-danger">*</span></Form.Label>
                    <Form.Control
                      as="textarea"
                      rows={3}
                      value={shippingAddress}
                      onChange={(e) => setShippingAddress(e.target.value)}
                      required
                      placeholder="Nhập địa chỉ giao hàng của bạn"
                    />
                  </Form.Group>
                  
                  <Form.Group className="mb-3" controlId="notes">
                    <Form.Label>Ghi chú</Form.Label>
                    <Form.Control
                      as="textarea"
                      rows={3}
                      value={notes}
                      onChange={(e) => setNotes(e.target.value)}
                      placeholder="Nhập ghi chú cho đơn hàng (nếu có)"
                    />
                  </Form.Group>
                  
                  <Button 
                    variant="primary" 
                    type="submit" 
                    className="w-100" 
                    disabled={loading || success}
                  >
                    {loading ? 'Đang xử lý...' : 'Đặt hàng'}
                  </Button>
                </Form>
              </Card.Body>
            </Card>
          </Col>
          
          <Col md={4}>
            <Card>
              <Card.Body>
                <h4 className="mb-3">Đơn hàng của bạn</h4>
                
                {cart.items.map(item => (
                  <div key={item.id} className="d-flex justify-content-between mb-2">
                    <div>
                      {item.productName} <span className="text-muted">x{item.quantity}</span>
                    </div>
                    <div>{(item.price * item.quantity).toLocaleString('vi-VN')} đ</div>
                  </div>
                ))}
                
                <hr />
                
                <div className="d-flex justify-content-between mb-2">
                  <div><strong>Tổng cộng</strong></div>
                  <div><strong>{cart.totalPrice.toLocaleString('vi-VN')} đ</strong></div>
                </div>
                
                <div className="d-flex justify-content-between mb-2">
                  <div>Phí vận chuyển</div>
                  <div>Miễn phí</div>
                </div>
                
                <hr />
                
                <div className="d-flex justify-content-between">
                  <div><strong>Thành tiền</strong></div>
                  <div className="text-danger"><strong>{cart.totalPrice.toLocaleString('vi-VN')} đ</strong></div>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      ) : (
        <div className="text-center my-5">
          <h3>Giỏ hàng của bạn đang trống</h3>
          <p>Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán.</p>
          <Button onClick={() => navigate('/products')} variant="primary" className="mt-3">
            Tiếp tục mua sắm
          </Button>
        </div>
      )}
    </Container>
  );
};

export default Checkout;
