import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../services/api';
import '../styles/components.css';
import '../styles/login.css';

export default function Login() {
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        if (user?.token) {
          navigate('/dashboard');
        }
      } catch {
        localStorage.removeItem('user');
      }
    }
  }, [navigate]);

  const login = async (e) => {
    if (e) e.preventDefault();
    if (!email || !password) {
      setError('Please fill in both email and password.');
      return;
    }

    try {
      setLoading(true);
      setError('');

      const response = await api.post('auth/login', {
        email,
        password,
      });

      localStorage.setItem('user', JSON.stringify(response.data));
      navigate('/dashboard');
    } catch (err) {
      const msg = err.response?.data?.message || 'Invalid email or password.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Programming Tech Assist</h1>
        <p>Build your learning roadmap & track your skills</p>

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
          onSubmit={login}
          style={{
            display: 'flex',
            flexDirection: 'column',
            width: '100%',
            gap: '10px',
          }}
        >
          <input
            type="email"
            placeholder="Email Address"
            value={email}
            disabled={loading}
            onChange={(e) => {
              setEmail(e.target.value);
              setError('');
            }}
          />

          <input
            type="password"
            placeholder="Password"
            value={password}
            disabled={loading}
            onChange={(e) => {
              setPassword(e.target.value);
              setError('');
            }}
          />

          <button
            type="submit"
            disabled={loading}
            style={{ cursor: loading ? 'wait' : 'pointer' }}
          >
            {loading ? 'Authenticating...' : 'Login'}
          </button>
        </form>

        <div style={{ marginTop: '16px' }}>
          <span>Don't have an account? </span>
          <Link to="/register">Create Account</Link>
        </div>
      </div>
    </div>
  );
}
