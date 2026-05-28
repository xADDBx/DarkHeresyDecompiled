using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace OwlPack.Runtime;

public class TypeLibrary : IOwlPackable<TypeLibrary>, IOwlPackable
{
	internal class SerializedTypeInfo : IOwlPackable, IOwlPackable<SerializedTypeInfo>
	{
		public Type Type;

		public TypeInfo TypeInfo;

		public static readonly Regex ShortTypeNameRegex = new Regex(", Version=\\d+.\\d+.\\d+.\\d+, Culture=[\\w-]+, PublicKeyToken=(?:null|[a-f0-9]{16})", RegexOptions.Compiled);

		private string m_TypeName;

		public static TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = typeof(SerializedTypeInfo).FullName,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Type", typeof(string)),
				new FieldInfo("TypeInfo", typeof(TypeInfo))
			}
		};

		internal string TypeNameString => m_TypeName;

		public string TypeName
		{
			get
			{
				if (m_TypeName != null)
				{
					return m_TypeName;
				}
				string assemblyQualifiedName = Type.AssemblyQualifiedName;
				m_TypeName = ShortTypeNameRegex.Replace(assemblyQualifiedName, "");
				return m_TypeName;
			}
			set
			{
				Type = Type.GetType(value, throwOnError: false);
				m_TypeName = value;
			}
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SerializedTypeInfo source = new SerializedTypeInfo();
			result = Unsafe.As<SerializedTypeInfo, TPossiblyBase>(ref source);
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SerializedTypeInfo>();
			List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
			state.References.Register(objectId, this);
			formatter.EnterObject();
			for (int i = 0; i < typeInfo.Fields.Length; i++)
			{
				formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
				switch (mappingForType[fieldID])
				{
				case 0:
					TypeName = formatter.ReadString(state);
					break;
				case 1:
					TypeInfo = Serializer.DeserializeObject<TypeInfo>(formatter, state);
					break;
				case byte.MaxValue:
					formatter.SkipField(size);
					break;
				}
			}
			formatter.LeaveObject();
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort typeID = state.TypeLibrary.GetTypeID<SerializedTypeInfo>();
			formatter.StartObject(typeID, OwlPackTypeInfo.Name, objectId);
			string value = TypeName;
			formatter.StringField(0, "Type", ref value, state);
			formatter.Field(1, "TypeInfo", ref TypeInfo, state);
			formatter.EndObject();
		}
	}

	private delegate void CreationDelegate<T>(ref T result);

	public const ushort ID_Byte = 0;

	public const ushort ID_SByte = 1;

	public const ushort ID_Short = 2;

	public const ushort ID_UShort = 3;

	public const ushort ID_Int = 4;

	public const ushort ID_UInt = 5;

	public const ushort ID_Long = 6;

	public const ushort ID_ULong = 7;

	public const ushort ID_Float = 8;

	public const ushort ID_Double = 9;

	public const ushort ID_Char = 10;

	public const ushort ID_Bool = 11;

	private const ushort UnmanagedTypesCount = 12;

	public const ushort ID_String = 12;

	public const ushort ID_Enum = 13;

	public static ushort BasicTypesCount;

	public const int MaxTypes = 32766;

	private Dictionary<Type, ushort> m_TypeToID = new Dictionary<Type, ushort>();

	private Dictionary<ushort, Type> m_IDToType = new Dictionary<ushort, Type>();

	private List<SerializedTypeInfo> m_Types = new List<SerializedTypeInfo>();

	private Dictionary<ushort, IObjectSerializer> m_ExternalSerializerCache = new Dictionary<ushort, IObjectSerializer>(128);

	private Dictionary<(Type, ushort), Delegate> m_CreationDelegates = new Dictionary<(Type, ushort), Delegate>();

	private static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TypeLibrary).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Types", typeof(List<SerializedTypeInfo>))
		}
	};

	public static Dictionary<string, Type> OldNames { get; set; } = new Dictionary<string, Type>();


	public TypeLibrary()
	{
		RegisterBasicTypes();
		BasicTypesCount = (ushort)m_Types.Count;
	}

	private void RegisterBasicTypes()
	{
		RegisterType<byte>();
		RegisterType<sbyte>();
		RegisterType<short>();
		RegisterType<ushort>();
		RegisterType<int>();
		RegisterType<uint>();
		RegisterType<long>();
		RegisterType<ulong>();
		RegisterType<float>();
		RegisterType<double>();
		RegisterType<char>();
		RegisterType<bool>();
		RegisterType<string>();
		RegisterType<Enum>();
		RegisterType<TypeLibrary>(OwlPackTypeInfo);
		RegisterType<TypeInfo>(TypeInfo.OwlPackTypeInfo);
		RegisterType<FieldInfo>(FieldInfo.OwlPackTypeInfo);
		RegisterType<SerializedTypeInfo>(SerializedTypeInfo.OwlPackTypeInfo);
		RegisterType<Array>(ArraySerializer.OwlPackTypeInfo);
		RegisterType<List<SerializedTypeInfo>>(ListSerializer<List<SerializedTypeInfo>, SerializedTypeInfo>.OwlPackTypeInfo);
		RegisterType<List<TypeInfo>>(ListSerializer<List<TypeInfo>, TypeInfo>.OwlPackTypeInfo);
	}

	public ushort RegisterType<T>(TypeInfo? typeInfo = null)
	{
		return RegisterType(typeof(T), typeInfo);
	}

	private ushort RegisterType(Type type, TypeInfo? typeInfo = null)
	{
		if (m_TypeToID.TryGetValue(type, out var value))
		{
			return value;
		}
		if (m_Types.Count == 32766)
		{
			throw new Exception($"Too many packable types (including variations of generic), maximum number is {32766}");
		}
		value = (ushort)m_Types.Count;
		m_TypeToID.Add(type, value);
		m_IDToType.Add(value, type);
		m_Types.Add(new SerializedTypeInfo
		{
			Type = type,
			TypeInfo = typeInfo
		});
		return value;
	}

	internal bool ReplaceType(Type original, Type modified, TypeInfo modifiedTypeInfo)
	{
		for (ushort num = 0; num < m_Types.Count; num++)
		{
			if (m_Types[num].Type == original)
			{
				m_Types[num].Type = modified;
				m_Types[num].TypeInfo = modifiedTypeInfo;
				m_TypeToID[modified] = num;
				m_TypeToID.Remove(original);
				m_IDToType[num] = modified;
				return true;
			}
		}
		return false;
	}

	public ushort GetTypeID(Type type)
	{
		if (m_TypeToID.TryGetValue(type, out var value))
		{
			return value;
		}
		throw new Exception($"Type {type} is not registered in TypeLibrary");
	}

	public bool TryGetTypeID(Type type, out ushort id)
	{
		return m_TypeToID.TryGetValue(type, out id);
	}

	public ushort GetTypeID<T>()
	{
		return GetTypeID(typeof(T));
	}

	private void OnDeserialized_Types()
	{
		for (ushort num = BasicTypesCount; num < m_Types.Count; num++)
		{
			if (m_Types[num].Type != null)
			{
				m_TypeToID.Add(m_Types[num].Type, num);
				m_IDToType.Add(num, m_Types[num].Type);
			}
		}
	}

	public TypeInfo GetTypeInfo<T>()
	{
		if (!m_TypeToID.TryGetValue(typeof(T), out var value))
		{
			throw new Exception("Type " + typeof(T).FullName + " not found in m_TypeToID");
		}
		return m_Types[value].TypeInfo;
	}

	public TypeInfo GetTypeInfo(Type type)
	{
		if (!m_TypeToID.TryGetValue(type, out var value))
		{
			throw new Exception("Type " + type.FullName + " not found in m_TypeToID");
		}
		return m_Types[value].TypeInfo;
	}

	public TypeInfo GetTypeInfo(ushort typeID)
	{
		return m_Types[typeID].TypeInfo;
	}

	public Type GetTypeByID(ushort typeID)
	{
		return m_Types[typeID].Type;
	}

	public IObjectSerializer GetExternalTypeSerializer(Type type)
	{
		ExternalTypeInfo typeInfo = ExternalTypeLibrary.Instance.GetTypeInfo(type);
		if (!TryGetTypeID(type, out var id) || !m_ExternalSerializerCache.ContainsKey(id))
		{
			if (type.IsGenericType && typeInfo.SerializerClass.GetTypeInfo().GenericTypeParameters.Length > 1)
			{
				List<Type> list = new List<Type> { type };
				list.AddRange(type.GenericTypeArguments);
				IObjectSerializer objectSerializer = (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass.MakeGenericType(list.ToArray()));
				id = RegisterType(type, objectSerializer.TypeInfo);
				m_ExternalSerializerCache.Add(id, objectSerializer);
			}
			else if (typeInfo.SerializerClass.IsGenericType)
			{
				Type[] typeArguments = new Type[1] { type };
				IObjectSerializer objectSerializer2 = (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass.MakeGenericType(typeArguments));
				id = RegisterType(type, objectSerializer2.TypeInfo);
				m_ExternalSerializerCache.Add(id, objectSerializer2);
			}
			else
			{
				IObjectSerializer objectSerializer3 = (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass);
				id = RegisterType(type, objectSerializer3.TypeInfo);
				m_ExternalSerializerCache.Add(id, objectSerializer3);
			}
		}
		return m_ExternalSerializerCache[id];
	}

	public IObjectSerializer GetExternalTypeSerializer(ushort typeID)
	{
		if (!m_IDToType.TryGetValue(typeID, out var value))
		{
			throw new Exception($"External type {typeID} ({m_Types[typeID].TypeNameString}) not registered");
		}
		ExternalTypeInfo typeInfo = ExternalTypeLibrary.Instance.GetTypeInfo(value);
		if (!m_ExternalSerializerCache.ContainsKey(typeID))
		{
			if (value.IsGenericType && typeInfo.SerializerClass.GetTypeInfo().GenericTypeParameters.Length > 1)
			{
				List<Type> list = new List<Type> { value };
				list.AddRange(value.GenericTypeArguments);
				m_ExternalSerializerCache.Add(typeID, (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass.MakeGenericType(list.ToArray())));
			}
			else if (typeInfo.SerializerClass.IsGenericType)
			{
				Type[] typeArguments = new Type[1] { value };
				m_ExternalSerializerCache.Add(typeID, (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass.MakeGenericType(typeArguments)));
			}
			else
			{
				m_ExternalSerializerCache.Add(typeID, (IObjectSerializer)Activator.CreateInstance(typeInfo.SerializerClass));
			}
		}
		return m_ExternalSerializerCache[typeID];
	}

	public void CreateObject<TPossiblyBase>(ushort typeID, ref TPossiblyBase result)
	{
		m_CreationDelegates.TryGetValue((typeof(TPossiblyBase), typeID), out var value);
		if ((object)value == null)
		{
			if (!m_IDToType.TryGetValue(typeID, out var value2))
			{
				throw new Exception($"Type with id {typeID} ({m_Types[typeID].TypeNameString}) not found");
			}
			try
			{
				MethodInfo method = value2.GetMethod("CreateForDeserialization", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method == null)
				{
					throw new Exception("CreateForDeserialization method not found in type '" + value2.FullName + "'.");
				}
				value = method.MakeGenericMethod(typeof(TPossiblyBase)).CreateDelegate(typeof(CreationDelegate<TPossiblyBase>));
				m_CreationDelegates.Add((typeof(TPossiblyBase), typeID), value);
			}
			catch (ArgumentException)
			{
				string fullName = typeof(TPossiblyBase).FullName;
				throw new Exception("Unable to instantinate Create method for type " + fullName + ": ArgumentException");
			}
		}
		(value as CreationDelegate<TPossiblyBase>)(ref result);
	}

	public void UpdateWithOldNames(Dictionary<string, Type> oldNames)
	{
		if (oldNames == null || oldNames.Count == 0)
		{
			return;
		}
		for (ushort num = 0; num < m_Types.Count; num++)
		{
			if (oldNames.TryGetValue(m_Types[num].TypeName, out var value))
			{
				m_Types[num].TypeName = value.AssemblyQualifiedName;
				m_IDToType[num] = value;
				m_TypeToID[value] = num;
			}
			if (m_Types[num].Type == null)
			{
				_ = m_Types[num].TypeName;
				Match match = Regex.Match(m_Types[num].TypeName, "^(?<genericType>.+?)\\`(?<arity>\\d+)\\[(?<args>.+)\\](, (?<assemblyName>.+))?$");
				if (match.Success)
				{
					string text = match.Groups["genericType"].Value;
					string value2 = match.Groups["args"].Value;
					string text2 = (match.Groups["assemblyName"].Success ? match.Groups["assemblyName"].Value : null);
					string text3 = text + "`" + match.Groups["arity"].Value;
					if (!string.IsNullOrEmpty(text2))
					{
						text3 = text3 + ", " + text2;
					}
					if (oldNames.TryGetValue(text3, out var value3))
					{
						string fullName = value3.FullName;
						int num2 = fullName.LastIndexOf('`');
						if (num2 >= 0)
						{
							text = fullName.Substring(0, num2);
						}
						text2 = value3.Assembly.GetName().Name;
					}
					string text4 = value2;
					foreach (KeyValuePair<string, Type> oldName in oldNames)
					{
						string text5 = oldName.Value.FullName + ", " + oldName.Value.Assembly.GetName().Name;
						text4 = text4.Replace("[" + oldName.Key + ",", "[" + text5 + ",").Replace("[" + oldName.Key + "]", "[" + text5 + "]");
					}
					string text6 = text + "`" + match.Groups["arity"].Value + "[" + text4 + "]";
					if (!string.IsNullOrEmpty(text2))
					{
						text6 = text6 + ", " + text2;
					}
					m_Types[num].TypeName = text6;
					if (m_Types[num].Type != null)
					{
						m_IDToType[num] = m_Types[num].Type;
						m_TypeToID[m_Types[num].Type] = num;
					}
				}
			}
		}
	}

	public static bool TypesEqual(Type t1, Type t2)
	{
		if (t1.IsGenericType && t1.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			t1 = t1.GetGenericArguments()[0];
		}
		if (t2.IsGenericType && t2.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			t2 = t2.GetGenericArguments()[0];
		}
		return t1 == t2;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TypeLibrary source = new TypeLibrary();
		result = Unsafe.As<TypeLibrary, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort typeID = state.TypeLibrary.GetTypeID<TypeLibrary>();
		formatter.StartObject(typeID, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Types", ref m_Types, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TypeLibrary>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		state.References.Register(objectId, this);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				m_Types = Serializer.DeserializeObject<List<SerializedTypeInfo>>(formatter, state);
				OnDeserialized_Types();
				break;
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
