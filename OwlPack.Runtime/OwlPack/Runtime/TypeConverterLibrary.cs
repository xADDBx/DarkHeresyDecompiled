using System;
using System.Collections.Generic;

namespace OwlPack.Runtime;

public class TypeConverterLibrary
{
	private static TypeConverterLibrary s_Instance = new TypeConverterLibrary();

	private Dictionary<(Type, Type), ITypeConverter> m_Converters = new Dictionary<(Type, Type), ITypeConverter>();

	public static TypeConverterLibrary Instance => s_Instance;

	public void RegisterConverter<TOldType, TNewType>(ITypeConverter<TNewType> converter)
	{
		m_Converters.Add((typeof(TOldType), typeof(TNewType)), converter);
	}

	public void Clear()
	{
		m_Converters.Clear();
	}

	public ITypeConverter<TNewType> GetConverter<TNewType>(Type serializedType)
	{
		if (m_Converters.TryGetValue((serializedType, typeof(TNewType)), out var value))
		{
			return value as ITypeConverter<TNewType>;
		}
		return null;
	}
}
