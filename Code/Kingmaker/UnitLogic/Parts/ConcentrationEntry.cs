using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Parts;

[Obsolete("Unused")]
[OwlPackable(OwlPackableMode.Generate)]
public class ConcentrationEntry : IOwlPackable, IOwlPackable<ConcentrationEntry>
{
	public int ActionPointCost;

	public int MovementPointCost;

	public BlueprintAbility Ability;

	public BaseUnitEntity Target;

	public List<Buff> Buffs = new List<Buff>();

	public int Index;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConcentrationEntry",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[JsonConstructor]
	private ConcentrationEntry()
	{
	}

	public ConcentrationEntry(BlueprintAbility ability, BaseUnitEntity target)
	{
		Ability = ability;
		Target = target;
		ConcentrationAbility component = ability.GetComponent<ConcentrationAbility>();
		if (component != null)
		{
			ActionPointCost = component.ActionPointCost;
			MovementPointCost = component.MovementPointCost;
		}
	}

	public void RemoveEntry()
	{
		Buff[] array = Buffs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Remove();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ConcentrationEntry source = new ConcentrationEntry();
		result = Unsafe.As<ConcentrationEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConcentrationEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConcentrationEntry>();
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
