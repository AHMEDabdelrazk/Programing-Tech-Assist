export default function TechnologyGroup({
    title,
    technologies,
    setSelectedTech
}) {

    return (

        <div className="tech-group">

            <h2>{title}</h2>

            <div className="tech-list">

                {
                    technologies.map(tech => (

                        <button
                            key={tech.name}
                            className="tech-btn"
                            style={{
                                background: tech.color
                            }}
                            onClick={() =>
                                setSelectedTech(tech)
                            }
                        >
                            {tech.name}
                        </button>

                    ))
                }

            </div>

        </div>
    );
}