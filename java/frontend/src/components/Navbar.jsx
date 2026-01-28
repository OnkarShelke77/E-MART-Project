import React, { useState, useEffect, useRef } from 'react';
import styles from '../styles/Navbar.module.css';
import { useCart } from '../context/CartContext';
import { useNavigate, useLocation } from 'react-router-dom';

const Navbar = ({ onCartClick, onLogoClick }) => {
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const dropdownRef = useRef(null);
    const navigate = useNavigate();
    const location = useLocation();
    const { cartItems } = useCart();

    // Check login status on mount and when location or localStorage changes
    useEffect(() => {
        const checkLogin = () => {
            const user = localStorage.getItem('user');
            setIsLoggedIn(!!user);
        };

        checkLogin();
        // Listen for storage events (e.g. from other tabs)
        window.addEventListener('storage', checkLogin);
        return () => window.removeEventListener('storage', checkLogin);
    }, [location]);

    // Close dropdown on outside click
    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setShowDropdown(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const cartCount = cartItems.reduce((total, item) => total + item.quantity, 0);

    const toggleMenu = () => {
        setIsMobileMenuOpen(!isMobileMenuOpen);
    };

    const handleLogout = () => {
        localStorage.removeItem('user');
        localStorage.removeItem('token');
        setIsLoggedIn(false);
        setShowDropdown(false);
        navigate('/login');
    };

    return (
        <nav className={styles.navbar}>
            <div className={styles.logo} onClick={onLogoClick} style={{ cursor: 'pointer' }}>
                e<span className={styles.highlight}>Mart</span>
            </div>

            <div className={styles.searchContainer}>
                <input
                    type="text"
                    placeholder="Search for products, brands and more..."
                    className={styles.searchInput}
                />
                <button className={styles.searchBtn}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <circle cx="11" cy="11" r="8"></circle>
                        <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                    </svg>
                </button>
            </div>

            <div className={styles.hamburger} onClick={toggleMenu}>
                <span className={styles.line}></span>
                <span className={styles.line}></span>
                <span className={styles.line}></span>
            </div>

            <div className={`${styles.navLinks} ${isMobileMenuOpen ? styles.active : ''}`}>
                <a href="#" className={styles.navItem} onClick={(e) => { e.preventDefault(); onCartClick(); }} style={{ position: 'relative' }}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <circle cx="9" cy="21" r="1"></circle>
                        <circle cx="20" cy="21" r="1"></circle>
                        <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"></path>
                    </svg>
                    Cart
                    {cartCount > 0 && (
                        <span style={{
                            position: 'absolute',
                            top: '-8px',
                            right: '-8px',
                            backgroundColor: '#ff4444',
                            color: 'white',
                            borderRadius: '50%',
                            width: '20px',
                            height: '20px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            fontSize: '12px',
                            fontWeight: 'bold'
                        }}>
                            {cartCount}
                        </span>
                    )}
                </a>

                {isLoggedIn ? (
                    <div className={styles.profileContainer} ref={dropdownRef}>
                        <button
                            className={styles.profileBtn}
                            onClick={() => setShowDropdown(!showDropdown)}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                                <circle cx="12" cy="7" r="4"></circle>
                            </svg>
                            Profile
                        </button>
                        <div className={`${styles.dropdownMenu} ${showDropdown ? styles.show : ''}`}>
                            <button className={styles.dropdownItem} onClick={() => { setShowDropdown(false); navigate('/profile'); }}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                                    <circle cx="12" cy="7" r="4"></circle>
                                </svg>
                                My Profile
                            </button>
                            <button className={`${styles.dropdownItem} ${styles.logoutItem}`} onClick={handleLogout}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                                    <polyline points="16 17 21 12 16 7"></polyline>
                                    <line x1="21" y1="12" x2="9" y2="12"></line>
                                </svg>
                                Logout
                            </button>
                        </div>
                    </div>
                ) : (
                    <button
                        className={styles.loginBtn}
                        onClick={() => navigate('/login')}
                    >
                        Login
                    </button>
                )}
            </div>
        </nav>
    );
};

export default Navbar;
