# Elegant Suits - E-Commerce API

This project is an API-based e-commerce application for selling elegant suits and clothing.

## Project Structure

The project follows a clean architecture pattern with the following layers:

- **API Layer**: Controllers that handle HTTP requests and responses
- **Service Layer**: Business logic implementation
- **Repository Layer**: Data access logic
- **Domain Layer**: Core business entities and interfaces
- **DTOs**: Data Transfer Objects for API communication

## API Endpoints

### Authentication

- `POST /api/Auth/login` - Login with email and password
- `POST /api/Auth/register` - Register a new user
- `POST /api/Auth/logout` - Logout the current user

### Products

- `GET /api/Products` - Get all products (optional categoryId query parameter)
- `GET /api/Products/paged` - Get paginated products
- `GET /api/Products/{id}` - Get a specific product by ID
- `GET /api/Products/search` - Search products by keyword
- `POST /api/Products` - Create a new product (Admin only)
- `PUT /api/Products/{id}` - Update a product (Admin only)
- `DELETE /api/Products/{id}` - Delete a product (Admin only)

### Categories

- `GET /api/Categories` - Get all categories
- `GET /api/Categories/{id}` - Get a specific category by ID
- `POST /api/Categories` - Create a new category (Admin only)
- `PUT /api/Categories/{id}` - Update a category (Admin only)
- `DELETE /api/Categories/{id}` - Delete a category (Admin only)

### Users

- `GET /api/Users` - Get all users (Admin only)
- `GET /api/Users/{id}` - Get a specific user by ID
- `GET /api/Users/current` - Get the current user
- `PUT /api/Users/{id}` - Update a user
- `DELETE /api/Users/{id}` - Delete a user

### Cart

- `GET /api/Cart` - Get the current user's cart
- `POST /api/Cart` - Add a product to the cart
- `PUT /api/Cart` - Update a cart item
- `DELETE /api/Cart/{cartItemId}` - Remove a cart item
- `DELETE /api/Cart` - Clear the cart

### Orders

- `GET /api/Orders` - Get all orders (Admin only)
- `GET /api/Orders/my-orders` - Get the current user's orders
- `GET /api/Orders/{id}` - Get a specific order by ID
- `POST /api/Orders` - Create a new order
- `PUT /api/Orders/{id}/status` - Update an order's status (Admin only)
- `DELETE /api/Orders/{id}` - Delete an order (Admin only)

## Authentication

The API uses JWT (JSON Web Token) for authentication. To access protected endpoints, include the JWT token in the Authorization header:

```
Authorization: Bearer {token}
```

## Response Format

All API responses follow a standard format:

```json
{
  "IsSuccess": true,
  "Message": "Operation completed successfully",
  "Data": { ... },
  "Errors": null
}
```

## Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Run the application
4. Access the Swagger documentation at `/swagger`

## Technologies Used

- ASP.NET Core 6.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger for API documentation
