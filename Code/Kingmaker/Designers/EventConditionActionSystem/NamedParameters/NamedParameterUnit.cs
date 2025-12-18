using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("95469ff256aabcf409b9c5860a4c4ba9")]
[OwlPackable(OwlPackableMode.Generate)]
public class NamedParameterUnit : AbstractUnitEvaluator, IOwlPackable<NamedParameterUnit>
{
	public string Parameter;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NamedParameterUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find unit {0} in context parameters", Parameter);
		}
		if (value.Value is string uniqueId)
		{
			UnitReference unitReference = new UnitReference(uniqueId);
			current.Context.Params[Parameter] = new NamedParameterValue_Unit(unitReference);
			return unitReference.ToAbstractUnitEntity();
		}
		return (value.Value as IEntityRef)?.Get<AbstractUnitEntity>();
	}

	public override string GetCaption()
	{
		return "Unit P:" + Parameter;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NamedParameterUnit source = new NamedParameterUnit();
		result = Unsafe.As<NamedParameterUnit, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<NamedParameterUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NamedParameterUnit>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
