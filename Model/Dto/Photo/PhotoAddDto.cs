namespace Model.Dto.photo;

public class PhotoAddDto
{
    public long? ModelId { get; set; }
    public string? Photo { get; set; }

    public string? taskId { get; set; }
    public string connectionId { get; set; }
}