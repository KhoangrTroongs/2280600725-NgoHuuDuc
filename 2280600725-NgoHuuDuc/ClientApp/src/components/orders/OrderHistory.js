import React, { useState, useEffect } from 'react';
import { Container, Table, Badge, Button, Spinner, Alert } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import axios from 'axios';

const OrderHistory = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await axios.get('/api/Orders/my-orders');
      if (response.data.isSuccess) {
        setOrders(response.data.data);
      } else {
        setError(response.data.message || 'Không thể tải lịch sử đơn hàng.');
      }
    } catch (error) {
      console.error('Error fetching orders:', error);
      setError('Đã xảy ra lỗi khi tải lịch sử đơn hàng. Vui lòng thử lại sau.');
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

  return (
    <Container className="py-4">
      <h1 className="mb-4">Lịch sử đơn hàng</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      
      {orders.length > 0 ? (
        <Table responsive striped hover>
          <thead>
            <tr>
              <th>Mã đơn hàng</th>
              <th>Ngày đặt</th>
              <th>Tổng tiền</th>
              <th>Trạng thái</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {orders.map(order => (
              <tr key={order.id}>
                <td>#{order.id}</td>
                <td>{formatDate(order.orderDate)}</td>
                <td>{order.totalPrice.toLocaleString('vi-VN')} đ</td>
                <td>{getStatusBadge(order.status)}</td>
                <td>
                  <Button 
                    as={Link} 
                    to={`/orders/${order.id}`} 
                    variant="outline-primary" 
                    size="sm"
                  >
                    Chi tiết
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      ) : (
        <div className="text-center my-5">
          <h3>Bạn chưa có đơn hàng nào</h3>
          <p>Hãy mua sắm và đặt hàng để xem lịch sử đơn hàng của bạn tại đây.</p>
          <Button as={Link} to="/products" variant="primary" className="mt-3">
            Mua sắm ngay
          </Button>
        </div>
      )}
    </Container>
  );
};

export default OrderHistory;
