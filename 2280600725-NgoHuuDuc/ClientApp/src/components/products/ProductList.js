import React, { useState, useEffect, useContext } from 'react';
import { Container, Row, Col, Card, Button, Form, Pagination, Spinner } from 'react-bootstrap';
import { Link, useParams, useNavigate, useLocation } from 'react-router-dom';
import axios from 'axios';
import { CartContext } from '../../contexts/CartContext';

const ProductList = () => {
  const { categoryId } = useParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedCategory, setSelectedCategory] = useState(categoryId || '');
  const { addToCart } = useContext(CartContext);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    fetchCategories();
  }, []);

  useEffect(() => {
    if (categoryId) {
      setSelectedCategory(categoryId);
    }
  }, [categoryId]);

  useEffect(() => {
    fetchProducts();
  }, [currentPage, selectedCategory, searchKeyword]);

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

  const fetchProducts = async () => {
    setLoading(true);
    try {
      let url = '/api/Products/paged';
      const params = {
        pageIndex: currentPage,
        pageSize: 8
      };

      if (selectedCategory) {
        params.categoryId = selectedCategory;
      }

      if (searchKeyword) {
        url = '/api/Products/search';
        params.keyword = searchKeyword;
      }

      const response = await axios.get(url, { params });
      if (response.data.isSuccess) {
        setProducts(response.data.data.items);
        setTotalPages(Math.ceil(response.data.data.totalItems / response.data.data.pageSize));
      }
    } catch (error) {
      console.error('Error fetching products:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCategoryChange = (e) => {
    const categoryId = e.target.value;
    setSelectedCategory(categoryId);
    setCurrentPage(1);
    
    if (categoryId) {
      navigate(`/products/category/${categoryId}`);
    } else {
      navigate('/products');
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchProducts();
  };

  const handleAddToCart = async (productId) => {
    const result = await addToCart(productId, 1);
    if (result.success) {
      // Show success message or notification
    } else {
      // Show error message
      alert(result.message);
    }
  };

  const handlePageChange = (pageNumber) => {
    setCurrentPage(pageNumber);
    window.scrollTo(0, 0);
  };

  // Generate pagination items
  let paginationItems = [];
  for (let number = 1; number <= totalPages; number++) {
    paginationItems.push(
      <Pagination.Item 
        key={number} 
        active={number === currentPage}
        onClick={() => handlePageChange(number)}
      >
        {number}
      </Pagination.Item>
    );
  }

  return (
    <Container>
      <h1 className="mb-4">Sản phẩm</h1>
      
      {/* Search and Filter */}
      <Row className="mb-4">
        <Col md={6}>
          <Form onSubmit={handleSearch}>
            <Form.Group className="d-flex">
              <Form.Control
                type="text"
                placeholder="Tìm kiếm sản phẩm..."
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
              />
              <Button variant="primary" type="submit" className="ms-2">
                Tìm kiếm
              </Button>
            </Form.Group>
          </Form>
        </Col>
        <Col md={6}>
          <Form.Group>
            <Form.Select 
              value={selectedCategory} 
              onChange={handleCategoryChange}
            >
              <option value="">Tất cả danh mục</option>
              {categories.map(category => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
        </Col>
      </Row>
      
      {/* Products */}
      {loading ? (
        <div className="text-center my-5">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
        </div>
      ) : products.length > 0 ? (
        <>
          <Row>
            {products.map(product => (
              <Col key={product.id} lg={3} md={4} sm={6} className="mb-4">
                <Card className={`h-100 product-card ${product.quantity <= 0 ? 'out-of-stock' : ''}`}>
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
                    <Card.Text>
                      {product.quantity > 0 ? (
                        <small className="text-muted">Còn {product.quantity} sản phẩm</small>
                      ) : (
                        <small className="text-danger">Hết hàng</small>
                      )}
                    </Card.Text>
                    <div className="mt-auto d-flex flex-column gap-2">
                      <Button 
                        as={Link} 
                        to={`/products/${product.id}`} 
                        variant="outline-primary" 
                        className="w-100"
                      >
                        Xem chi tiết
                      </Button>
                      {product.quantity > 0 ? (
                        <Button 
                          variant="primary" 
                          className="w-100"
                          onClick={() => handleAddToCart(product.id)}
                        >
                          Thêm vào giỏ
                        </Button>
                      ) : (
                        <Button 
                          variant="secondary" 
                          className="w-100"
                          disabled
                        >
                          Hết hàng
                        </Button>
                      )}
                    </div>
                  </Card.Body>
                </Card>
              </Col>
            ))}
          </Row>
          
          {/* Pagination */}
          {totalPages > 1 && (
            <div className="d-flex justify-content-center mt-4">
              <Pagination>
                <Pagination.First onClick={() => handlePageChange(1)} disabled={currentPage === 1} />
                <Pagination.Prev onClick={() => handlePageChange(currentPage - 1)} disabled={currentPage === 1} />
                {paginationItems}
                <Pagination.Next onClick={() => handlePageChange(currentPage + 1)} disabled={currentPage === totalPages} />
                <Pagination.Last onClick={() => handlePageChange(totalPages)} disabled={currentPage === totalPages} />
              </Pagination>
            </div>
          )}
        </>
      ) : (
        <div className="text-center my-5">
          <h3>Không tìm thấy sản phẩm nào</h3>
          <p>Vui lòng thử tìm kiếm với từ khóa khác hoặc chọn danh mục khác.</p>
        </div>
      )}
    </Container>
  );
};

export default ProductList;
