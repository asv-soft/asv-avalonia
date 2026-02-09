namespace Asv.Avalonia;

public interface IDataFormatter
{
    string Name { get; }
    string Description { get; }
    string Id { get; }
    string Print(double value, string? format = null);
}
