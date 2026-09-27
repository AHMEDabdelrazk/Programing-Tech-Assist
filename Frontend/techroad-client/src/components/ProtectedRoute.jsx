import { Navigate } from "react-router-dom";

export default function ProtectedRoute({ children }) {
  const userStr = localStorage.getItem("user");
  let isAuthenticated = false;

  if (userStr) {
    try {
      const user = JSON.parse(userStr);
      if (user && user.token) {
        isAuthenticated = true;
      }
    } catch {
      isAuthenticated = false;
    }
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return children;
}
