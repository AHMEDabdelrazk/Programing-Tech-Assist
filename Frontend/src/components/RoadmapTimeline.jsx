export default function RoadmapTimeline({ technology }) {
  if (!technology) return null;

  return (
    <div className="timeline">
      <h2>{technology.name} Roadmap</h2>

      {technology.levels.map((level) => (
        <div key={level.name} className="timeline-item">
          <div className="dot"></div>

          <div>
            <h3>{level.name}</h3>

            <p>{level.topics.length} topics to master</p>
            <small>Tracked individually below</small>
          </div>
        </div>
      ))}
    </div>
  );
}
