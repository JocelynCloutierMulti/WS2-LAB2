namespace FR_WS2_BaseLab.Models;

public partial class Topic
{
    public int Id { get; set; }

    public int CatId { get; set; }

    public string? UserId { get; set; }

    public bool Inactive { get; set; }

    public string Title { get; set; } = null!;

    public string Texte { get; set; } = null!;

    public DateTime Date { get; set; }

    public int Views { get; set; }

    public virtual Category? Cat { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual AspNetUser? User { get; set; }
}
