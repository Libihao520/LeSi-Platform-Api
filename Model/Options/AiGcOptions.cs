namespace Model.Options;

public class AiGcOptions
{
    public List<AiGcService> Services { get; set; }
}
public class AiGcService
{
    public string Model { get; set; }
    public string Url { get; set; }
    public string Token { get; set; }
}