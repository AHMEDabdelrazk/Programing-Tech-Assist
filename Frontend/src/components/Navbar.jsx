import { useNavigate } from 'react-router-dom';

export default function Navbar() {
  const navigate = useNavigate();

  const user = JSON.parse(localStorage.getItem('user'));

  const logout = () => {
    localStorage.removeItem('user');

    navigate('/');
  };

  return (
    <div className="navbar">
      <div className="nav-left">
        <div className="logo">🚀</div>

        <div>
          <h2>Programming Tech Assist</h2>
        </div>
      </div>

      <div className="nav-right">
        <div className="user-card">
          <div className="avatar">{user?.username?.[0]?.toUpperCase()}</div>

          <span>{user?.username}</span>
        </div>

        <button onClick={logout} className="logout-btn">
          Logout
        </button>
      </div>
    </div>
  );
}
