﻿// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

// THIS FILE WAS AUTO-GENERATED BY ./ntcore/generate_topics.py. DO NOT MODIFY

using System;
using NetworkTables.Handles;
using NetworkTables.Natives;

namespace NetworkTables;

/** NetworkTables Double implementation. */
internal sealed class DoubleEntryImpl<T> : EntryBase<T>, IDoubleEntry where T : struct, INtEntryHandle
{
    /**
     * Constructor.
     *
     * @param topic Topic
     * @param handle Native handle
     * @param defaultValue Default value for Get()
     */
    internal DoubleEntryImpl(DoubleTopic topic, T handle, double defaultValue) : base(handle)
    {
        Topic = topic;
        m_defaultValue = defaultValue;
    }

    public override DoubleTopic Topic { get; }


    public double Get()
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        if (value.IsDouble)
        {
            return value.GetDouble();
        }
        return m_defaultValue;
    }


    public double Get(double defaultValue)
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        if (value.IsDouble)
        {
            return value.GetDouble();
        }
        return defaultValue;
    }


    public TimestampedDouble GetAtomic()
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        double baseValue = value.IsDouble ? value.GetDouble() : m_defaultValue;
        return new TimestampedDouble(value.Time, value.ServerTime, baseValue);
    }


    public TimestampedDouble GetAtomic(double defaultValue)
    {
        NetworkTableValue value = NtCore.GetEntryValue(Handle);
        double baseValue = value.IsDouble ? value.GetDouble() : defaultValue;
        return new TimestampedDouble(value.Time, value.ServerTime, baseValue);
    }


    public TimestampedDouble[] ReadQueue()
    {
        NetworkTableValue[] values = NtCore.ReadQueueValue(Handle);
        TimestampedDouble[] timestamped = new TimestampedDouble[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            timestamped[i] = new TimestampedDouble(values[i].Time, values[i].ServerTime, values[i].GetDouble());
        }
        return timestamped;
    }


    public double[] ReadQueueValues()
    {
        NetworkTableValue[] values = NtCore.ReadQueueValue(Handle);
        double[] timestamped = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            timestamped[i] = values[i].GetDouble();
        }
        return timestamped;
    }


    public void Set(double value)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeDouble(0, value);
        NtCore.SetEntryValue(Handle, ntValue);
    }

    public void Set(long time, double value)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeDouble(time, value);
        NtCore.SetEntryValue(Handle, ntValue);
    }

    public void SetDefault(double value)
    {
        RefNetworkTableValue ntValue = RefNetworkTableValue.MakeDouble(value);
        NtCore.SetDefaultEntryValue(Handle, ntValue);
    }
    public void Unpublish()
    {
        NtCore.Unpublish(Handle);
    }

    private readonly double m_defaultValue;
}
