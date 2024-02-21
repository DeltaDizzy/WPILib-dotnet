using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using WPIUtil.Marshal;

namespace WPIUtil;

[NativeMarshalling(typeof(WpiStringMarshaller))]
public readonly ref struct WpiString
{
    public readonly ReadOnlySpan<byte> buffer;
    public readonly bool isString;

    public WpiString(ReadOnlySpan<char> buffer)
    {
        this.buffer = MemoryMarshal.AsBytes(buffer);
        isString = true;
    }

    public WpiString(ReadOnlySpan<byte> buffer)
    {
        this.buffer = buffer;
        isString = false;
    }

    public static implicit operator WpiString(string? buffer)
    {
        return new(buffer);
    }

    public static implicit operator WpiString(ReadOnlySpan<byte> buffer)
    {
        return new(buffer);
    }

    public static implicit operator WpiString(Span<byte> buffer)
    {
        return new(buffer);
    }

    public static implicit operator WpiString(ReadOnlySpan<char> buffer)
    {
        return new(buffer);
    }

    public static implicit operator WpiString(Span<char> buffer)
    {
        return new(buffer);
    }

    public static implicit operator WpiString(byte[] buffer)
    {
        return new(buffer);
    }
}
