namespace OwlPack.Runtime;

public interface IObjectSerializer : ISerializer
{
	TypeInfo TypeInfo { get; }

	void Serialize<TFormatter, TPossiblyBase>(TFormatter formatter, ref TPossiblyBase value, SerializerState state) where TFormatter : IOutputFormatter;

	void Deserialize<TFormatter, TPossiblyBase>(TFormatter formatter, ref TPossiblyBase value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
