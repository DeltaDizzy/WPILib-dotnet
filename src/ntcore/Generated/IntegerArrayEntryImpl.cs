// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

// THIS FILE WAS AUTO-GENERATED BY ./ntcore/generate_topics.py. DO NOT MODIFY

using NetworkTables.Handles;
using NetworkTables.Natives;

namespace NetworkTables;

internal sealed class IntegerArrayEntryImpl<T> : EntryBase<T>, IIntegerArrayEntry where T : struct, INtEntryHandle
{
    internal IntegerArrayEntryImpl(IntegerArrayTopic topic, T handle, long[] defaultValue) : base(handle)
    {
        Topic = topic;
        m_defaultValue = defaultValue;
    }

    public override IntegerArrayTopic Topic { get; }

    public long[] Get()
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        if (value.IsIntegerArray)
        {
            return value.GetIntegerArray();
        }
        return m_defaultValue;
    }

    public long[] Get(long[] defaultValue)
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        if (value.IsIntegerArray)
        {
            return value.GetIntegerArray();
        }
        return defaultValue;
    }

    public TimestampedObject<long[]> GetAtomic()
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        long[] baseValue = value.IsIntegerArray ? value.GetIntegerArray() : m_defaultValue;
        return new TimestampedObject<long[]>(value.Time, value.ServerTime, baseValue);
    }

    public TimestampedObject<long[]> GetAtomic(long[] defaultValue)
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        long[] baseValue = value.IsIntegerArray ? value.GetIntegerArray() : defaultValue;
        return new TimestampedObject<long[]>(value.Time, value.ServerTime, baseValue);
    }

    public TimestampedObject<long[]>[] ReadQueue()
    {
        NetworkTableValue[] values = NtCore.ReadQueueValue(Handle);
        TimestampedObject<long[]>[] timestamped = new TimestampedObject<long[]>[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            timestamped[i] = new TimestampedObject<long[]>(values[i].Time, values[i].ServerTime, values[i].GetIntegerArray());
        }
        return timestamped;
    }

    public long[][] ReadQueueValues()
    {
        NetworkTableValue[] values = NtCore.ReadQueueValue(Handle);
        long[][] timestamped = new long[values.Length][];
        for (int i = 0; i < values.Length; i++)
        {
            timestamped[i] = values[i].GetIntegerArray();
        }
        return timestamped;
    }

    public void Set(ReadOnlySpan<long> value)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeIntegerArray(value, 0);
        NtCore.SetEntryValue(Handle, ntValue);
    }

    public void Set(ReadOnlySpan<long> value, long time)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeIntegerArray(value, time);
        NtCore.SetEntryValue(Handle, ntValue);
    }

    public void SetDefault(ReadOnlySpan<long> value)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeIntegerArray(value);
        NtCore.SetDefaultEntryValue(Handle, ntValue);
    }

    public void Unpublish()
    {
        NtCore.Unpublish(Handle);
    }

    private readonly long[] m_defaultValue;
}
