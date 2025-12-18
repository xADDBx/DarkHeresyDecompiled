using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartOverwatch : BaseUnitPart, IHashable, IOwlPackable<PartOverwatch>
{
	[JsonProperty]
	[OwlPackInclude]
	private AbilityData m_Ability;

	[JsonProperty]
	[OwlPackInclude]
	private AbilityData m_AbilityOnTrigger;

	[JsonProperty]
	[OwlPackInclude]
	private TargetWrapper m_Target;

	private readonly HashSet<GridNodeBase> m_OverwatchArea = new HashSet<GridNodeBase>();

	private bool m_AreaIsConfigured;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartOverwatch",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("m_Ability", typeof(AbilityData)),
			new FieldInfo("m_AbilityOnTrigger", typeof(AbilityData)),
			new FieldInfo("m_Target", typeof(TargetWrapper)),
			new FieldInfo("m_GainedBuffs", typeof(List<EntityFactRef<Buff>>)),
			new FieldInfo("m_AttackedUnits", typeof(List<EntityRef<BaseUnitEntity>>)),
			new FieldInfo("Mode", typeof(OverwatchMode)),
			new FieldInfo("HitsPerTarget", typeof(OverwatchHitsPerTarget))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<EntityFactRef<Buff>> m_GainedBuffs { get; set; } = new List<EntityFactRef<Buff>>();


	[JsonProperty]
	[OwlPackInclude]
	private List<EntityRef<BaseUnitEntity>> m_AttackedUnits { get; set; } = new List<EntityRef<BaseUnitEntity>>();


	[JsonProperty]
	[OwlPackInclude]
	public OverwatchMode Mode { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public OverwatchHitsPerTarget HitsPerTarget { get; private set; }

	public bool IsStopped { get; private set; }

	public AbilityData Ability => m_Ability;

	public AbilityData AbilityOnTrigger => m_AbilityOnTrigger;

	public IReadOnlyCollection<GridNodeBase> OverwatchArea => m_OverwatchArea;

	public void Start([NotNull] AbilityData ability, [NotNull] AbilityData abilityOnTrigger, [NotNull] TargetWrapper target, OverwatchMode mode = OverwatchMode.Overwatch, OverwatchHitsPerTarget hitsPerTarget = OverwatchHitsPerTarget.HitOnce)
	{
		Clear();
		m_Ability = ability ?? throw new ArgumentNullException("ability");
		m_AbilityOnTrigger = abilityOnTrigger ?? throw new ArgumentNullException("abilityOnTrigger");
		m_Target = target ?? throw new ArgumentNullException("target");
		AbilityEffectOverwatch component = ability.Blueprint.GetComponent<AbilityEffectOverwatch>();
		if (component == null)
		{
			PFLog.Default.ErrorWithReport($"AbilityEffectOverwatch is missing: {ability}");
			Stop();
			return;
		}
		if (!TrySetupOverwatchArea())
		{
			PFLog.Default.ErrorWithReport("Overwatch area is empty!");
			Stop();
			return;
		}
		Mode = mode;
		HitsPerTarget = hitsPerTarget;
		BlueprintBuffReference[] applyingBuffs = component.ApplyingBuffs;
		foreach (BlueprintBuffReference blueprintBuffReference in applyingBuffs)
		{
			Buff buff = base.Owner.Buffs.Add(blueprintBuffReference, base.Owner);
			m_GainedBuffs.Add(buff);
		}
	}

	private bool TrySetupOverwatchArea()
	{
		if (m_AreaIsConfigured)
		{
			return true;
		}
		m_AreaIsConfigured = true;
		m_OverwatchArea.AddRange(AbilityEffectOverwatch.GetOverwatchArea(Ability, m_Target));
		return !m_OverwatchArea.Empty();
	}

	private void Clear()
	{
		ClearBuffs();
		m_Ability = null;
		m_AbilityOnTrigger = null;
		Mode = OverwatchMode.Overwatch;
		HitsPerTarget = OverwatchHitsPerTarget.HitOnce;
		m_AttackedUnits.Clear();
		m_OverwatchArea.Clear();
		m_AreaIsConfigured = false;
	}

	private void ClearBuffs()
	{
		foreach (EntityFactRef<Buff> gainedBuff in m_GainedBuffs)
		{
			gainedBuff.Fact?.Remove();
		}
		m_GainedBuffs.Clear();
	}

	public void Stop()
	{
		ClearBuffs();
		IsStopped = true;
		RemoveSelf();
	}

	public bool Contains(BaseUnitEntity unit)
	{
		TrySetupOverwatchArea();
		foreach (GridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			if (m_OverwatchArea.Contains(occupiedNode))
			{
				return true;
			}
		}
		return false;
	}

	public void TryTriggerAttack(BaseUnitEntity target)
	{
		if (HitsPerTarget != 0 || !m_AttackedUnits.Contains(target))
		{
			m_AttackedUnits.Add(target);
			UnitOverwatchAttackParams cmdParams = new UnitOverwatchAttackParams(AbilityOnTrigger, target);
			base.Owner.Commands.Run(cmdParams);
			if (Mode == OverwatchMode.Overwatch)
			{
				Stop();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_Ability);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<AbilityData>.GetHash128(m_AbilityOnTrigger);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val4);
		List<EntityFactRef<Buff>> gainedBuffs = m_GainedBuffs;
		if (gainedBuffs != null)
		{
			for (int i = 0; i < gainedBuffs.Count; i++)
			{
				EntityFactRef<Buff> obj = gainedBuffs[i];
				Hash128 val5 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		List<EntityRef<BaseUnitEntity>> attackedUnits = m_AttackedUnits;
		if (attackedUnits != null)
		{
			for (int j = 0; j < attackedUnits.Count; j++)
			{
				EntityRef<BaseUnitEntity> obj2 = attackedUnits[j];
				Hash128 val6 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj2);
				result.Append(ref val6);
			}
		}
		OverwatchMode val7 = Mode;
		result.Append(ref val7);
		OverwatchHitsPerTarget val8 = HitsPerTarget;
		result.Append(ref val8);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartOverwatch source = new PartOverwatch();
		result = Unsafe.As<PartOverwatch, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartOverwatch>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Ability", ref m_Ability, state);
		formatter.Field(1, "m_AbilityOnTrigger", ref m_AbilityOnTrigger, state);
		formatter.Field(2, "m_Target", ref m_Target, state);
		List<EntityFactRef<Buff>> value = m_GainedBuffs;
		formatter.Field(3, "m_GainedBuffs", ref value, state);
		List<EntityRef<BaseUnitEntity>> value2 = m_AttackedUnits;
		formatter.Field(4, "m_AttackedUnits", ref value2, state);
		OverwatchMode value3 = Mode;
		formatter.EnumField(5, "Mode", ref value3, state);
		OverwatchHitsPerTarget value4 = HitsPerTarget;
		formatter.EnumField(6, "HitsPerTarget", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartOverwatch>();
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
				m_Ability = formatter.ReadPackable<AbilityData>(state);
				break;
			case 1:
				m_AbilityOnTrigger = formatter.ReadPackable<AbilityData>(state);
				break;
			case 2:
				m_Target = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 3:
				m_GainedBuffs = formatter.ReadPackable<List<EntityFactRef<Buff>>>(state);
				break;
			case 4:
				m_AttackedUnits = formatter.ReadPackable<List<EntityRef<BaseUnitEntity>>>(state);
				break;
			case 5:
				Mode = formatter.ReadEnum<OverwatchMode>(state);
				break;
			case 6:
				HitsPerTarget = formatter.ReadEnum<OverwatchHitsPerTarget>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
