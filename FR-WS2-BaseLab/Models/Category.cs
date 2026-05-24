using System;
using System.Collections.Generic;

namespace FR_WS2_BaseLab.Models;

public partial class Category
{
    public int Id { get; set; }

    public bool Inactive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<CategoryImage> CategoryImages { get; set; } = new List<CategoryImage>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
