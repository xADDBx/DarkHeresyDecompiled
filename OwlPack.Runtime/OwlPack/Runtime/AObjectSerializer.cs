using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public abstract class AObjectSerializer<T> : IObjectSerializer, ISerializer
{
	public abstract TypeInfo TypeInfo { get; }

	public void Serialize<TFormatter, TPossiblyBase>(TFormatter formatter, ref TPossiblyBase value, SerializerState state) where TFormatter : IOutputFormatter
	{
		T value2 = Unsafe.As<TPossiblyBase, T>(ref value);
		SerializeInternal(formatter, ref value2, state);
	}

	public void Deserialize<TFormatter, TPossiblyBase>(TFormatter formatter, ref TPossiblyBase value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		T value2 = ((value is T) ? Unsafe.As<TPossiblyBase, T>(ref value) : default(T));
		DeserializeInternal(formatter, ref value2, objectId, state);
		value = Unsafe.As<T, TPossiblyBase>(ref value2);
	}

	protected abstract void SerializeInternal<TFormatter>(TFormatter formatter, ref T? value, SerializerState state) where TFormatter : IOutputFormatter;

	protected abstract void DeserializeInternal<TFormatter>(TFormatter formatter, ref T? value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
