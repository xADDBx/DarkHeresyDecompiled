using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("DOT/DOTLogic")]
[TypeId("f13ba6e499da486e9ec3ddc458c6c110")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Owner
})]
[ContextRole(ContextField.Caster, "buff applier (poisoner)", FallsBackTo = "Owner", Note = "if applier left the area, falls back to victim — damage attribution will go to victim")]
[ContextRole(ContextField.Owner, "the buff holder (the poisoned unit)")]
public class DOTLogic : UnitBuffComponentDelegate, ITickEachRound
{
	public class Settings : ContextData<Settings>
	{
		public SkillType? SaveOverride { get; private set; }

		public int? DifficultyOverride { get; private set; }

		public Settings Setup(SkillType? saveType, int? difficulty)
		{
			SaveOverride = saveType;
			DifficultyOverride = difficulty;
			return this;
		}

		protected override void Reset()
		{
			SaveOverride = null;
			DifficultyOverride = null;
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class Data : IEntityFactComponentSavableData, IHashable, IOwlPackable<Data>
	{
		[JsonProperty]
		[OwlPackInclude]
		public SkillType? SaveTypeOverride;

		[JsonProperty]
		[OwlPackInclude]
		public int? DifficultyOverride;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Data",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("SaveTypeOverride", typeof(SkillType?)),
				new FieldInfo("DifficultyOverride", typeof(int?))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			if (SaveTypeOverride.HasValue)
			{
				SkillType val2 = SaveTypeOverride.Value;
				result.Append(ref val2);
			}
			if (DifficultyOverride.HasValue)
			{
				int val3 = DifficultyOverride.Value;
				result.Append(ref val3);
			}
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Data source = new Data();
			result = Unsafe.As<Data, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Data>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EnumNullableField(0, "SaveTypeOverride", ref SaveTypeOverride, state);
			formatter.UnmanagedNullableField(1, "DifficultyOverride", ref DifficultyOverride, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Data>();
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
					SaveTypeOverride = formatter.ReadNullableEnum<SkillType>(state);
					break;
				case 1:
					DifficultyOverride = formatter.ReadNullableUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	[Obsolete]
	public class PartDOTDirector : MechanicEntityPart, IHashable, IOwlPackable<PartDOTDirector>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "PartDOTDirector",
			OldNames = null,
			Fields = new FieldInfo[0]
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
			PartDOTDirector source = new PartDOTDirector();
			result = Unsafe.As<PartDOTDirector, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<PartDOTDirector>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartDOTDirector>();
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

	public DOT Type;

	public DamageType DamageType;

	public DamageStrategy DamageStrategy;

	public RestrictionCalculator SaveThrowRestrictions = new RestrictionCalculator();

	public SkillType SaveType;

	public int Difficulty;

	public bool CanApplyCriticalEffects;

	protected override void OnInitialize()
	{
		Data data = RequestSavableData<Data>();
		data.SaveTypeOverride = ContextData<Settings>.Current?.SaveOverride;
		data.DifficultyOverride = ContextData<Settings>.Current?.DifficultyOverride;
	}

	void ITickEachRound.OnNewRound()
	{
		DealDamage(base.Buff, this, base.Owner);
	}

	public static void TryDealDamage(Buff buff, bool onlyDamage, MechanicEntity? replaceTarget = null)
	{
		Buff buff = buff;
		MechanicEntity replaceTarget = replaceTarget;
		buff.CallComponents(delegate(DOTLogic logic)
		{
			DealDamage(buff, logic, replaceTarget ?? buff.Owner, onlyDamage);
		});
	}

	public static void TryDealDamage(MechanicEntity target, DOT type, bool damageOnly = false)
	{
		if (TryGetDOT(target, type, out Buff buff, out DOTLogic logic))
		{
			DealDamage(buff, logic, buff.Owner, damageOnly);
		}
	}

	public static int GetCurrentDamageOfType(MechanicEntity target, DOT type)
	{
		if (!TryGetDOT(target, type, out Buff buff, out DOTLogic logic))
		{
			return 0;
		}
		return CalculateDamage(buff, logic, target).AverageValue;
	}

	public static int GetBasicDamageOfType(MechanicEntity target, DOT type)
	{
		if (!TryGetDOT(target, type, out Buff buff, out DOTLogic _))
		{
			return 0;
		}
		return buff.Rank;
	}

	private static void DealDamage(Buff buff, DOTLogic logic, MechanicEntity target, bool onlyDamage = false)
	{
		MechanicEntity initiator = buff.Context.MaybeCaster ?? target;
		IntermediateDamage intermediateDamage = CalculateDamage(buff, logic, target);
		if (!target.Features.HealInsteadOfDamageForDOTs)
		{
			Rulebook.Trigger(new RuleDealDamage(initiator, target, intermediateDamage)
			{
				Reason = buff
			});
		}
		else if (!onlyDamage)
		{
			Rulebook.Trigger(RuleHealDamage.Setup(initiator, target).Base(intermediateDamage.MaxValueBaseWithMaxModifiers).Create());
		}
	}

	public static IntermediateDamage CalculateDamage(Buff buff, DOTLogic logic, MechanicEntity? replaceTarget = null)
	{
		MechanicEntity mechanicEntity = replaceTarget ?? buff.Owner;
		IntermediateDamage resultDamage = Rulebook.Trigger(new RuleCalculateDamage(buff.Context.MaybeCaster ?? mechanicEntity, mechanicEntity, buff.Context.SourceAbility, null, logic.DamageType.CreateDamage(buff.Rank), GetRandomBodyPart(mechanicEntity))
		{
			Reason = buff,
			IsDOT = true
		}).ResultDamage;
		resultDamage.CanApplyCriticalEffects = logic.CanApplyCriticalEffects;
		resultDamage.PushApplyStrategy(logic.DamageStrategy);
		return resultDamage;
	}

	private static BlueprintBodyPart GetRandomBodyPart(MechanicEntity target)
	{
		return Rulebook.Trigger(new RulePerformBodyPartHitRoll(target)).ResultHitLocation;
	}

	private static bool TryGetDOT(MechanicEntity target, DOT type, out Buff buff, out DOTLogic logic)
	{
		buff = null;
		logic = null;
		if (!DOTRoot.Instance.TryGetDOTBuffOfType(type, out var buff2))
		{
			return false;
		}
		Buff buff3 = target.Buffs.GetBuff(buff2);
		if (buff3 == null || buff3.Rank <= 0)
		{
			return false;
		}
		buff = buff3;
		logic = buff3.GetComponent((DOTLogic c) => c.Type == type);
		return true;
	}
}
