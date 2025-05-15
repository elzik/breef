using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Infrastructure;

public class WebPageDownLoaderOptions
{
    [Required]
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                            "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                            "Chrome/110.0.0.0 Safari/537.36";
}
