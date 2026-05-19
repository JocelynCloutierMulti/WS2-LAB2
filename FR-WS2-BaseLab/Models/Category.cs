using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FR_WS2_BaseLab.Models;

public partial class Category
{
    public int Id { get; set; }

    public bool Inactive { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string Description { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
