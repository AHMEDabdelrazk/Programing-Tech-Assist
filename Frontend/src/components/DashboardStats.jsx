import { useEffect, useState } from "react";
import "../styles/dashboard.css";
import api from "../services/api";

export default function DashboardStats({ refreshTrigger }) {
  const [stats, setStats] = useState({
    technologiesTracked: 0,
    averageScore: 0,
    completedRoadmaps: 0,
    totalProjects: 0,
    totalLevelsCompleted: 0
  });

  useEffect(() => {
    api.get("/progress/stats")
      .then((res) => {
        if (res.data) {
          setStats(res.data);
        }
      })
      .catch(() => {
        // Fallback gracefully
      });
  }, [refreshTrigger]);

  return (
    <div className="stats-grid" style={{ marginBottom: "24px" }}>
      <div className="stat-card">
        <h2>{stats.technologiesTracked}</h2>
        <p>Technologies Tracked</p>
      </div>

      <div className="stat-card">
        <h2>{stats.averageScore}%</h2>
        <p>Average Mastery</p>
      </div>

      <div className="stat-card">
        <h2>{stats.completedRoadmaps}</h2>
        <p>Mastered Roadmaps (100%)</p>
      </div>

      <div className="stat-card">
        <h2>{stats.totalProjects}</h2>
        <p>Projects Built</p>
      </div>
    </div>
  );
}