using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OwlPack.Runtime;

internal class TypeSerializer : AObjectSerializer<Type>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(Type).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Name", typeof(string))
		},
		Flags = TypeFlags.IsExternal
	};

	private static readonly Regex _shortTypeNameRegex = new Regex(", Version=\\d+.\\d+.\\d+.\\d+, Culture=[\\w-]+, PublicKeyToken=(?:null|[a-f0-9]{16})", RegexOptions.Compiled);

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	private static Regex ShortTypeNameRegex()
	{
		return _shortTypeNameRegex;
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref Type value, uint objectId, DeserializerState state)
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Type>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		value = null;
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
			{
				string typeName = formatter.ReadString(state);
				value = Type.GetType(typeName);
				break;
			}
			}
		}
		formatter.LeaveObject();
	}

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref Type value, SerializerState state)
	{
		if (value == null)
		{
			formatter.NullObject();
			return;
		}
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<Type>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string assemblyQualifiedName = value.AssemblyQualifiedName;
		string value2 = ShortTypeNameRegex().Replace(assemblyQualifiedName, "");
		formatter.StringField(0, "Name", ref value2, state);
		formatter.EndObject();
	}
}
