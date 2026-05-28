using System;
using System.Collections.Generic;

namespace OwlPack.Runtime;

internal class DateTimeSerializer : AObjectSerializer<DateTime>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(DateTime).FullName,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Ticks", typeof(long))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref DateTime value, uint objectId, DeserializerState state)
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DateTime>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		value = default(DateTime);
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
				long dateData = formatter.ReadUnmanaged<long>(state);
				value = DateTime.FromBinary(dateData);
				break;
			}
			}
		}
		formatter.LeaveObject();
	}

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref DateTime value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<DateTime>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		long value2 = value.ToBinary();
		formatter.UnmanagedField(0, "Ticks", ref value2, state);
		formatter.EndObject();
	}
}
