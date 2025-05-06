import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Carousel } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import axios from 'axios';

const Home = () => {
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch featured products (newest products)
        const productsResponse = await axios.get('/api/Products');
        if (productsResponse.data.isSuccess) {
          // Get the latest 4 products
          setFeaturedProducts(productsResponse.data.data.slice(0, 4));
        }

        // Fetch categories
        const categoriesResponse = await axios.get('/api/Categories');
        if (categoriesResponse.data.isSuccess) {
          setCategories(categoriesResponse.data.data);
        }
      } catch (error) {
        console.error('Error fetching data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return (
    <Container>
      {/* Hero Section */}
      <Carousel className="mb-4">
        <Carousel.Item>
          <img
            className="d-block w-100"
            src="https://via.placeholder.com/1200x400?text=Elegant+Suits"
            alt="First slide"
            style={{ objectFit: 'cover', height: '400px' }}
          />
          <Carousel.Caption>
            <h3>Bộ sưu tập mới nhất</h3>
            <p>Khám phá các mẫu vest cao cấp dành cho quý ông hiện đại.</p>
            <Button as={Link} to="/products" variant="light">Xem ngay</Button>
          </Carousel.Caption>
        </Carousel.Item>
        <Carousel.Item>
          <img
            className="d-block w-100"
            src="https://via.placeholder.com/1200x400?text=Premium+Collection"
            alt="Second slide"
            style={{ objectFit: 'cover', height: '400px' }}
          />
          <Carousel.Caption>
            <h3>Bộ sưu tập cao cấp</h3>
            <p>Thiết kế tinh tế, chất liệu cao cấp, phù hợp với mọi dịp.</p>
            <Button as={Link} to="/products" variant="light">Khám phá</Button>
          </Carousel.Caption>
        </Carousel.Item>
      </Carousel>

      {/* Featured Products */}
      <h2 className="text-center mb-4">Sản phẩm nổi bật</h2>
      <Row>
        {featuredProducts.map(product => (
          <Col key={product.id} md={3} sm={6} className="mb-4">
            <Card className="h-100 product-card">
              <Card.Img 
                variant="top" 
                src={product.imageUrl || "https://via.placeholder.com/300x300?text=No+Image"} 
                className="product-image"
              />
              <Card.Body className="d-flex flex-column">
                <Card.Title>{product.name}</Card.Title>
                <Card.Text className="text-danger fw-bold">
                  {product.price.toLocaleString('vi-VN')} đ
                </Card.Text>
                <div className="mt-auto">
                  <Button 
                    as={Link} 
                    to={`/products/${product.id}`} 
                    variant="outline-primary" 
                    className="w-100"
                  >
                    Xem chi tiết
                  </Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
      
      <div className="text-center mb-5">
        <Button as={Link} to="/products" variant="primary">
          Xem tất cả sản phẩm
        </Button>
      </div>

      {/* Categories */}
      <h2 className="text-center mb-4">Danh mục sản phẩm</h2>
      <Row>
        {categories.map(category => (
          <Col key={category.id} md={4} className="mb-4">
            <Card className="h-100">
              <Card.Body>
                <Card.Title>{category.name}</Card.Title>
                <Card.Text>{category.description}</Card.Text>
                <Button 
                  as={Link} 
                  to={`/products/category/${category.id}`} 
                  variant="outline-secondary"
                >
                  Xem sản phẩm
                </Button>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </Container>
  );
};

export default Home;
