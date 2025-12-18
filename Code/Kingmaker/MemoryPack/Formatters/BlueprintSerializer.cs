using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using OwlPack.Runtime;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class BlueprintSerializer<TBlueprint> : AObjectSerializer<TBlueprint> where TBlueprint : BlueprintScriptableObject
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TBlueprint).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Guid", typeof(string))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref TBlueprint? value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<TBlueprint>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "Guid", ref value.AssetGuid, state);
		formatter.EndObject();
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref TBlueprint? value, uint objectId, DeserializerState state)
	{
		string text = "";
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TBlueprint>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				text = formatter.ReadString(state);
				break;
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
		value = ResourcesLibrary.TryGetBlueprint<TBlueprint>(text);
		if (value == null)
		{
			throw new Exception("Blueprint with guid " + text + " not found during deserialization");
		}
		state.References.Register(objectId, value);
	}
}
