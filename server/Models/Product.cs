using System;
using System.Collections.Generic;

namespace Smart_Recipe_Generator.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public string Emoji { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
