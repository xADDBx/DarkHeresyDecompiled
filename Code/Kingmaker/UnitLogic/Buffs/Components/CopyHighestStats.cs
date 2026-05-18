using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("Stats/CopyHighestStats")]
[TypeId("c6dfa9198ecc52e48b4a7e5e0a6bd161")]
public class CopyHighestStats : UnitBuffComponentDelegate, IStatModifier
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[OwlPackInclude]
		public Dictionary<StatType, int> Deltas = new Dictionary<StatType, int>();

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Deltas", typeof(Dictionary<StatType, int>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SavableData source = new SavableData();
			result = Unsafe.As<SavableData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<SavableData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Deltas", ref Deltas, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavableData>();
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
					Deltas = formatter.ReadPackable<Dictionary<StatType, int>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private static readonly HashSet<StatType> _Attributes = new HashSet<StatType>
	{
		StatType.BallisticSkill,
		StatType.WeaponSkill,
		StatType.Strength,
		StatType.Toughness,
		StatType.Agility,
		StatType.Intelligence,
		StatType.Willpower,
		StatType.Perception,
		StatType.Fellowship
	};

	[SerializeField]
	private bool m_FromCasterToTarget;

	protected override void OnActivate()
	{
		BaseUnitEntity baseUnitEntity = (m_FromCasterToTarget ? EvalContext.Root.Caster : EvalContext.Root.Target?.Entity) as BaseUnitEntity;
		BaseUnitEntity owner = base.Owner;
		if (baseUnitEntity == null || owner == null)
		{
			return;
		}
		SavableData savableData = RequestSavableData<SavableData>();
		savableData.Deltas.Clear();
		foreach (StatType attribute in _Attributes)
		{
			int modifiedValue = baseUnitEntity.Actor.Stats.GetStat(attribute).ModifiedValue;
			int modifiedValue2 = owner.Actor.Stats.GetStat(attribute).ModifiedValue;
			int num = modifiedValue - modifiedValue2;
			if (num > 0)
			{
				savableData.Deltas[attribute] = num;
			}
		}
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (_Attributes.Contains(stat) && RequestSavableData<SavableData>().Deltas.TryGetValue(stat, out var value))
		{
			collector.Modifiers.Add(ModifierType.ValAdd, value, base.Fact, null, BonusType.None, StatType.Unknown, ModifierDescriptor.UntypedStackable);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		foreach (StatType attribute in _Attributes)
		{
			entries.Add(new AffectedStatEntry(attribute));
		}
	}
}
