import React from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import { Link } from 'react-router-dom';

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer mt-auto py-3 bg-dark text-white">
      <Container>
        <Row>
          <Col md={4}>
            <h5>Elegant Suits</h5>
            <p>Chuyên cung cấp các sản phẩm thời trang cao cấp dành cho nam giới.</p>
          </Col>
          <Col md={4}>
            <h5>Liên kết</h5>
            <ul className="list-unstyled">
              <li><Link to="/" className="text-white">Trang chủ</Link></li>
              <li><Link to="/products" className="text-white">Sản phẩm</Link></li>
              <li><Link to="/cart" className="text-white">Giỏ hàng</Link></li>
            </ul>
          </Col>
          <Col md={4}>
            <h5>Liên hệ</h5>
            <address>
              <p>123 Đường ABC, Quận XYZ, TP. HCM</p>
              <p>Email: info@elegantsuit.com</p>
              <p>Điện thoại: (123) 456-7890</p>
            </address>
          </Col>
        </Row>
        <hr />
        <div className="text-center">
          <p>&copy; {currentYear} Elegant Suits. All rights reserved.</p>
        </div>
      </Container>
    </footer>
  );
};

export default Footer;
