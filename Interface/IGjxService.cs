namespace Interface;

public interface IGjxService
{
    Task<string> GetEwm(string txt);
}