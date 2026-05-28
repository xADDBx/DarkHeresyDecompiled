namespace OwlPack.Runtime;

public interface ITypeConverter
{
}
public interface ITypeConverter<TNewType> : ITypeConverter
{
	TNewType Convert(object serializedObject);
}
