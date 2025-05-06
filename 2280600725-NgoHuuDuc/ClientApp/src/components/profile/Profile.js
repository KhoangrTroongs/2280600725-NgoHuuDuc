import React, { useState, useContext, useEffect } from 'react';
import { Container, Form, Button, Card, Alert, Row, Col } from 'react-bootstrap';
import { AuthContext } from '../../contexts/AuthContext';

const Profile = () => {
  const { currentUser, updateProfile } = useContext(AuthContext);
  const [formData, setFormData] = useState({
    fullName: '',
    dateOfBirth: '',
    phoneNumber: '',
    address: '',
    gender: 'Male'
  });
  const [avatarFile, setAvatarFile] = useState(null);
  const [avatarPreview, setAvatarPreview] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (currentUser) {
      setFormData({
        fullName: currentUser.fullName || '',
        dateOfBirth: formatDate(currentUser.dateOfBirth) || '',
        phoneNumber: currentUser.phoneNumber || '',
        address: currentUser.address || '',
        gender: currentUser.gender || 'Male'
      });
      setAvatarPreview(currentUser.avatarUrl || '');
    }
  }, [currentUser]);

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });
  };

  const handleAvatarChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setAvatarFile(file);
      setAvatarPreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);
    
    try {
      // Create FormData object for file upload
      const formDataObj = new FormData();
      formDataObj.append('fullName', formData.fullName);
      formDataObj.append('dateOfBirth', formData.dateOfBirth);
      formDataObj.append('phoneNumber', formData.phoneNumber || '');
      formDataObj.append('address', formData.address || '');
      formDataObj.append('gender', formData.gender);
      
      if (avatarFile) {
        formDataObj.append('avatarFile', avatarFile);
      }
      
      const result = await updateProfile(currentUser.id, formDataObj);
      if (result.success) {
        setSuccess('Cập nhật thông tin thành công!');
      } else {
        setError(result.message || 'Cập nhật thông tin thất bại. Vui lòng thử lại.');
      }
    } catch (error) {
      setError('Đã xảy ra lỗi. Vui lòng thử lại sau.');
      console.error('Update profile error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5">
      <h1 className="mb-4">Thông tin cá nhân</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      
      <Card>
        <Card.Body>
          <Form onSubmit={handleSubmit}>
            <Row>
              <Col md={4} className="text-center mb-4">
                <div className="mb-3">
                  <img
                    src={avatarPreview || "https://via.placeholder.com/150?text=Avatar"}
                    alt="Avatar"
                    className="img-thumbnail rounded-circle"
                    style={{ width: '150px', height: '150px', objectFit: 'cover' }}
                  />
                </div>
                <Form.Group controlId="avatarFile" className="mb-3">
                  <Form.Label>Thay đổi ảnh đại diện</Form.Label>
                  <Form.Control
                    type="file"
                    accept="image/*"
                    onChange={handleAvatarChange}
                  />
                </Form.Group>
                <div className="text-muted">
                  <small>Email: {currentUser?.email}</small>
                </div>
              </Col>
              
              <Col md={8}>
                <Form.Group className="mb-3" controlId="fullName">
                  <Form.Label>Họ và tên <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    name="fullName"
                    value={formData.fullName}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>
                
                <Form.Group className="mb-3" controlId="dateOfBirth">
                  <Form.Label>Ngày sinh <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="date"
                    name="dateOfBirth"
                    value={formData.dateOfBirth}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>
                
                <Form.Group className="mb-3" controlId="phoneNumber">
                  <Form.Label>Số điện thoại</Form.Label>
                  <Form.Control
                    type="tel"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleChange}
                  />
                </Form.Group>
                
                <Form.Group className="mb-3" controlId="address">
                  <Form.Label>Địa chỉ</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={3}
                    name="address"
                    value={formData.address}
                    onChange={handleChange}
                  />
                </Form.Group>
                
                <Form.Group className="mb-3" controlId="gender">
                  <Form.Label>Giới tính <span className="text-danger">*</span></Form.Label>
                  <Form.Select
                    name="gender"
                    value={formData.gender}
                    onChange={handleChange}
                    required
                  >
                    <option value="Male">Nam</option>
                    <option value="Female">Nữ</option>
                    <option value="Other">Khác</option>
                  </Form.Select>
                </Form.Group>
                
                <Button variant="primary" type="submit" disabled={loading}>
                  {loading ? 'Đang cập nhật...' : 'Cập nhật thông tin'}
                </Button>
              </Col>
            </Row>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default Profile;
