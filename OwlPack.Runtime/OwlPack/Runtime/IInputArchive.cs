namespace OwlPack.Runtime;

public interface IInputArchive
{
	T Deserialize<T>();
}
