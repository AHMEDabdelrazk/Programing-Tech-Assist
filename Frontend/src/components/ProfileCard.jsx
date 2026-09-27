import { useEffect, useState } from "react";
import api from "../services/api";

export default function ProfileCard({ refreshTrigger }) {
  const [user, setUser] = useState(null);
  const [stats, setStats] = useState({
    technologiesTracked: 0,
    averageScore: 0,
    completedRoadmaps: 0,
    totalProjects: 0
  });

  useEffect(() => {
    try {
      const stored = localStorage.getItem("user");
      if (stored) {
        setUser(JSON.parse(stored));
      }
    } catch {
      // Ignore
    }

    // Fetch live statistics for user
    api.get("/progress/stats")
      .then((res) => {
        setStats(res.data);
      })
      .catch(() => {
        // Fallback gracefully
      });
  }, [refreshTrigger]);

  return (
    <div className="profile-card">
      <h2>{user?.username || "Learner"}</h2>
      <p>Roadmaps Started: <strong>{stats.technologiesTracked}</strong></p>
      <p>Average Progress: <strong>{stats.averageScore}%</strong></p>
      <p>Projects Built: <strong>{stats.totalProjects}</strong></p>
    </div>
  );
}