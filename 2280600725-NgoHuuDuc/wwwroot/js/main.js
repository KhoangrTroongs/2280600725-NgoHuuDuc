document.addEventListener('DOMContentLoaded', () => {
  // Active nav link handler
  const currentPage = window.location.pathname.split('/').pop();
  const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

  navLinks.forEach(link => {
    const linkHref = link.getAttribute('href');
    if (linkHref === currentPage || (currentPage === '' && linkHref === 'index.html')) {
      link.classList.add('active');
    } else {
      link.classList.remove('active');
    }
  });

  // Change navbar background on scroll
  const navbar = document.querySelector('.navbar');
  window.addEventListener('scroll', () => {
    if (window.scrollY > 50) {
      navbar?.classList.add('shadow-sm');
    } else {
      navbar?.classList.remove('shadow-sm');
    }
  });

  // Show loading state
  window.addEventListener('load', () => {
    const loader = document.querySelector('.loading');
    if (loader) {
      loader.style.display = 'none';
    }
  });

  // Form validation with better feedback
  const validateForm = (form) => {
    const inputs = form.querySelectorAll('input, textarea');
    let isValid = true;

    inputs.forEach(input => {
      input.classList.remove('is-invalid');
      const errorMsg = input.parentElement.querySelector('.invalid-feedback');
      if (errorMsg) {
        errorMsg.remove();
      }

      if (!input.value.trim()) {
        isValid = false;
        input.classList.add('is-invalid');
        const div = document.createElement('div');
        div.className = 'invalid-feedback';
        div.textContent = `${input.placeholder} is required`;
        input.parentElement.appendChild(div);
      } else if (input.type === 'email' && !isValidEmail(input.value)) {
        isValid = false;
        input.classList.add('is-invalid');
        const div = document.createElement('div');
        div.className = 'invalid-feedback';
        div.textContent = 'Please enter a valid email address';
        input.parentElement.appendChild(div);
      }
    });

    return isValid;
  };

  // Contact form submission handling
  const contactForm = document.querySelector('.contact-form');
  if (contactForm) {
    contactForm.addEventListener('submit', (e) => {
      e.preventDefault();

      if (validateForm(contactForm)) {
        // Show success message
        const successMsg = document.createElement('div');
        successMsg.className = 'alert alert-success mt-3';
        successMsg.textContent = 'Thank you for your message! We will get back to you soon.';
        contactForm.appendChild(successMsg);

        // Reset form after delay
        setTimeout(() => {
          contactForm.reset();
          successMsg.remove();
        }, 3000);
      }
    });
  }

  // Newsletter subscription
  const newsletterForm = document.querySelector('.footer .input-group');
  if (newsletterForm) {
    const subscribeBtn = newsletterForm.querySelector('button');
    const emailInput = newsletterForm.querySelector('input[type="email"]');

    subscribeBtn.addEventListener('click', () => {
      const email = emailInput.value.trim();

      if (email === '') {
        alert('Please enter your email address.');
        return;
      }

      // Simple email validation
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        alert('Please enter a valid email address.');
        return;
      }

      // In a real application, you would send the email to a server here
      alert('Thank you for subscribing to our newsletter!');
      emailInput.value = '';
    });
  }
});
