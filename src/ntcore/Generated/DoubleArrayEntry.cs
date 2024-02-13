// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

// THIS FILE WAS AUTO-GENERATED BY ./ntcore/generate_topics.py. DO NOT MODIFY

namespace NetworkTables;

/// <summary>
/// NetworkTables DoubleArray entry.
/// </summary>
public interface IDoubleArrayEntry : IDoubleArraySubscriber, IDoubleArrayPublisher
{
    /// <summary>
    /// Stops publishing the entry if its published.
    /// </summary>
    void Unpublish();
}
