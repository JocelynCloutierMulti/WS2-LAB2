
namespace FR_WS2_BaseLab.Models;

public partial class Image
{
    public string FileName { get; set; } = null!;

    public int CatId { get; set; }

    public virtual Category Cat { get; set; } = null!;
}
