using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartPreventInterruption : BaseUnitPart, IHashable, IOwlPackable<UnitPartPreventInterruption>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartPreventInterruption",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_NonInterruptiveAbilities", typeof(List<BlueprintActivatableAbility>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<BlueprintActivatableAbility> m_NonInterruptiveAbilities { get; set; } = new List<BlueprintActivatableAbility>();


	public void AddNonInterruptive(BlueprintActivatableAbility ability)
	{
		m_NonInterruptiveAbilities.Add(ability);
	}

	public void RemoveNonInterruptive(BlueprintActivatableAbility ability)
	{
		m_NonInterruptiveAbilities.Remove(ability);
	}

	public bool CanInterrupt(BlueprintActivatableAbility ability)
	{
		return !m_NonInterruptiveAbilities.Contains(ability);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BlueprintActivatableAbility> nonInterruptiveAbilities = m_NonInterruptiveAbilities;
		if (nonInterruptiveAbilities != null)
		{
			for (int i = 0; i < nonInterruptiveAbilities.Count; i++)
			{
				Hash128 val2 = SimpleBlueprintHasher.GetHash128(nonInterruptiveAbilities[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartPreventInterruption source = new UnitPartPreventInterruption();
		result = Unsafe.As<UnitPartPreventInterruption, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartPreventInterruption>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<BlueprintActivatableAbility> value = m_NonInterruptiveAbilities;
		formatter.Field(0, "m_NonInterruptiveAbilities", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartPreventInterruption>();
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
				m_NonInterruptiveAbilities = formatter.ReadPackable<List<BlueprintActivatableAbility>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
