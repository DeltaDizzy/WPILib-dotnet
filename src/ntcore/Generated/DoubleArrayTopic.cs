// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

// THIS FILE WAS AUTO-GENERATED BY ./ntcore/generate_topics.py. DO NOT MODIFY

using System;
using NetworkTables.Handles;
using NetworkTables.Natives;

namespace NetworkTables;

/// <summary>
/// NetworkTables DoubleArray topic.
/// </summary>
public class DoubleArrayTopic : Topic
{
    /// <summary>
    /// The default type string for this topic type
    /// </summary>
    public static string kTypeString => "double[]";
    /// <summary>
    /// The default type string for this topic type in a UTF8 Span
    /// </summary>
    public static ReadOnlySpan<byte> kTypeStringUtf8 => "double[]"u8;

    /// <summary>
    /// Constructs a typed topic from a generic topic.
    /// </summary>
    /// <param name="topic">typed topic</param>
    public DoubleArrayTopic(Topic topic) : base(topic.Instance, topic.Handle) { }

    /// <summary>
    /// Constructor; use NetworkTableInstance.GetDoubleArrayTopic() instead.
    /// </summary>
    /// <param name="inst">Instance</param>
    /// <param name="handle">Native handle</param>
    public DoubleArrayTopic(NetworkTableInstance inst, NtTopic handle) : base(inst, handle) { }


    /// <summary>
    /// Create a new subscriver to the topic.
    /// </summary>
    /// <remarks>
    /// The subscriber is only active as long as the returned object is not closed.
    ///
    /// Subscribers that do not match the published data type do not return any
    /// values. To determine if the data type matches, use the appropriate Topic
    /// functions.
    /// </remarks>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">subscribe options</param>
    /// <returns>subscriber</returns>
    public IDoubleArraySubscriber Subscribe(
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtSubscriber>(
            this,
            NtCore.Subscribe(
                Handle, NetworkTableType.DoubleArray,
                "double[]"u8, options),
            defaultValue);
    }

    /// <summary>
    /// Create a new subscriber to the topic, with the specified type string.
    /// </summary>
    /// <remarks>
    /// The subscriber is only active as long as the returned object is not closed.
    ///
    /// Subscribers that do not match the published data type do not return any
    /// values. To determine if the data type matches, use the appropriate Topic
    /// functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">subscribe options</param>
    /// <returns>subscriber</returns>
    public IDoubleArraySubscriber SubscribeEx(
        string typeString,
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtSubscriber>(
            this,
            NtCore.Subscribe(
                Handle, NetworkTableType.DoubleArray,
                typeString, options),
            defaultValue);
    }

    /// <summary>
    /// Create a new subscriber to the topic, with the specified type string.
    /// </summary>
    /// <remarks>
    /// The subscriber is only active as long as the returned object is not closed.
    ///
    /// Subscribers that do not match the published data type do not return any
    /// values. To determine if the data type matches, use the appropriate Topic
    /// functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">subscribe options</param>
    /// <returns>subscriber</returns>
    public IDoubleArraySubscriber SubscribeEx(
        ReadOnlySpan<byte> typeString,
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtSubscriber>(
            this,
            NtCore.Subscribe(
                Handle, NetworkTableType.DoubleArray,
                typeString, options),
            defaultValue);
    }

    /// <summary>
    /// Create a new publisher to the topic.
    /// </summary>
    /// <remarks>
    /// The publisher is only active as long as the returned object is not closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="options">publish options</param>
    /// <returns>publisher</returns>
    public IDoubleArrayPublisher Publish(
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtPublisher>(
            this,
            NtCore.Publish(
                Handle, NetworkTableType.DoubleArray,
                "double[]"u8, options),
            []);
    }

    /// <summary>
    /// Create a new publisher to the topic, with type string and initial properties.
    /// </summary>
    /// <remarks>
    /// The publisher is only active as long as the returned object is not closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="properties">JSON properties</param>
    /// <param name="options">publish options</param>
    /// <returns>publisher</returns>
    public IDoubleArrayPublisher PublishEx(
        string typeString, string properties,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtPublisher>(
            this,
            NtCore.PublishEx(
                Handle, NetworkTableType.DoubleArray,
                typeString, properties, options),
            []);
    }

    /// <summary>
    /// Create a new publisher to the topic, with type string and initial properties.
    /// </summary>
    /// <remarks>
    /// The publisher is only active as long as the returned object is not closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="properties">JSON properties</param>
    /// <param name="options">publish options</param>
    /// <returns>publisher</returns>
    public IDoubleArrayPublisher PublishEx(
        ReadOnlySpan<byte> typeString,
        string properties,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtPublisher>(
            this,
            NtCore.PublishEx(
                Handle, NetworkTableType.DoubleArray,
                typeString, properties, options),
            []);
    }

    /// <summary>
    /// Create a new publisher to the topic, with type string and initial properties.
    /// </summary>
    /// <remarks>
    /// The publisher is only active as long as the returned object is not closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="properties">JSON properties</param>
    /// <param name="options">publish options</param>
    /// <returns>publisher</returns>
    public IDoubleArrayPublisher PublishEx(
        string typeString,
        ReadOnlySpan<byte> properties,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtPublisher>(
            this,
            NtCore.PublishEx(
                Handle, NetworkTableType.DoubleArray,
                typeString, properties, options),
            []);
    }

    /// <summary>
    /// Create a new publisher to the topic, with type string and initial properties.
    /// </summary>
    /// <remarks>
    /// The publisher is only active as long as the returned object is not closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="properties">JSON properties</param>
    /// <param name="options">publish options</param>
    /// <returns>publisher</returns>
    public IDoubleArrayPublisher PublishEx(
        ReadOnlySpan<byte> typeString,
        ReadOnlySpan<byte> properties,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtPublisher>(
            this,
            NtCore.PublishEx(
                Handle, NetworkTableType.DoubleArray,
                typeString, properties, options),
            []);
    }

    /// <summary>
    /// Create a new entry for the topic.
    /// </summary>
    /// <remarks>
    /// Entries act as a combination of a subscriber and a weak publisher. The
    /// subscriber is active as long as the entry is not closed. The publisher is
    /// created when the entry is first written to, and remains active until either
    /// Unpublish() is called or the entry is closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">publish and/or subscribe options</param>
    /// <returns>entry</returns>
    public IDoubleArrayEntry GetEntry(
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtEntry>(
            this,
            NtCore.GetEntry(
                Handle, NetworkTableType.DoubleArray,
                "double[]"u8, options),
            defaultValue);
    }

    /// <summary>
    /// Create a new entry for the topic, with the specified type string.
    /// </summary>
    /// <remarks>
    /// Entries act as a combination of a subscriber and a weak publisher. The
    /// subscriber is active as long as the entry is not closed. The publisher is
    /// created when the entry is first written to, and remains active until either
    /// Unpublish() is called or the entry is closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">publish and/or subscribe options</param>
    /// <returns>entry</returns>
    public IDoubleArrayEntry GetEntryEx(
        string typeString,
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtEntry>(
            this,
            NtCore.GetEntry(
                Handle, NetworkTableType.DoubleArray,
                typeString, options),
            defaultValue);
    }

    /// <summary>
    /// Create a new entry for the topic, with the specified type string.
    /// </summary>
    /// <remarks>
    /// Entries act as a combination of a subscriber and a weak publisher. The
    /// subscriber is active as long as the entry is not closed. The publisher is
    /// created when the entry is first written to, and remains active until either
    /// Unpublish() is called or the entry is closed.
    ///
    /// It is not possible to publish two different data types to the same topic.
    /// Conflicts between publishers are typically resolved by the server on a
    /// first-come, first-served basis. Any published values that do not match
    /// the topic's data type are dropped (ignored). To determine if the data
    /// type matches, use tha appropriate Topic functions.
    /// </remarks>
    /// <param name="typeString">type string</param>
    /// <param name="defaultValue">
    /// default value used when a default is not provided to a getter function
    /// </param>
    /// <param name="options">publish and/or subscribe options</param>
    /// <returns>entry</returns>
    public IDoubleArrayEntry GetEntryEx(
        ReadOnlySpan<byte> typeString,
        double[] defaultValue,
        PubSubOptions options)
    {
        return new DoubleArrayEntryImpl<NtEntry>(
            this,
            NtCore.GetEntry(
                Handle, NetworkTableType.DoubleArray,
                typeString, options),
            defaultValue);
    }

}
