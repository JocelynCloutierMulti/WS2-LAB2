using System;
using System.Collections.Generic;

namespace FR_WS2_BaseLab.Models;

public partial class Post
{
    public int Id { get; set; }

    public int TopId { get; set; }

    public string UserId { get; set; } = null!;

    public bool Inactive { get; set; }

    public string Texte { get; set; } = null!;

    public DateOnly Date { get; set; }

    public virtual Topic Top { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
