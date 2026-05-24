namespace FR_WS2_BaseLab.Models.ViewModels;

	public class CategoryImageDto
	{
	    public int Id { get; set; }
		public int CategoryId { get; set; }

		public string Url { get; set; } = string.Empty;
	    
	    public string OrginalFileName { get; set;} = string.Empty;

		public string contentType { get; set; } = string.Empty;

	    public long SizeBytes { get; set; }

	    public string? AltText { get; set; }

}

