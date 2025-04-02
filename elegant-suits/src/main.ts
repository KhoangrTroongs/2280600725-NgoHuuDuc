// Get suit shop images from Unsplash
const suitImages = [
  'https://images.unsplash.com/photo-1507679799987-c73779587ccf?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
  'https://images.unsplash.com/photo-1593032465175-481ac7f401a0?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
  'https://images.unsplash.com/photo-1594938298603-c8148c4dae35?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
  'https://images.unsplash.com/photo-1617137968427-85924c800a22?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
  'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
  'https://images.unsplash.com/photo-1553240799-36bbf332a5c3?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
];

const heroImage = 'https://images.unsplash.com/photo-1611312449408-fcece27cdbb7?ixlib=rb-1.2.1&auto=format&fit=crop&w=2000&q=80';
const aboutImage = 'https://images.unsplash.com/photo-1504593811423-6dd665756598?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80';

// Define products
const products = [
  {
    id: 1,
    name: 'Classic Navy Suit',
    price: 499.99,
    oldPrice: 599.99,
    image: suitImages[0],
    badge: 'Best Seller',
  },
  {
    id: 2,
    name: 'Slim Fit Gray Suit',
    price: 449.99,
    oldPrice: null,
    image: suitImages[1],
    badge: 'New',
  },
  {
    id: 3,
    name: 'Tailored Black Suit',
    price: 549.99,
    oldPrice: 649.99,
    image: suitImages[2],
    badge: null,
  },
  {
    id: 4,
    name: 'Modern Brown Suit',
    price: 479.99,
    oldPrice: null,
    image: suitImages[3],
    badge: null,
  },
  {
    id: 5,
    name: 'Royal Blue Suit',
    price: 529.99,
    oldPrice: 599.99,
    image: suitImages[4],
    badge: 'Sale',
  },
  {
    id: 6,
    name: 'Charcoal Gray Suit',
    price: 489.99,
    oldPrice: null,
    image: suitImages[5],
    badge: null,
  }
];

// Define testimonials
const testimonials = [
  {
    text: "I purchased a suit for my wedding and was blown away by the quality and fit. The staff was extremely helpful in finding the perfect style for my big day.",
    author: "John Smith",
    position: "Marketing Director"
  },
  {
    text: "As someone who wears suits daily for work, I've found Elegant Suits to provide the best balance of quality, style, and durability. Highly recommended!",
    author: "Michael Johnson",
    position: "Financial Analyst"
  },
  {
    text: "The attention to detail in their tailoring is unmatched. My custom suit fits perfectly and I've received countless compliments.",
    author: "David Williams",
    position: "Attorney"
  }
];

// Format price
const formatPrice = (price: number): string => {
  return `$${price.toFixed(2)}`;
};

// Create navigation bar
const createNavbar = (): string => {
  return `
    <nav class="navbar navbar-expand-lg navbar-light bg-white fixed-top">
      <div class="container">
        <a class="navbar-brand" href="#">Elegant Suits</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item">
              <a class="nav-link active" href="#home">Home</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#products">Products</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#about">About</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#contact">Contact</a>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  `;
};

// Create hero section
const createHeroSection = (): string => {
  return `
    <section id="home" class="hero" style="background-image: url('${heroImage}');">
      <div class="hero-overlay"></div>
      <div class="container">
        <div class="row">
          <div class="col-lg-8 hero-content">
            <h1>Premium Tailored Suits For Modern Gentlemen</h1>
            <p>Experience the perfect fit with our hand-crafted suits designed for the discerning gentleman.</p>
            <a href="#products" class="btn btn-secondary btn-lg">Explore Collection</a>
          </div>
        </div>
      </div>
    </section>
  `;
};

// Create featured products section
const createFeaturedProductsSection = (): string => {
  const featuredProducts = products.slice(0, 3);

  const productCards = featuredProducts.map(product => `
    <div class="col-md-4">
      <div class="card product-card">
        <div class="position-relative">
          <img src="${product.image}" class="card-img-top product-card-img" alt="${product.name}">
          ${product.badge ? `<div class="product-card-badge">${product.badge}</div>` : ''}
        </div>
        <div class="card-body product-card-body">
          <h5 class="card-title product-card-title">${product.name}</h5>
          <p class="product-card-price">
            ${formatPrice(product.price)}
            ${product.oldPrice ? `<span class="old-price">${formatPrice(product.oldPrice)}</span>` : ''}
          </p>
          <a href="#" class="btn btn-primary">View Details</a>
        </div>
      </div>
    </div>
  `).join('');

  return `
    <section class="featured">
      <div class="container">
        <h2 class="section-title text-center">Featured Suits</h2>
        <div class="row">
          ${productCards}
        </div>
        <div class="text-center mt-4">
          <a href="#products" class="btn btn-secondary btn-lg">View All Products</a>
        </div>
      </div>
    </section>
  `;
};

// Create all products section
const createProductsSection = (): string => {
  const productCards = products.map(product => `
    <div class="col-md-4">
      <div class="card product-card">
        <div class="position-relative">
          <img src="${product.image}" class="card-img-top product-card-img" alt="${product.name}">
          ${product.badge ? `<div class="product-card-badge">${product.badge}</div>` : ''}
        </div>
        <div class="card-body product-card-body">
          <h5 class="card-title product-card-title">${product.name}</h5>
          <p class="product-card-price">
            ${formatPrice(product.price)}
            ${product.oldPrice ? `<span class="old-price">${formatPrice(product.oldPrice)}</span>` : ''}
          </p>
          <a href="#" class="btn btn-primary">View Details</a>
        </div>
      </div>
    </div>
  `).join('');

  return `
    <section id="products" class="featured py-5 bg-light">
      <div class="container">
        <h2 class="section-title text-center">Our Collection</h2>
        <div class="row">
          ${productCards}
        </div>
      </div>
    </section>
  `;
};

// Create about section
const createAboutSection = (): string => {
  return `
    <section id="about" class="about-section">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-lg-6">
            <div class="about-image">
              <img src="${aboutImage}" alt="About Elegant Suits" class="img-fluid">
            </div>
          </div>
          <div class="col-lg-6">
            <div class="about-content">
              <h2 class="section-title">About Elegant Suits</h2>
              <p>Founded in 2005, Elegant Suits has been dedicated to providing high-quality, stylish suits for the modern gentleman. Our passion for quality craftsmanship and attention to detail sets us apart from the competition.</p>
              <p>We believe that every man deserves to look and feel his best, which is why we offer a wide range of styles, sizes, and customization options to ensure the perfect fit.</p>
              <p>Our team of experienced tailors combines traditional techniques with modern innovation to create suits that not only look exceptional but are built to last.</p>
              <a href="#contact" class="btn btn-secondary">Contact Us</a>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="py-5 bg-light">
      <div class="container">
        <h2 class="section-title text-center">What Our Customers Say</h2>
        <div class="row">
          ${testimonials.map(testimonial => `
            <div class="col-lg-4">
              <div class="testimonial">
                <p class="testimonial-text">"${testimonial.text}"</p>
                <p class="testimonial-author">${testimonial.author}</p>
                <p class="testimonial-position">${testimonial.position}</p>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
    </section>
  `;
};

// Create contact section
const createContactSection = (): string => {
  return `
    <section id="contact" class="contact-section">
      <div class="container">
        <h2 class="section-title text-center">Contact Us</h2>
        <div class="row">
          <div class="col-lg-5">
            <div class="contact-info">
              <h5>Our Store</h5>
              <p><i class="fas fa-map-marker-alt"></i> 123 Fashion Street, New York, NY 10001</p>
              <p><i class="fas fa-phone"></i> (123) 456-7890</p>
              <p><i class="fas fa-envelope"></i> info@elegantsuits.com</p>
            </div>

            <div class="contact-info">
              <h5>Opening Hours</h5>
              <p><i class="fas fa-clock"></i> Monday - Friday: 9:00 AM - 8:00 PM</p>
              <p><i class="fas fa-clock"></i> Saturday: 10:00 AM - 6:00 PM</p>
              <p><i class="fas fa-clock"></i> Sunday: Closed</p>
            </div>
          </div>

          <div class="col-lg-7">
            <form class="contact-form">
              <div class="row">
                <div class="col-md-6">
                  <input type="text" class="form-control" placeholder="Your Name" required>
                </div>
                <div class="col-md-6">
                  <input type="email" class="form-control" placeholder="Your Email" required>
                </div>
              </div>
              <input type="text" class="form-control" placeholder="Subject" required>
              <textarea class="form-control" rows="5" placeholder="Your Message" required></textarea>
              <button type="submit" class="btn btn-secondary">Send Message</button>
            </form>
          </div>
        </div>
      </div>
    </section>
  `;
};

// Create footer section
const createFooter = (): string => {
  return `
    <footer class="footer">
      <div class="container">
        <div class="row">
          <div class="col-lg-4 mb-4">
            <h4 class="footer-title">Elegant Suits</h4>
            <p>Premium tailored suits for the modern gentleman. Experience the perfect fit with our hand-crafted suits.</p>
            <div class="footer-social">
              <a href="#"><i class="fab fa-facebook-f"></i></a>
              <a href="#"><i class="fab fa-instagram"></i></a>
              <a href="#"><i class="fab fa-twitter"></i></a>
              <a href="#"><i class="fab fa-pinterest"></i></a>
            </div>
          </div>

          <div class="col-lg-2 col-md-4 mb-4">
            <h4 class="footer-title">Shop</h4>
            <ul class="footer-links">
              <li><a href="#products">All Products</a></li>
              <li><a href="#">New Arrivals</a></li>
              <li><a href="#">Best Sellers</a></li>
              <li><a href="#">Sale</a></li>
            </ul>
          </div>

          <div class="col-lg-2 col-md-4 mb-4">
            <h4 class="footer-title">Company</h4>
            <ul class="footer-links">
              <li><a href="#about">About Us</a></li>
              <li><a href="#">Careers</a></li>
              <li><a href="#">Press</a></li>
              <li><a href="#">Blog</a></li>
            </ul>
          </div>

          <div class="col-lg-4 col-md-4 mb-4">
            <h4 class="footer-title">Newsletter</h4>
            <p>Subscribe to our newsletter to receive updates on new arrivals and special offers.</p>
            <div class="input-group mb-3">
              <input type="email" class="form-control" placeholder="Your Email">
              <button class="btn btn-secondary" type="button">Subscribe</button>
            </div>
          </div>
        </div>

        <div class="footer-bottom">
          <p>&copy; 2025 Elegant Suits. All Rights Reserved.</p>
        </div>
      </div>
    </footer>
  `;
};

// Render the full page
const renderPage = (): void => {
  const appElement = document.querySelector<HTMLDivElement>("#app");
  if (appElement) {
    appElement.innerHTML = `
      ${createNavbar()}
      ${createHeroSection()}
      ${createFeaturedProductsSection()}
      ${createProductsSection()}
      ${createAboutSection()}
      ${createContactSection()}
      ${createFooter()}
    `;
  }
};

// Initialize the page
renderPage();

// Add scroll behavior for smooth navigation
document.addEventListener('DOMContentLoaded', () => {
  // Smooth scrolling for anchor links
  const anchorLinks = document.querySelectorAll('a[href^="#"]');
  for (const anchor of anchorLinks) {
    anchor.addEventListener('click', function(this: HTMLAnchorElement, e: Event) {
      e.preventDefault();

      const href = this.getAttribute('href') || '';
      // Make sure href isn't just "#" before trying to select
      if (href && href !== '#') {
        const target = document.querySelector(href);
        if (target) {
          window.scrollTo({
            top: (target as HTMLElement).offsetTop - 70,
            behavior: 'smooth'
          });
        }
      }
    });
  }

  // Change navbar background on scroll
  const navbar = document.querySelector('.navbar');
  window.addEventListener('scroll', () => {
    if (window.scrollY > 50) {
      navbar?.classList.add('shadow-sm');
    } else {
      navbar?.classList.remove('shadow-sm');
    }
  });

  // Form submission handling
  const contactForm = document.querySelector('.contact-form');
  contactForm?.addEventListener('submit', (e) => {
    e.preventDefault();
    alert('Thank you for your message! We will get back to you soon.');
    (e.target as HTMLFormElement).reset();
  });
});
