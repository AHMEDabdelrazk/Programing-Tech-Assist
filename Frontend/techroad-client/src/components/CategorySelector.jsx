export default function CategorySelector({
 categories,
 selected,
 setSelected
}) {

 return (

  <div className="category-row">

   {categories.map(category=>

    <button
      key={category.name}
      className="category-btn"
      style={{
        background:category.color
      }}
      onClick={()=>
        setSelected(category)
      }
    >

      {category.name}

    </button>

   )}

  </div>

 );
}