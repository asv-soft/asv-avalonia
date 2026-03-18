namespace Asv.Avalonia;

public interface ISupportTextSearch : IRoutable
{
    void Query(string text);
    void Focus();
}
