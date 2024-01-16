using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace WPIHal;

[NativeMarshalling(typeof(MatchInfoMarshaller))]
[StructLayout(LayoutKind.Auto)]
public record struct MatchInfo(string EventName, MatchType MatchType, int MatchNumber, int ReplayNumber, byte[] GameSpecificMessage);

[CustomMarshaller(typeof(MatchInfo), MarshalMode.ManagedToUnmanagedOut, typeof(MatchInfoMarshaller))]
public static class MatchInfoMarshaller
{
    public static unsafe MatchInfo ConvertToManaged(NativeMatchInfo unmanaged)
    {
        return new MatchInfo
        {
            EventName = unmanaged.eventName.FromNullTerminatedString(),
            MatchType = unmanaged.matchType,
            MatchNumber = unmanaged.matchNumber,
            ReplayNumber = unmanaged.replayNumber,
            GameSpecificMessage = unmanaged.gameSpecificMessage.FromRawBytes(unmanaged.gameSpecificMessageSize)
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMatchInfo
    {
        [System.Runtime.CompilerServices.InlineArray(64)]
        public struct Utf8StringBuffer
        {
            private byte _element0;

            public readonly unsafe string FromNullTerminatedString()
            {
                ReadOnlySpan<byte> thisSpan = this;
                fixed (byte* b = thisSpan)
                {
                    return Marshal.PtrToStringUTF8((nint)b)!;
                }
            }

            public readonly unsafe byte[] FromRawBytes(int length)
            {
                byte[] ret = new byte[int.Min(length, 64)];
                ReadOnlySpan<byte> thisSpan = this;
                thisSpan[ret.Length..].CopyTo(ret.AsSpan());
                return ret;
            }
        }

        public Utf8StringBuffer eventName;
        public MatchType matchType;
        public ushort matchNumber;
        public byte replayNumber;
        public Utf8StringBuffer gameSpecificMessage;
        public ushort gameSpecificMessageSize;
    }
}
