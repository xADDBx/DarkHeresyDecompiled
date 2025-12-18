using System.Collections.Generic;
using Kingmaker.Blueprints;
using OwlPack.Runtime;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class BlueprintReferenceSerializer<TBlueprintReference> : AObjectSerializer<TBlueprintReference> where TBlueprintReference : BlueprintReferenceBase, new()
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TBlueprintReference).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Guid", typeof(string))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref TBlueprintReference? value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<TBlueprintReference>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value2 = value.Guid;
		formatter.StringField(0, "Guid", ref value2, state);
		formatter.EndObject();
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref TBlueprintReference? value, uint objectId, DeserializerState state)
	{
		string value2 = "";
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TBlueprintReference>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				value2 = formatter.ReadString(state);
				break;
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
		value = new TBlueprintReference();
		value.ReadGuidFromJson(value2);
		state.References.Register(objectId, value);
	}
}
