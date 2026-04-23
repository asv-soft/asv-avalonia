namespace Asv.Avalonia;

public interface ISupportTextSearch : IViewModel
{
    void Query(string text);
    void Focus();
}
