import ProfileCard from "./ProfileCard";

export default function Sidebar({
  categories,
  selectedCategory,
  setSelectedCategory,
  refreshTrigger
}) {
  return (
    <div className="sidebar">
      <ProfileCard refreshTrigger={refreshTrigger} />

      <h3>Categories</h3>

      {categories.map((category) => (
        <button
          key={category.name}
          className={
            selectedCategory?.name === category.name
              ? "sidebar-btn active"
              : "sidebar-btn"
          }
          style={{
            background: category.color
          }}
          onClick={() => setSelectedCategory(category)}
        >
          {category.name}
        </button>
      ))}
    </div>
  );
}