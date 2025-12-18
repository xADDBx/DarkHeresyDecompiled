using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Scaling.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartOverrideScaling : MechanicEntityPart, IHashable, IOwlPackable<PartOverrideScaling>
{
	private sealed class Entry
	{
		public readonly EntityFact SourceFact;

		public readonly OverrideScaling SourceComponent;

		public Entry(EntityFact sourceFact, OverrideScaling sourceComponent)
		{
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
		}

		public bool IsSuitable(BlueprintMechanicEntityFact blueprint)
		{
			BpRef<BlueprintMechanicEntityFact>[] suitableFacts = SourceComponent.SuitableFacts;
			foreach (BlueprintMechanicEntityFact blueprintMechanicEntityFact in suitableFacts)
			{
				if (blueprint == blueprintMechanicEntityFact)
				{
					return true;
				}
			}
			ReferenceArrayProxy<BlueprintAbilityGroup>? referenceArrayProxy = (blueprint as BlueprintBuff)?.AbilityGroups;
			if (!referenceArrayProxy.HasValue)
			{
				return false;
			}
			BpRef<BlueprintAbilityGroup>[] suitableAbilityGroups = SourceComponent.SuitableAbilityGroups;
			foreach (BlueprintAbilityGroup bp in suitableAbilityGroups)
			{
				if (referenceArrayProxy.Value.HasReference(bp))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsSuitable(BlueprintAbilityWrapper ability)
		{
			BpRef<BlueprintMechanicEntityFact>[] suitableFacts = SourceComponent.SuitableFacts;
			for (int i = 0; i < suitableFacts.Length; i++)
			{
				if ((BlueprintMechanicEntityFact?)suitableFacts[i] is BlueprintAbility blueprint && ability.SameAbility(blueprint))
				{
					return true;
				}
			}
			BpRef<BlueprintAbilityGroup>[] suitableAbilityGroups = SourceComponent.SuitableAbilityGroups;
			foreach (BlueprintAbilityGroup bp in suitableAbilityGroups)
			{
				if (ability.AbilityGroups.HasReference(bp))
				{
					return true;
				}
			}
			return false;
		}
	}

	private readonly List<Entry> _entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartOverrideScaling",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Add(EntityFact sourceFact, OverrideScaling sourceComponent)
	{
		_entries.Add(new Entry(sourceFact, sourceComponent));
	}

	public void Remove(EntityFact sourceFact, OverrideScaling sourceComponent)
	{
		EntityFact sourceFact = sourceFact;
		OverrideScaling sourceComponent = sourceComponent;
		_entries.RemoveAll((Entry i) => i.SourceFact == sourceFact && i.SourceComponent == sourceComponent);
		if (_entries.Empty())
		{
			RemoveSelf();
		}
	}

	public ScalingInfo? Get(BlueprintMechanicEntityFact blueprint)
	{
		foreach (Entry entry in _entries)
		{
			if (entry.IsSuitable(blueprint))
			{
				return new ScalingInfo(entry.SourceComponent.Calculator, entry.SourceComponent.Description);
			}
		}
		return null;
	}

	public ScalingInfo? Get(BlueprintAbilityWrapper ability)
	{
		foreach (Entry entry in _entries)
		{
			if (entry.IsSuitable(ability))
			{
				return new ScalingInfo(entry.SourceComponent.Calculator, entry.SourceComponent.Description);
			}
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartOverrideScaling source = new PartOverrideScaling();
		result = Unsafe.As<PartOverrideScaling, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartOverrideScaling>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartOverrideScaling>();
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
