using System;
using System.Collections.Generic;
using System.Reflection;

namespace OwlPack.Runtime;

public class ExternalTypeLibrary
{
	private static ExternalTypeLibrary s_Instance;

	private Dictionary<Type, ExternalTypeInfo> m_Types = new Dictionary<Type, ExternalTypeInfo>();

	private List<(Type Key, ExternalTypeInfo Value)> m_TypesByBase = new List<(Type, ExternalTypeInfo)>();

	public static ExternalTypeLibrary Instance => s_Instance;

	static ExternalTypeLibrary()
	{
		s_Instance = new ExternalTypeLibrary();
		Instance.RegisterType(typeof(Array), typeof(ArraySerializer));
		Instance.RegisterType(typeof(List<>), typeof(ListSerializer<, >));
		Instance.RegisterType(typeof(HashSet<>), typeof(HashSetSerializer<, >));
		Instance.RegisterType(typeof(Queue<>), typeof(QueueSerializer<, >));
		Instance.RegisterType(typeof(Dictionary<, >), typeof(DictionarySerializer<, , >));
		Instance.RegisterType(typeof(DateTime), typeof(DateTimeSerializer));
		Instance.RegisterType(typeof(TimeSpan), typeof(TimeSpanSerializer));
		Instance.RegisterType(typeof(Guid), typeof(GuidSerializer));
		Instance.RegisterTypeForAllDerived(typeof(Type), typeof(TypeSerializer));
	}

	private bool TryGetValueByBase(Type possiblyDerived, out ExternalTypeInfo result)
	{
		foreach (var item in m_TypesByBase)
		{
			if (item.Key.IsAssignableFrom(possiblyDerived))
			{
				result = item.Value;
				return true;
			}
		}
		result = default(ExternalTypeInfo);
		return false;
	}

	public void RegisterType(Type type, Type serializerClass, TypeInfo typeInfo = null)
	{
		m_Types.Add(type, new ExternalTypeInfo
		{
			SerializerClass = serializerClass,
			TypeInfo = typeInfo
		});
	}

	public void RegisterTypeForAllDerived(Type type, Type serializerClass, TypeInfo typeInfo = null)
	{
		if (type.IsGenericType)
		{
			if (!serializerClass.IsGenericType)
			{
				throw new ArgumentException("Cannot register class " + serializerClass.FullName + " as serializer for types derived from " + type.FullName + ": universal serializer for generic base type must be generic");
			}
			if (serializerClass.GetTypeInfo().GenericTypeParameters.Length - 1 != type.GetTypeInfo().GenericTypeParameters.Length)
			{
				throw new ArgumentException($"Cannot register class {serializerClass.FullName} as serializer for types derived from {type.FullName}: universal serializer accept {type.GetTypeInfo().GenericTypeParameters.Length + 1} generic parameters");
			}
		}
		else if (serializerClass.GetTypeInfo().GenericTypeParameters.Length > 1)
		{
			throw new ArgumentException("Cannot register class " + serializerClass.FullName + " as serializer for types derived from " + type.FullName + ": universal serializer must be non-generic, or a generic type with 1 type parameter");
		}
		m_TypesByBase.Add((type, new ExternalTypeInfo
		{
			SerializerClass = serializerClass,
			TypeInfo = typeInfo
		}));
	}

	public ExternalTypeInfo GetTypeInfo(Type type)
	{
		if (type.IsGenericType)
		{
			type = type.GetGenericTypeDefinition();
		}
		else if (type.IsArray)
		{
			type = typeof(Array);
		}
		if (m_Types.TryGetValue(type, out var value))
		{
			return value;
		}
		if (TryGetValueByBase(type, out value))
		{
			return value;
		}
		throw new Exception("External type " + type.FullName + " is not registered in ExternalTypeLibrary");
	}
}
