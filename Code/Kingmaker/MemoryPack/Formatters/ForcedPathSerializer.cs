using System.Collections.Generic;
using Kingmaker.Pathfinding;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class ForcedPathSerializer : AObjectSerializer<ForcedPath>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(ForcedPath).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("vectorPath", typeof(List<Vector3>))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref ForcedPath? value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ForcedPath>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "vectorPath", ref value.vectorPath, state);
		formatter.EndObject();
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref ForcedPath? value, uint objectId, DeserializerState state)
	{
		List<Vector3> vectorPath = null;
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ForcedPath>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				vectorPath = formatter.ReadPackable<List<Vector3>>(state);
				break;
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
		value = new ForcedPath();
		value.vectorPath = vectorPath;
		value.Repair();
		state.References.Register(objectId, value);
	}
}
