import { useEffect, useState } from 'react';
import api from '../services/api';
import StatisticsCard from './StatisticsCard';

export default function ProgressCard({ technology, onProgressSaved }) {
  const [topicProgress, setTopicProgress] = useState([]);
  const [projects, setProjects] = useState(0);
  const [serverScore, setServerScore] = useState(null);
  const [saving, setSaving] = useState(false);
  const [feedback, setFeedback] = useState('');

  const topicCount =
    technology?.levels?.reduce(
      (total, level) => total + level.topics.length,
      0
    ) || 0;
  const completedCount = topicProgress.filter(
    (topic) => topic.completed
  ).length;
  const clientScore =
    topicCount === 0
      ? Math.min(Math.max(0, projects) * 5, 20)
      : Math.round(
          technology.levels.reduce((score, level) => {
            const weight =
              { Basics: 20, Intermediate: 30, Advanced: 30 }[level.name] || 0;
            const completed = topicProgress.filter(
              (topic) => topic.level === level.name && topic.completed
            ).length;
            return (
              score +
              (level.topics.length
                ? (completed / level.topics.length) * weight
                : 0)
            );
          }, 0) + Math.min(Math.max(0, projects) * 5, 20)
        );

  const displayScore = serverScore !== null ? serverScore : clientScore;

  const loadProgress = async () => {
    try {
      const response = await api.get(
        `/progress/${encodeURIComponent(technology.name)}`
      );
      if (response.data) {
        setTopicProgress(
          response.data.topicProgress?.length
            ? response.data.topicProgress
            : buildTopicProgress()
        );
        setProjects(response.data.projects || 0);
        setServerScore(response.data.score);
      }
    } catch {
      // Progress not yet saved for this tech, reset to defaults
      setTopicProgress(buildTopicProgress());
      setProjects(0);
      setServerScore(0);
    }
  };

  const buildTopicProgress = () =>
    technology.levels.flatMap((level) =>
      level.topics.map((topic) => ({
        level: level.name,
        topic,
        completed: false,
      }))
    );

  useEffect(() => {
    if (technology?.name) {
      // The loader synchronizes this component with the selected technology's remote record.
      // eslint-disable-next-line react-hooks/set-state-in-effect
      loadProgress();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [technology?.name]);

  const saveProgress = async () => {
    try {
      setSaving(true);
      setFeedback('');

      const response = await api.post('progress', {
        technology: technology.name,
        projects: Number(projects) || 0,
        topicProgress: topicProgress.map((topic) => ({
          level: topic.level,
          topic: topic.topic,
          completed: topic.completed,
        })),
      });

      if (response.data) {
        setServerScore(response.data.score);
      }

      setFeedback('Progress saved successfully!');
      if (onProgressSaved) {
        onProgressSaved();
      }

      setTimeout(() => setFeedback(''), 3000);
    } catch (err) {
      setFeedback(
        err.response?.data?.message ||
          'Failed to save progress. Please try again.'
      );
    } finally {
      setSaving(false);
    }
  };

  if (!technology) return null;

  return (
    <div className="progress-card">
      <h1>{technology.name} Progress Tracker</h1>

      <div className="progress-bar">
        <div
          className="fill"
          style={{
            width: `${displayScore}%`,
            transition: 'width 0.4s ease-in-out',
          }}
        />
      </div>

      <h2>Mastery: {displayScore}%</h2>

      {feedback && (
        <div
          style={{
            background: feedback.includes('failed') ? '#ffebee' : '#e8f5e9',
            color: feedback.includes('failed') ? '#c62828' : '#2e7d32',
            padding: '8px 12px',
            borderRadius: '6px',
            marginBottom: '12px',
            fontWeight: '500',
            textAlign: 'center',
          }}
        >
          {feedback}
        </div>
      )}

      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '10px',
          margin: '15px 0',
        }}
      >
        <div>
          <p className="progress-caption">
            {completedCount} of {topicCount} topics complete. Topic weights:
            Basics 20%, Intermediate 30%, Advanced 30%.
          </p>
          {technology.levels.map((level) => (
            <section className="level-checklist" key={level.name}>
              <h3>
                {level.name} <span>{level.topics.length} topics</span>
              </h3>
              {level.topics.map((topic) => {
                const item = topicProgress.find(
                  (progress) =>
                    progress.level === level.name && progress.topic === topic
                );
                return (
                  <label className="topic-check" key={topic}>
                    <input
                      type="checkbox"
                      checked={item?.completed || false}
                      onChange={(event) => {
                        setTopicProgress((current) =>
                          current.map((progress) =>
                            progress.level === level.name &&
                            progress.topic === topic
                              ? { ...progress, completed: event.target.checked }
                              : progress
                          )
                        );
                        setServerScore(null);
                      }}
                    />
                    <span>{topic}</span>
                  </label>
                );
              })}
            </section>
          ))}

          <div style={{ marginTop: '10px' }}>
            <label style={{ display: 'block', marginBottom: '6px' }}>
              <strong>Projects Built (5% each, max 20%):</strong>
            </label>
            <input
              type="number"
              min="0"
              max="10"
              value={projects}
              onChange={(e) => {
                setProjects(Math.max(0, parseInt(e.target.value, 10) || 0));
                setServerScore(null);
              }}
              style={{
                width: '100px',
                padding: '8px',
                borderRadius: '6px',
                border: '1px solid #ccc',
              }}
            />
          </div>
        </div>
      </div>

      <StatisticsCard
        score={displayScore}
        projects={projects}
        basics={topicProgress.some(
          (topic) => topic.level === 'Basics' && topic.completed
        )}
        intermediate={topicProgress.some(
          (topic) => topic.level === 'Intermediate' && topic.completed
        )}
        advanced={topicProgress.some(
          (topic) => topic.level === 'Advanced' && topic.completed
        )}
      />

      <button
        onClick={saveProgress}
        disabled={saving}
        style={{ marginTop: '20px', cursor: saving ? 'wait' : 'pointer' }}
      >
        {saving ? 'Saving...' : 'Save Progress'}
      </button>
    </div>
  );
}
