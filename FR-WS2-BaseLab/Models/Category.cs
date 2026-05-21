namespace FR_WS2_BaseLab.Models;

public partial class Category
{
    public int Id { get; set; }

    public bool Inactive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<CategoryImage>? CategoryImages { get; set; } = [];

    public virtual ICollection<Topic> Topics { get; set; } = [];
}
