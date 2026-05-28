namespace OwlPack.Runtime;

public interface IOwlPackable
{
	void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
public interface IOwlPackable<T>
{
}
