using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("5abcc51ecc3df064ebb6c4ec13a8a8e9")]
[OwlPackable(OwlPackableMode.Generate)]
public class SpawnedUnit : AbstractUnitEvaluator, IOwlPackable<SpawnedUnit>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SpawnedUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		SpawnedUnitData current = ContextData<SpawnedUnitData>.Current;
		if (current != null)
		{
			return current.Unit;
		}
		Dictionary<string, INamedParameterValue> dictionary = ContextData<NamedParametersContext.ContextData>.Current?.Context?.Params;
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue("Spawned", out var value))
		{
			if (value.Value is string uniqueId)
			{
				UnitReference unitReference = new UnitReference(uniqueId);
				dictionary["Spawned"] = new NamedParameterValue_Unit(unitReference);
				return unitReference.ToAbstractUnitEntity();
			}
			return (value.Value as IEntityRef)?.Get<AbstractUnitEntity>();
		}
		return null;
	}

	public override string GetCaption()
	{
		return "Spawned Unit";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SpawnedUnit source = new SpawnedUnit();
		result = Unsafe.As<SpawnedUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SpawnedUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SpawnedUnit>();
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
