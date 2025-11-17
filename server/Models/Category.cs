using System;
using System.Collections.Generic;

namespace Smart_Recipe_Generator.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Emoji { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
