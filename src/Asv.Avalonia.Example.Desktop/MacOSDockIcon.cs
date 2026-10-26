using System.Runtime.InteropServices;
using Avalonia.Platform;

namespace Asv.Avalonia.Example;

internal static class MacOSDockIcon
{
    private const string ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";

    public static void Set(Uri resource)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        using var stream = AssetLoader.Open(resource);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var bytes = buffer.ToArray();
        var data = SendBytes(
            Send(GetClass("NSData"), Selector("alloc")),
            Selector("initWithBytes:length:"),
            bytes,
            (nuint)bytes.Length
        );
        try
        {
            var image = SendPointer(
                Send(GetClass("NSImage"), Selector("alloc")),
                Selector("initWithData:"),
                data
            );
            try
            {
                if (image == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not load the macOS Dock icon.");
                }

                // A window icon does not set the process-wide Dock icon for unbundled launches.
                SendVoidPointer(
                    Send(GetClass("NSApplication"), Selector("sharedApplication")),
                    Selector("setApplicationIconImage:"),
                    image
                );
            }
            finally
            {
                SendVoid(image, Selector("release"));
            }
        }
        finally
        {
            SendVoid(data, Selector("release"));
        }
    }

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr GetClass(string name);

    [DllImport(ObjectiveCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr Selector(string name);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr Send(IntPtr receiver, IntPtr selector);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPointer(IntPtr receiver, IntPtr selector, IntPtr value);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendBytes(
        IntPtr receiver,
        IntPtr selector,
        byte[] bytes,
        nuint length
    );

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void SendVoid(IntPtr receiver, IntPtr selector);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidPointer(IntPtr receiver, IntPtr selector, IntPtr value);
}
