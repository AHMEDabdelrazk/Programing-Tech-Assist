import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../services/api';
import '../styles/components.css';
import '../styles/login.css';

export default function Register() {
  const navigate = useNavigate();

  const [user, setUser] = useState({
    username: '',
    email: '',
    password: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const register = async (e) => {
    if (e) e.preventDefault();

    if (!user.username || !user.email || !user.password) {
      setError('Please fill in all required fields.');
      return;
    }

    if (user.password.length < 6) {
      setError('Password must be at least 6 characters long.');
      return;
    }

    try {
      setLoading(true);
      setError('');

      const response = await api.post('auth/register', user);

      // Auto-login with returned JWT token
      if (response.data?.token) {
        localStorage.setItem('user', JSON.stringify(response.data));
        navigate('/dashboard');
      } else {
        navigate('/');
      }
    } catch (err) {
      const msg =
        err.response?.data?.message ||
        (err.response?.data?.errors
          ? Object.values(err.response.data.errors).flat().join(', ')
          : 'Registration failed.');
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Create Account</h1>
        <p>Join Programming Tech Assist and accelerate your developer career</p>

        {error && (
          <div
            style={{
              background: '#ffebee',
              color: '#c62828',
              padding: '10px',
              borderRadius: '6px',
              marginBottom: '12px',
              fontSize: '14px',
              textAlign: 'center',
            }}
          >
            {error}
          </div>
        )}

        <form
          onSubmit={register}
          style={{
            display: 'flex',
            flexDirection: 'column',
            width: '100%',
            gap: '10px',
          }}
        >
          <input
            placeholder="Username (min 3 chars)"
            value={user.username}
            disabled={loading}
            onChange={(e) => {
              setUser({ ...user, username: e.target.value });
              setError('');
            }}
          />

          <input
            type="email"
            placeholder="Email Address"
            value={user.email}
            disabled={loading}
            onChange={(e) => {
              setUser({ ...user, email: e.target.value });
              setError('');
            }}
          />

          <input
            type="password"
            placeholder="Password (min 6 chars)"
            value={user.password}
            disabled={loading}
            onChange={(e) => {
              setUser({ ...user, password: e.target.value });
              setError('');
            }}
          />

          <button
            type="submit"
            disabled={loading}
            style={{ cursor: loading ? 'wait' : 'pointer' }}
          >
            {loading ? 'Creating Account...' : 'Register'}
          </button>
        </form>

        <div style={{ marginTop: '16px' }}>
          <span>Already have an account? </span>
          <Link to="/">Back To Login</Link>
        </div>
      </div>
    </div>
  );
}
