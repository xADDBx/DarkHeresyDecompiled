using System.Collections.Generic;
using OwlPack.Runtime;
using UnityEngine;

namespace OwlPack.Unity;

public class Vector2Serializer : AObjectSerializer<Vector2>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Vector2",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("x", typeof(float)),
			new FieldInfo("y", typeof(float))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	public static void Register()
	{
		ExternalTypeLibrary.Instance.RegisterType(typeof(Vector2), typeof(Vector2Serializer), OwlPackTypeInfo);
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref Vector2 value, uint objectId, DeserializerState state)
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Vector2>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		float x = 0f;
		float y = 0f;
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
				x = formatter.ReadUnmanaged<float>(state);
				break;
			case 1:
				y = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
		value = new Vector2(x, y);
	}

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref Vector2 value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<Vector2>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "x", ref value.x, state);
		formatter.UnmanagedField(1, "y", ref value.y, state);
		formatter.EndObject();
	}
}
