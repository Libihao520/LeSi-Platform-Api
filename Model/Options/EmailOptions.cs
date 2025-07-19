namespace Model.Options;

public class EmailOptions
{
    public List<EmailAndKeys>? EmailAndKeys { get; set; }
}

public class EmailAndKeys
{
    public string MyEmail { get; set; }

    public string MyKey { get; set; }
}