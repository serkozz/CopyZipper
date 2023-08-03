public interface IOptions
{
    public String WatchPath { get; set; }
    public IEnumerable<String> ToPaths { get; set; }
    public String? FileNameRegex { get; set; }
    public FileExtension FileExtension { get; set; }
    public Boolean Override { get; set; }
    public Boolean DeleteAfter { get; set; }
    public String? LogPath { get; set; }
    object? Clone();
}