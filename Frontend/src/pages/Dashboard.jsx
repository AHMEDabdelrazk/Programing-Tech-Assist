import { useEffect, useState } from "react";
import api from "../services/api";
import Navbar from "../components/Navbar";
import Sidebar from "../components/Sidebar";
import TechnologyGroup from "../components/TechnologyGroup";
import ProgressCard from "../components/ProgressCard";
import RoadmapTimeline from "../components/RoadmapTimeline";
import DashboardStats from "../components/DashboardStats";
import "../styles/dashboard.css";

export default function Dashboard() {
  const [search, setSearch] = useState("");
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [selectedTech, setSelectedTech] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api
      .get("/roadmap")
      .then((response) => {
        setCategories(response.data);
        if (response.data && response.data.length > 0) {
          setSelectedCategory(response.data[0]);
        }
      })
      .catch((err) => {
        console.error("Failed to load roadmaps:", err);
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const handleProgressSaved = () => {
    setRefreshTrigger((prev) => prev + 1);
  };

  const grouped = selectedCategory?.technologies?.reduce((acc, tech) => {
    if (!acc[tech.group]) {
      acc[tech.group] = [];
    }
    acc[tech.group].push(tech);
    return acc;
  }, {});

  return (
    <>
      <Navbar />
      <div className="layout">
        <Sidebar
          categories={categories}
          selectedCategory={selectedCategory}
          setSelectedCategory={(cat) => {
            setSelectedCategory(cat);
            setSelectedTech(null);
          }}
          refreshTrigger={refreshTrigger}
        />

        <div className="main-content">
          <DashboardStats refreshTrigger={refreshTrigger} />

          <div style={{ display: "flex", gap: "12px", alignItems: "center", marginBottom: "20px" }}>
            <input
              className="search-box"
              placeholder="Search technologies in this category..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{ flex: 1 }}
            />
          </div>

          <div className="hero">
            <h1>
              Become a Professional {selectedCategory?.name || "Software"} Developer
            </h1>
            <p>
              Select technologies below, follow their guided learning paths, and track your
              milestone progress with automated score evaluation.
            </p>
          </div>

          <div className="guide-box">
            {selectedTech
              ? `Active Technology: ${selectedTech.name} — Follow the levels and build recommended projects below.`
              : selectedCategory?.guide || "Select a technology to explore its detailed learning roadmap."}
          </div>

          {loading ? (
            <p style={{ textAlign: "center", padding: "40px", color: "#666" }}>
              Loading roadmap tracks...
            </p>
          ) : (
            grouped &&
            Object.entries(grouped).map(([group, technologies]) => {
              const filteredTechs = technologies.filter((tech) =>
                tech.name.toLowerCase().includes(search.toLowerCase())
              );

              if (filteredTechs.length === 0) return null;

              return (
                <TechnologyGroup
                  key={group}
                  title={group}
                  technologies={filteredTechs}
                  setSelectedTech={setSelectedTech}
                />
              );
            })
          )}

          {selectedTech && (
            <>
              <RoadmapTimeline technology={selectedTech} />
              <ProgressCard
                technology={selectedTech}
                onProgressSaved={handleProgressSaved}
              />
            </>
          )}
        </div>
      </div>
    </>
  );
}