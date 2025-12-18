using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[OwlPackable(OwlPackableMode.Generate)]
public class NamedParameterValue_MapObject : INamedParameterValue, IOwlPackable, IOwlPackable<INamedParameterValue>, IOwlPackable<NamedParameterValue_MapObject>
{
	[OwlPackInclude]
	private MapObjectEntity m_Value;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NamedParameterValue_MapObject",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Value", typeof(MapObjectEntity))
		}
	};

	public object Value => m_Value;

	public NamedParameterValue_MapObject()
	{
	}

	public NamedParameterValue_MapObject(MapObjectEntity value)
	{
		m_Value = value;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NamedParameterValue_MapObject source = new NamedParameterValue_MapObject();
		result = Unsafe.As<NamedParameterValue_MapObject, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<NamedParameterValue_MapObject>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Value", ref m_Value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NamedParameterValue_MapObject>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
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
				m_Value = formatter.ReadPackable<MapObjectEntity>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
