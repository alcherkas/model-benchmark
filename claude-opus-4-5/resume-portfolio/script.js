/**
 * Solutions Architect Portfolio - JavaScript
 * Handles animations, interactivity, and visual effects
 */

// ============================================
// Utility Functions
// ============================================

/**
 * Debounce function for performance optimization
 */
function debounce(func, wait = 20) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Check if user prefers reduced motion
 */
function prefersReducedMotion() {
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

/**
 * Check if element is in viewport
 */
function isInViewport(element, threshold = 0.2) {
    const rect = element.getBoundingClientRect();
    const windowHeight = window.innerHeight || document.documentElement.clientHeight;
    return rect.top <= windowHeight * (1 - threshold) && rect.bottom >= 0;
}

// ============================================
// Navigation
// ============================================

class Navigation {
    constructor() {
        this.navbar = document.getElementById('navbar');
        this.navToggle = document.getElementById('nav-toggle');
        this.navMenu = document.getElementById('nav-menu');
        this.navLinks = document.querySelectorAll('.nav-link');
        this.scrollProgress = document.getElementById('scroll-progress');
        this.sections = document.querySelectorAll('section[id]');
        
        this.init();
    }
    
    init() {
        // Mobile menu toggle
        if (this.navToggle) {
            this.navToggle.addEventListener('click', () => this.toggleMenu());
        }
        
        // Close menu on link click
        this.navLinks.forEach(link => {
            link.addEventListener('click', () => this.closeMenu());
        });
        
        // Scroll events
        window.addEventListener('scroll', debounce(() => {
            this.handleScroll();
            this.updateActiveLink();
            this.updateScrollProgress();
        }, 10));
    }
    
    toggleMenu() {
        this.navToggle.classList.toggle('active');
        this.navMenu.classList.toggle('active');
    }
    
    closeMenu() {
        this.navToggle.classList.remove('active');
        this.navMenu.classList.remove('active');
    }
    
    handleScroll() {
        if (window.scrollY > 50) {
            this.navbar.classList.add('scrolled');
        } else {
            this.navbar.classList.remove('scrolled');
        }
    }
    
    updateActiveLink() {
        let current = '';
        
        this.sections.forEach(section => {
            const sectionTop = section.offsetTop - 100;
            if (window.scrollY >= sectionTop) {
                current = section.getAttribute('id');
            }
        });
        
        this.navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href') === `#${current}`) {
                link.classList.add('active');
            }
        });
    }
    
    updateScrollProgress() {
        const windowHeight = document.documentElement.scrollHeight - window.innerHeight;
        const scrolled = (window.scrollY / windowHeight) * 100;
        this.scrollProgress.style.width = `${scrolled}%`;
    }
}

// ============================================
// Particle System for Background - Minimalist
// ============================================

class ParticleSystem {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;
        
        this.ctx = this.canvas.getContext('2d');
        this.particles = [];
        this.mouseX = 0;
        this.mouseY = 0;
        this.particleCount = prefersReducedMotion() ? 20 : 40; // Reduced for cleaner look
        
        this.init();
    }
    
    init() {
        this.resize();
        this.createParticles();
        
        window.addEventListener('resize', debounce(() => this.resize(), 100));
        window.addEventListener('mousemove', (e) => {
            this.mouseX = e.clientX;
            this.mouseY = e.clientY;
        });
        
        if (!prefersReducedMotion()) {
            this.animate();
        } else {
            this.drawStatic();
        }
    }
    
    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }
    
    createParticles() {
        this.particles = [];
        for (let i = 0; i < this.particleCount; i++) {
            this.particles.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height,
                vx: (Math.random() - 0.5) * 0.3, // Slower movement
                vy: (Math.random() - 0.5) * 0.3,
                size: Math.random() * 1.5 + 0.5, // Smaller particles
                opacity: Math.random() * 0.4 + 0.1
            });
        }
    }
    
    drawStatic() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        this.particles.forEach(particle => {
            this.ctx.beginPath();
            this.ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
            this.ctx.fillStyle = `rgba(0, 229, 255, ${particle.opacity})`;
            this.ctx.fill();
        });
        
        this.drawConnections();
    }
    
    animate() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        this.particles.forEach(particle => {
            // Update position
            particle.x += particle.vx;
            particle.y += particle.vy;
            
            // Subtle mouse interaction
            const dx = this.mouseX - particle.x;
            const dy = this.mouseY - particle.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance < 120) {
                const force = (120 - distance) / 120;
                particle.vx -= (dx / distance) * force * 0.01;
                particle.vy -= (dy / distance) * force * 0.01;
            }
            
            // Boundary check
            if (particle.x < 0 || particle.x > this.canvas.width) particle.vx *= -1;
            if (particle.y < 0 || particle.y > this.canvas.height) particle.vy *= -1;
            
            // Draw particle
            this.ctx.beginPath();
            this.ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
            this.ctx.fillStyle = `rgba(0, 229, 255, ${particle.opacity})`;
            this.ctx.fill();
        });
        
        this.drawConnections();
        
        requestAnimationFrame(() => this.animate());
    }
    
    drawConnections() {
        for (let i = 0; i < this.particles.length; i++) {
            for (let j = i + 1; j < this.particles.length; j++) {
                const dx = this.particles[i].x - this.particles[j].x;
                const dy = this.particles[i].y - this.particles[j].y;
                const distance = Math.sqrt(dx * dx + dy * dy);
                
                if (distance < 120) { // Shorter connection distance
                    const opacity = (120 - distance) / 120 * 0.15; // More subtle
                    this.ctx.beginPath();
                    this.ctx.moveTo(this.particles[i].x, this.particles[i].y);
                    this.ctx.lineTo(this.particles[j].x, this.particles[j].y);
                    this.ctx.strokeStyle = `rgba(0, 229, 255, ${opacity})`;
                    this.ctx.lineWidth = 0.5;
                    this.ctx.stroke();
                }
            }
        }
    }
}

// ============================================
// Neural Network Visualization - Minimalist
// ============================================

class NeuralNetwork {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        if (!this.container) return;
        
        this.nodes = [];
        this.connections = [];
        this.init();
    }
    
    init() {
        // Create SVG
        this.svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        this.svg.setAttribute('width', '100%');
        this.svg.setAttribute('height', '100%');
        this.svg.setAttribute('viewBox', '0 0 350 350');
        this.container.appendChild(this.svg);
        
        // Create simplified layers (fewer nodes)
        const layers = [3, 4, 4, 3];
        let nodeId = 0;
        
        layers.forEach((count, layerIndex) => {
            const x = 70 + layerIndex * 70;
            const spacing = 280 / (count + 1);
            
            for (let i = 0; i < count; i++) {
                const y = 35 + spacing * (i + 1);
                this.nodes.push({ id: nodeId++, x, y, layer: layerIndex });
            }
        });
        
        // Create connections
        for (let i = 0; i < layers.length - 1; i++) {
            const currentLayer = this.nodes.filter(n => n.layer === i);
            const nextLayer = this.nodes.filter(n => n.layer === i + 1);
            
            currentLayer.forEach(node1 => {
                nextLayer.forEach(node2 => {
                    this.connections.push({ from: node1, to: node2 });
                });
            });
        }
        
        this.render();
        if (!prefersReducedMotion()) {
            this.animate();
        }
    }
    
    render() {
        // Draw connections first (behind nodes)
        this.connections.forEach((conn, index) => {
            const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
            line.setAttribute('x1', conn.from.x);
            line.setAttribute('y1', conn.from.y);
            line.setAttribute('x2', conn.to.x);
            line.setAttribute('y2', conn.to.y);
            line.setAttribute('stroke', 'rgba(0, 229, 255, 0.1)');
            line.setAttribute('stroke-width', '1');
            line.setAttribute('class', 'neural-connection');
            line.setAttribute('data-index', index);
            this.svg.appendChild(line);
        });
        
        // Draw nodes
        this.nodes.forEach((node, index) => {
            // Outer glow
            const glow = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
            glow.setAttribute('cx', node.x);
            glow.setAttribute('cy', node.y);
            glow.setAttribute('r', '8');
            glow.setAttribute('fill', 'none');
            glow.setAttribute('stroke', 'rgba(0, 229, 255, 0.15)');
            glow.setAttribute('stroke-width', '1');
            glow.setAttribute('class', 'neural-glow');
            this.svg.appendChild(glow);
            
            // Inner node
            const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
            circle.setAttribute('cx', node.x);
            circle.setAttribute('cy', node.y);
            circle.setAttribute('r', '4');
            circle.setAttribute('fill', 'rgba(0, 229, 255, 0.6)');
            circle.setAttribute('class', 'neural-node');
            circle.setAttribute('data-index', index);
            this.svg.appendChild(circle);
        });
    }
    
    animate() {
        // Subtle connection pulse - less frequent
        setInterval(() => {
            const connections = this.svg.querySelectorAll('.neural-connection');
            const randomConn = connections[Math.floor(Math.random() * connections.length)];
            
            if (randomConn) {
                randomConn.setAttribute('stroke', 'rgba(0, 229, 255, 0.4)');
                randomConn.setAttribute('stroke-width', '1.5');
                
                setTimeout(() => {
                    randomConn.setAttribute('stroke', 'rgba(0, 229, 255, 0.1)');
                    randomConn.setAttribute('stroke-width', '1');
                }, 400);
            }
        }, 400);
        
        // Subtle node pulse - less frequent
        setInterval(() => {
            const nodes = this.svg.querySelectorAll('.neural-node');
            const randomNode = nodes[Math.floor(Math.random() * nodes.length)];
            
            if (randomNode) {
                randomNode.setAttribute('r', '5');
                randomNode.setAttribute('fill', 'rgba(0, 229, 255, 0.9)');
                
                setTimeout(() => {
                    randomNode.setAttribute('r', '4');
                    randomNode.setAttribute('fill', 'rgba(0, 229, 255, 0.6)');
                }, 300);
            }
        }, 500);
    }
}

// ============================================
// Cursor Glow Effect - Futuristic Touch
// ============================================

class CursorGlow {
    constructor() {
        this.heroSection = document.querySelector('.hero-section');
        if (!this.heroSection || prefersReducedMotion()) return;
        
        this.init();
    }
    
    init() {
        this.heroSection.addEventListener('mousemove', (e) => {
            const rect = this.heroSection.getBoundingClientRect();
            const x = ((e.clientX - rect.left) / rect.width) * 100;
            const y = ((e.clientY - rect.top) / rect.height) * 100;
            
            this.heroSection.style.setProperty('--mouse-x', `${x}%`);
            this.heroSection.style.setProperty('--mouse-y', `${y}%`);
        });
    }
}

// ============================================
// Scroll Animations
// ============================================

class ScrollAnimations {
    constructor() {
        this.elements = {
            expertiseCards: document.querySelectorAll('.expertise-card'),
            skillBars: document.querySelectorAll('.skill-bar'),
            timelineItems: document.querySelectorAll('.timeline-item'),
            aiCards: document.querySelectorAll('.ai-card'),
            counters: document.querySelectorAll('.counter')
        };
        
        this.init();
    }
    
    init() {
        // Initial check
        this.checkVisibility();
        
        // Scroll event
        window.addEventListener('scroll', debounce(() => this.checkVisibility(), 10));
    }
    
    checkVisibility() {
        // Expertise cards
        this.elements.expertiseCards.forEach((card, index) => {
            if (isInViewport(card) && !card.classList.contains('visible')) {
                setTimeout(() => {
                    card.classList.add('visible');
                }, index * 100);
            }
        });
        
        // Skill bars
        this.elements.skillBars.forEach((bar, index) => {
            if (isInViewport(bar) && !bar.classList.contains('visible')) {
                setTimeout(() => {
                    bar.classList.add('visible');
                    const level = bar.dataset.level || 0;
                    bar.style.setProperty('--skill-level', `${level}%`);
                }, index * 150);
            }
        });
        
        // Timeline items
        this.elements.timelineItems.forEach((item, index) => {
            if (isInViewport(item) && !item.classList.contains('visible')) {
                setTimeout(() => {
                    item.classList.add('visible');
                }, index * 200);
            }
        });
        
        // AI cards
        this.elements.aiCards.forEach((card, index) => {
            if (isInViewport(card) && !card.classList.contains('visible')) {
                setTimeout(() => {
                    card.classList.add('visible');
                }, index * 150);
            }
        });
        
        // Counters
        this.elements.counters.forEach(counter => {
            if (isInViewport(counter) && !counter.classList.contains('counted')) {
                counter.classList.add('counted');
                this.animateCounter(counter);
            }
        });
    }
    
    animateCounter(element) {
        const target = parseInt(element.dataset.target, 10);
        const duration = 2000;
        const step = target / (duration / 16);
        let current = 0;
        
        const update = () => {
            current += step;
            if (current < target) {
                element.textContent = Math.floor(current);
                requestAnimationFrame(update);
            } else {
                element.textContent = target;
            }
        };
        
        if (!prefersReducedMotion()) {
            update();
        } else {
            element.textContent = target;
        }
    }
}

// ============================================
// Form Handling
// ============================================

class ContactForm {
    constructor(formId) {
        this.form = document.getElementById(formId);
        if (!this.form) return;
        
        this.init();
    }
    
    init() {
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        
        // Add focus effects
        const inputs = this.form.querySelectorAll('input, textarea');
        inputs.forEach(input => {
            input.addEventListener('focus', () => {
                input.parentElement.classList.add('focused');
            });
            
            input.addEventListener('blur', () => {
                if (!input.value) {
                    input.parentElement.classList.remove('focused');
                }
            });
        });
    }
    
    handleSubmit(e) {
        e.preventDefault();
        
        const formData = new FormData(this.form);
        const data = Object.fromEntries(formData.entries());
        
        // Show success message (in real implementation, send to server)
        const button = this.form.querySelector('.btn-submit');
        const originalText = button.innerHTML;
        
        button.innerHTML = '<span>Message Sent!</span>';
        button.style.background = 'linear-gradient(135deg, #10b981, #059669)';
        
        setTimeout(() => {
            button.innerHTML = originalText;
            button.style.background = '';
            this.form.reset();
        }, 3000);
        
        console.log('Form submitted:', data);
    }
}

// ============================================
// Typewriter Effect
// ============================================

class TypewriterEffect {
    constructor() {
        this.elements = document.querySelectorAll('.typewriter');
        
        if (prefersReducedMotion()) {
            this.elements.forEach(el => {
                el.style.animation = 'none';
                el.style.borderRight = 'none';
            });
        }
    }
}

// ============================================
// Smooth Scroll
// ============================================

class SmoothScroll {
    constructor() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                e.preventDefault();
                const target = document.querySelector(anchor.getAttribute('href'));
                
                if (target) {
                    const offset = 80; // Account for fixed navbar
                    const targetPosition = target.offsetTop - offset;
                    
                    window.scrollTo({
                        top: targetPosition,
                        behavior: prefersReducedMotion() ? 'auto' : 'smooth'
                    });
                }
            });
        });
    }
}

// ============================================
// Initialize Everything
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    // Initialize all modules
    new Navigation();
    new ParticleSystem('particles-canvas');
    new NeuralNetwork('neural-network');
    new CursorGlow();
    new ScrollAnimations();
    new ContactForm('contact-form');
    new TypewriterEffect();
    new SmoothScroll();
    
    // Add loaded class to body for initial animations
    document.body.classList.add('loaded');
    
    console.log('Portfolio initialized successfully!');
});

// ============================================
// Performance: Pause animations when tab is not visible
// ============================================

document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        document.body.classList.add('tab-hidden');
    } else {
        document.body.classList.remove('tab-hidden');
    }
});
