export default function TechnologySelector({
 technologies,
 setSelectedTech
}) {

 return (

  <div className="tech-row">

   {technologies.map(tech=>

    <button
      key={tech.name}
      style={{
        background:tech.color
      }}
      className="tech-btn"
      onClick={()=>
        setSelectedTech(tech)
      }
    >

      {tech.name}

    </button>

   )}

  </div>

 );
}