using System;
using System.Collections.Generic;

namespace OwlPack.Runtime;

internal class GuidSerializer : AObjectSerializer<Guid>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(Guid).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("String", typeof(string))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref Guid value, uint objectId, DeserializerState state)
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Guid>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		value = default(Guid);
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
				string input = formatter.ReadString(state);
				value = Guid.Parse(input);
				break;
			}
			}
		}
		formatter.LeaveObject();
	}

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref Guid value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<Guid>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value2 = value.ToString();
		formatter.StringField(0, "String", ref value2, state);
		formatter.EndObject();
	}
}
