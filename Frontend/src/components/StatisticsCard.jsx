export default function StatisticsCard({
  score = 0,
  projects = 0,
  basics = false,
  intermediate = false,
  advanced = false
}) {
  const completedLevels = (basics ? 1 : 0) + (intermediate ? 1 : 0) + (advanced ? 1 : 0);

  const getStatusBadge = () => {
    if (score >= 100) return { label: "Mastered 🏆", color: "#2e7d32" };
    if (score >= 50) return { label: "Proficient 🚀", color: "#1976d2" };
    if (score > 0) return { label: "In Progress ⏳", color: "#f57c00" };
    return { label: "Not Started 💤", color: "#757575" };
  };

  const status = getStatusBadge();

  return (
    <div className="statistics-card">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h2>Live Statistics</h2>
        <span style={{
          padding: "4px 8px",
          borderRadius: "4px",
          fontSize: "12px",
          fontWeight: "bold",
          color: "#fff",
          backgroundColor: status.color
        }}>
          {status.label}
        </span>
      </div>

      <p>Current Score: <strong>{score}%</strong></p>
      <p>Projects Built: <strong>{projects}</strong></p>
      <p>Completed Levels: <strong>{completedLevels} of 3</strong></p>
      <p>Roadmap Completion: <strong>{score}%</strong></p>
    </div>
  );
}