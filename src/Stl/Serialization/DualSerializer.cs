using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Stl.Internal;
using Stl.IO;
using Stl.Serialization.Internal;

namespace Stl.Serialization;

public sealed record DualSerializer<T>(
    DataFormat DefaultFormat,
    IByteSerializer<T> ByteSerializer,
    ITextSerializer<T> TextSerializer)
{
    public IByteSerializer<T> DefaultSerializer { get; } =
        DefaultFormat == DataFormat.Text ? TextSerializer : ByteSerializer;

    public DualSerializer()
        : this(DataFormat.Bytes)
    { }

    public DualSerializer(DataFormat defaultFormat)
        : this(
            defaultFormat,
            Serialization.ByteSerializer.Default.ToTyped<T>(),
            Serialization.TextSerializer.Default.ToTyped<T>())
    { }

    public DualSerializer(IByteSerializer<T> byteSerializer)
        : this(DataFormat.Bytes, byteSerializer, NoneTextSerializer<T>.Instance)
    { }

    public DualSerializer(ITextSerializer<T> textSerializer)
        : this(DataFormat.Text, NoneByteSerializer<T>.Instance, textSerializer)
    { }

    // Read

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public T Read(TextOrBytes data)
        => data.Format == DataFormat.Text
            ? TextSerializer.Read(data.Data)
            : ByteSerializer.Read(data.Data, out _);

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public T Read(ReadOnlyMemory<byte> data)
        => Read(data, DefaultFormat);
    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public T Read(ReadOnlyMemory<byte> data, DataFormat format)
        => format == DataFormat.Text
            ? TextSerializer.Read(data, out _)
            : ByteSerializer.Read(data, out _);

    // Write

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public TextOrBytes Write(T value)
        => Write(value, DefaultFormat);

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public TextOrBytes Write(T value, DataFormat format)
    {
        using var bufferWriter = new ArrayPoolBuffer<byte>();
        if (format == DataFormat.Text)
            TextSerializer.Write(bufferWriter, value);
        else
            ByteSerializer.Write(bufferWriter, value);
        return new TextOrBytes(format, bufferWriter.WrittenSpan.ToArray());
    }

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public void Write(IBufferWriter<byte> bufferWriter, T value)
        => DefaultSerializer.Write(bufferWriter, value);

    [RequiresUnreferencedCode(UnreferencedCode.Serialization)]
    public void Write(IBufferWriter<byte> bufferWriter, T value, DataFormat format)
    {
        if (format == DataFormat.Text)
            TextSerializer.Write(bufferWriter, value);
        else
            ByteSerializer.Write(bufferWriter, value);
    }
}
