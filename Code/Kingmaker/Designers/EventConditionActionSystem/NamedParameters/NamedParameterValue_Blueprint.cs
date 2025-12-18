using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[OwlPackable(OwlPackableMode.Generate)]
public class NamedParameterValue_Blueprint : INamedParameterValue, IOwlPackable, IOwlPackable<INamedParameterValue>, IOwlPackable<NamedParameterValue_Blueprint>
{
	[OwlPackInclude]
	private BlueprintScriptableObjectReference m_Value;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NamedParameterValue_Blueprint",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Value", typeof(BlueprintScriptableObjectReference))
		}
	};

	public object Value => m_Value;

	public NamedParameterValue_Blueprint()
	{
	}

	public NamedParameterValue_Blueprint(BlueprintScriptableObjectReference value)
	{
		m_Value = value;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NamedParameterValue_Blueprint source = new NamedParameterValue_Blueprint();
		result = Unsafe.As<NamedParameterValue_Blueprint, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NamedParameterValue_Blueprint>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Value", ref m_Value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NamedParameterValue_Blueprint>();
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
				m_Value = formatter.ReadPackable<BlueprintScriptableObjectReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
