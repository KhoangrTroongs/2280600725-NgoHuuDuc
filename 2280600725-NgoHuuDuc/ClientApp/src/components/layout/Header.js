import React, { useContext, useState, useEffect } from 'react';
import { Navbar, Container, Nav, NavDropdown, Badge } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { AuthContext } from '../../contexts/AuthContext';
import { CartContext } from '../../contexts/CartContext';
import axios from 'axios';

const Header = () => {
  const { currentUser, isAuthenticated, logout } = useContext(AuthContext);
  const { cart } = useContext(CartContext);
  const [categories, setCategories] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      const response = await axios.get('/api/Categories');
      if (response.data.isSuccess) {
        setCategories(response.data.data);
      }
    } catch (error) {
      console.error('Error fetching categories:', error);
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  const cartItemCount = cart.items.reduce((total, item) => total + item.quantity, 0);

  return (
    <Navbar bg="light" expand="lg" sticky="top">
      <Container>
        <Navbar.Brand as={Link} to="/">Elegant Suits</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/">Trang chủ</Nav.Link>
            <NavDropdown title="Sản phẩm" id="basic-nav-dropdown">
              <NavDropdown.Item as={Link} to="/products">Tất cả sản phẩm</NavDropdown.Item>
              <NavDropdown.Divider />
              {categories.map(category => (
                <NavDropdown.Item 
                  key={category.id} 
                  as={Link} 
                  to={`/products/category/${category.id}`}
                >
                  {category.name}
                </NavDropdown.Item>
              ))}
            </NavDropdown>
          </Nav>
          <Nav>
            <Nav.Link as={Link} to="/cart">
              Giỏ hàng
              {cartItemCount > 0 && (
                <Badge bg="danger" pill className="ms-1">
                  {cartItemCount}
                </Badge>
              )}
            </Nav.Link>
            {isAuthenticated ? (
              <NavDropdown title={currentUser?.fullName || 'Tài khoản'} id="user-dropdown">
                <NavDropdown.Item as={Link} to="/profile">Thông tin cá nhân</NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/orders">Lịch sử đơn hàng</NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item onClick={handleLogout}>Đăng xuất</NavDropdown.Item>
              </NavDropdown>
            ) : (
              <NavDropdown title="Tài khoản" id="user-dropdown">
                <NavDropdown.Item as={Link} to="/login">Đăng nhập</NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/register">Đăng ký</NavDropdown.Item>
              </NavDropdown>
            )}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Header;
