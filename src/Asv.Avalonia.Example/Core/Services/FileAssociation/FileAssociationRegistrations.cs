using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class FileAssociationRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterFileAssociation()
        {
            builder.AppBuilder.FileAssociation.Register<TextFileHandler>();
            return builder;
        }
    }
}
