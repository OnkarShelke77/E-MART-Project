import { useState, useEffect } from "react";
import axios from "axios";
import styles from "../styles/CheckoutAddress.module.css";
import { useNavigate } from "react-router-dom";
import { FiUser, FiPhone, FiHome, FiMapPin, FiTruck, FiChevronLeft, FiPlus, FiMap } from 'react-icons/fi';

const CheckoutAddress = () => {
  const [address, setAddress] = useState({
    fullName: "",
    mobile: "",
    houseNo: "",
    street: "",
    city: "",
    state: "",
    pincode: ""
  });

  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const token = localStorage.getItem("token");

  const handleChange = (e) => {
    setAddress({
      ...address,
      [e.target.name]: e.target.value
    });
  };

  useEffect(() => {
    const fetchAddress = async () => {
      try {
        const response = await axios.get("http://localhost:8080/api/address/my", {
          headers: { Authorization: `Bearer ${token}` }
        });

        if (response.data && response.data.length > 0) {
          // Use the most recent address (assuming last in list)
          const savedAddress = response.data[response.data.length - 1];
          setAddress({
            fullName: savedAddress.fullName || "",
            mobile: savedAddress.mobile || "",
            houseNo: savedAddress.houseNo || "",
            street: savedAddress.street || "",
            city: savedAddress.city || "",
            state: savedAddress.state || "",
            pincode: savedAddress.pincode || ""
          });
        }
      } catch (err) {
        console.error("Failed to fetch saved addresses", err);
        // Optional: Perform silent fail without showing error to user since it's just prefill
      }
    };

    if (token) {
      fetchAddress();
    }
  }, [token]);

  const saveAddress = async () => {
    setError("");

    if (!address.fullName || !address.mobile || !address.houseNo || !address.city || !address.pincode) {
      setError("Please fill all required fields (*)");
      return;
    }

    try {
      setLoading(true);

      // Fetch user from localStorage to get the ID if needed later, 
      // but the backend AddressController uses Authentication principal.
      await axios.post(
        "http://localhost:8080/api/address/add",
        address,
        {
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      navigate("/payment");
    } catch (err) {
      console.error("Address save failed", err);
      setError("Failed to save address. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.checkoutContainer}>
      <div className={styles.checkoutCard}>
        <button className={styles.backBtn} onClick={() => navigate("/cart")} style={{
          display: 'flex',
          alignItems: 'center',
          gap: '5px',
          background: 'none',
          border: 'none',
          color: '#6b7280',
          cursor: 'pointer',
          marginBottom: '20px',
          fontSize: '0.9rem',
          padding: '0',
          fontWeight: '600'
        }}>
          <FiChevronLeft /> Back to Cart
        </button>

        <div style={{ textAlign: 'center', marginBottom: '30px' }}>
          <div style={{
            width: '60px',
            height: '60px',
            background: 'rgba(99, 102, 241, 0.1)',
            borderRadius: '20px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 20px',
            color: '#6366f1'
          }}>
            <FiTruck size={30} />
          </div>
          <h2 className={styles.checkoutTitle}>Shipping Details</h2>
          <p className={styles.checkoutSubtitle}>Where should we deliver your order?</p>
        </div>

        {error && <div className={styles.errorText}>{error}</div>}

        <div className={styles.formGrid}>
          {/* Full Name */}
          <div className={styles.inputGroup}>
            <label><FiUser size={14} /> Full Name *</label>
            <div className={styles.inputWrapper}>
              <FiUser className={styles.iconPrefix} />
              <input name="fullName" value={address.fullName} placeholder="John Doe" onChange={handleChange} />
            </div>
          </div>

          {/* Mobile Number */}
          <div className={styles.inputGroup}>
            <label><FiPhone size={14} /> Mobile Number *</label>
            <div className={styles.inputWrapper}>
              <FiPhone className={styles.iconPrefix} />
              <input name="mobile" value={address.mobile} placeholder="10-digit number" onChange={handleChange} />
            </div>
          </div>

          {/* House No */}
          <div className={styles.inputGroup}>
            <label><FiHome size={14} /> House / Flat No *</label>
            <div className={styles.inputWrapper}>
              <FiHome className={styles.iconPrefix} />
              <input name="houseNo" value={address.houseNo} placeholder="House #123" onChange={handleChange} />
            </div>
          </div>

          {/* Street */}
          <div className={styles.inputGroup}>
            <label><FiMapPin size={14} /> Street / Area</label>
            <div className={styles.inputWrapper}>
              <FiMapPin className={styles.iconPrefix} />
              <input name="street" value={address.street} placeholder="Main Road, Sector 4" onChange={handleChange} />
            </div>
          </div>

          {/* City */}
          <div className={styles.inputGroup}>
            <label><FiMap size={14} /> City *</label>
            <div className={styles.inputWrapper}>
              <FiMap className={styles.iconPrefix} />
              <input name="city" value={address.city} placeholder="Mumbai" onChange={handleChange} />
            </div>
          </div>

          {/* State */}
          <div className={styles.inputGroup}>
            <label><FiMapPin size={14} /> State</label>
            <div className={styles.inputWrapper}>
              <FiMapPin className={styles.iconPrefix} />
              <input name="state" value={address.state} placeholder="Maharashtra" onChange={handleChange} />
            </div>
          </div>

          {/* Pincode */}
          <div className={`${styles.inputGroup} ${styles.fullWidth}`}>
            <label><FiMapPin size={14} /> Pincode *</label>
            <div className={styles.inputWrapper}>
              <FiMapPin className={styles.iconPrefix} />
              <input name="pincode" value={address.pincode} placeholder="400001" onChange={handleChange} />
            </div>
          </div>

          <button
            className={styles.checkoutBtn}
            onClick={saveAddress}
            disabled={loading}
          >
            {loading ? "Saving Address..." : (
              <>
                <span>Proceed to Payment</span>
                <FiPlus />
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default CheckoutAddress;
