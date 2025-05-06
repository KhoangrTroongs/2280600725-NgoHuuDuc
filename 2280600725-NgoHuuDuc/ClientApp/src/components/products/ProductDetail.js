import React, { useState, useEffect, useContext } from 'react';
import { Container, Row, Col, Button, Image, Spinner, Form, Alert } from 'react-bootstrap';
import { useParams, Link } from 'react-router-dom';
import axios from 'axios';
import { CartContext } from '../../contexts/CartContext';

const ProductDetail = () => {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [relatedProducts, setRelatedProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const { addToCart } = useContext(CartContext);

  useEffect(() => {
    fetchProduct();
  }, [id]);

  const fetchProduct = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await axios.get(`/api/Products/${id}`);
      if (response.data.isSuccess) {
        setProduct(response.data.data);
        fetchRelatedProducts(response.data.data.categoryId);
      }
    } catch (error) {
      console.error('Error fetching product:', error);
      setError('Không thể tải thông tin sản phẩm. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  const fetchRelatedProducts = async (categoryId) => {
    try {
      const response = await axios.get(`/api/Products`, {
        params: { categoryId }
      });
      if (response.data.isSuccess) {
        // Filter out current product and limit to 4 products
        const filtered = response.data.data
          .filter(p => p.id !== parseInt(id))
          .slice(0, 4);
        setRelatedProducts(filtered);
      }
    } catch (error) {
      console.error('Error fetching related products:', error);
    }
  };

  const handleQuantityChange = (e) => {
    const value = parseInt(e.target.value);
    if (value > 0 && value <= (product?.quantity || 1)) {
      setQuantity(value);
    }
  };

  const increaseQuantity = () => {
    if (quantity < (product?.quantity || 1)) {
      setQuantity(quantity + 1);
    }
  };

  const decreaseQuantity = () => {
    if (quantity > 1) {
      setQuantity(quantity - 1);
    }
  };

  const handleAddToCart = async () => {
    setError('');
    setSuccess('');
    
    if (!product) return;
    
    if (product.quantity <= 0) {
      setError('Sản phẩm đã hết hàng.');
      return;
    }
    
    if (quantity > product.quantity) {
      setError(`Chỉ còn ${product.quantity} sản phẩm trong kho.`);
      return;
    }
    
    const result = await addToCart(product.id, quantity);
    if (result.success) {
      setSuccess('Đã thêm sản phẩm vào giỏ hàng.');
    } else {
      setError(result.message || 'Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.');
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

  if (!product) {
    return (
      <Container className="text-center my-5">
        <h2>Không tìm thấy sản phẩm</h2>
        <p>Sản phẩm không tồn tại hoặc đã bị xóa.</p>
        <Button as={Link} to="/products" variant="primary">
          Quay lại danh sách sản phẩm
        </Button>
      </Container>
    );
  }

  return (
    <Container className="py-4">
      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      
      <Row>
        <Col md={6}>
          <Image 
            src={product.imageUrl || "https://via.placeholder.com/600x600?text=No+Image"} 
            alt={product.name} 
            fluid 
            className="product-detail-image"
          />
        </Col>
        <Col md={6}>
          <h1>{product.name}</h1>
          <h3 className="text-danger">{product.price.toLocaleString('vi-VN')} đ</h3>
          
          <div className="mb-3">
            <p><strong>Danh mục:</strong> {product.categoryName}</p>
            <p><strong>Tình trạng:</strong> {product.quantity > 0 ? `Còn hàng (${product.quantity} sản phẩm)` : 'Hết hàng'}</p>
          </div>
          
          <div className="mb-4">
            <h5>Mô tả sản phẩm:</h5>
            <p>{product.description || 'Không có mô tả cho sản phẩm này.'}</p>
          </div>
          
          {product.quantity > 0 ? (
            <>
              <div className="mb-3">
                <label htmlFor="quantity" className="form-label">Số lượng:</label>
                <div className="quantity-control">
                  <Button variant="outline-secondary" onClick={decreaseQuantity}>-</Button>
                  <Form.Control
                    type="number"
                    id="quantity"
                    min="1"
                    max={product.quantity}
                    value={quantity}
                    onChange={handleQuantityChange}
                    className="mx-2"
                  />
                  <Button variant="outline-secondary" onClick={increaseQuantity}>+</Button>
                </div>
              </div>
              
              <div className="d-grid gap-2">
                <Button variant="primary" size="lg" onClick={handleAddToCart}>
                  Thêm vào giỏ hàng
                </Button>
                <Button as={Link} to="/cart" variant="success" size="lg">
                  Mua ngay
                </Button>
              </div>
            </>
          ) : (
            <div className="d-grid gap-2">
              <Button variant="secondary" size="lg" disabled>
                Hết hàng
              </Button>
              <Button as={Link} to="/products" variant="outline-primary" size="lg">
                Xem sản phẩm khác
              </Button>
            </div>
          )}
        </Col>
      </Row>
      
      {/* Related Products */}
      {relatedProducts.length > 0 && (
        <div className="mt-5">
          <h3 className="mb-4">Sản phẩm liên quan</h3>
          <Row>
            {relatedProducts.map(relatedProduct => (
              <Col key={relatedProduct.id} md={3} sm={6} className="mb-4">
                <div className="card h-100 product-card">
                  <img 
                    src={relatedProduct.imageUrl || "https://via.placeholder.com/300x300?text=No+Image"} 
                    className="card-img-top product-image" 
                    alt={relatedProduct.name} 
                  />
                  <div className="card-body d-flex flex-column">
                    <h5 className="card-title">{relatedProduct.name}</h5>
                    <p className="card-text text-danger fw-bold">
                      {relatedProduct.price.toLocaleString('vi-VN')} đ
                    </p>
                    <div className="mt-auto">
                      <Link to={`/products/${relatedProduct.id}`} className="btn btn-outline-primary w-100">
                        Xem chi tiết
                      </Link>
                    </div>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        </div>
      )}
    </Container>
  );
};

export default ProductDetail;
