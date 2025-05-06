import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Badge, Button, Spinner, Alert, Image } from 'react-bootstrap';
import { Link, useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

const OrderDetail = () => {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    fetchOrder();
  }, [id]);

  const fetchOrder = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await axios.get(`/api/Orders/${id}`);
      if (response.data.isSuccess) {
        setOrder(response.data.data);
      } else {
        setError(response.data.message || 'Không thể tải thông tin đơn hàng.');
      }
    } catch (error) {
      console.error('Error fetching order:', error);
      setError('Đã xảy ra lỗi khi tải thông tin đơn hàng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status) => {
    switch (status) {
      case 0: // Pending
        return <Badge bg="warning">Chờ xác nhận</Badge>;
      case 1: // Processing
        return <Badge bg="info">Đang xử lý</Badge>;
      case 2: // Shipped
        return <Badge bg="primary">Đang giao hàng</Badge>;
      case 3: // Delivered
        return <Badge bg="success">Đã giao hàng</Badge>;
      case 4: // Cancelled
        return <Badge bg="danger">Đã hủy</Badge>;
      default:
        return <Badge bg="secondary">Không xác định</Badge>;
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
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

  if (!order) {
    return (
      <Container className="text-center my-5">
        <h2>Không tìm thấy đơn hàng</h2>
        <p>Đơn hàng không tồn tại hoặc bạn không có quyền xem đơn hàng này.</p>
        <Button as={Link} to="/orders" variant="primary">
          Quay lại lịch sử đơn hàng
        </Button>
      </Container>
    );
  }

  return (
    <Container className="py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Chi tiết đơn hàng #{order.id}</h1>
        <Button variant="outline-primary" onClick={() => navigate('/orders')}>
          Quay lại
        </Button>
      </div>
      
      {error && <Alert variant="danger">{error}</Alert>}
      
      <Row>
        <Col md={8}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Sản phẩm</h5>
            </Card.Header>
            <Card.Body>
              <Table responsive>
                <thead>
                  <tr>
                    <th>Sản phẩm</th>
                    <th>Giá</th>
                    <th>Số lượng</th>
                    <th>Tổng</th>
                  </tr>
                </thead>
                <tbody>
                  {order.orderDetails.map(item => (
                    <tr key={item.id}>
                      <td>
                        <div className="d-flex align-items-center">
                          <Image 
                            src={item.productImageUrl || "https://via.placeholder.com/80x80?text=No+Image"} 
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
                      <td>{item.quantity}</td>
                      <td>{(item.price * item.quantity).toLocaleString('vi-VN')} đ</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={4}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Thông tin đơn hàng</h5>
            </Card.Header>
            <Card.Body>
              <p><strong>Mã đơn hàng:</strong> #{order.id}</p>
              <p><strong>Ngày đặt:</strong> {formatDate(order.orderDate)}</p>
              <p><strong>Trạng thái:</strong> {getStatusBadge(order.status)}</p>
              <p><strong>Tổng tiền:</strong> {order.totalPrice.toLocaleString('vi-VN')} đ</p>
            </Card.Body>
          </Card>
          
          <Card>
            <Card.Header>
              <h5 className="mb-0">Thông tin giao hàng</h5>
            </Card.Header>
            <Card.Body>
              <p><strong>Người nhận:</strong> {order.userName}</p>
              <p><strong>Địa chỉ:</strong> {order.shippingAddress}</p>
              {order.notes && (
                <p><strong>Ghi chú:</strong> {order.notes}</p>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default OrderDetail;
