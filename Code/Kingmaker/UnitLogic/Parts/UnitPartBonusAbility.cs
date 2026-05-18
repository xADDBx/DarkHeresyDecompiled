using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartBonusAbility : BaseUnitPart, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, IEventTag<IUnitCommandActHandler, EntitySubscriber>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IInterruptTurnEndHandler<EntitySubscriber>, IInterruptTurnEndHandler, IEventTag<IInterruptTurnEndHandler, EntitySubscriber>, IHashable, IOwlPackable<UnitPartBonusAbility>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class BonusAbilityData : IHashable, IOwlPackable, IOwlPackable<BonusAbilityData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "BonusAbilityData",
			OldNames = null,
			Fields = new FieldInfo[5]
			{
				new FieldInfo("Count", typeof(int)),
				new FieldInfo("CostBonus", typeof(int)),
				new FieldInfo("Source", typeof(EntityFactSource)),
				new FieldInfo("Restrictions", typeof(RestrictionsHolder.Reference)),
				new FieldInfo("IgnoreAbilityRestrictionForUsage", typeof(bool))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public int Count { get; set; }

		[JsonProperty]
		[OwlPackInclude]
		public int CostBonus { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public EntityFactSource Source { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public RestrictionsHolder.Reference Restrictions { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public bool IgnoreAbilityRestrictionForUsage { get; private set; }

		[JsonConstructor]
		public BonusAbilityData(int count, EntityFactSource source, int costBonus, RestrictionsHolder.Reference restrictions, bool ignoreAbilityRestrictionForUsage)
		{
			Count = count;
			Source = source;
			CostBonus = costBonus;
			Restrictions = restrictions;
			IgnoreAbilityRestrictionForUsage = ignoreAbilityRestrictionForUsage;
		}

		public BonusAbilityData()
		{
		}

		public bool IsCorrectAbility(AbilityData data)
		{
			if (Restrictions?.Get()?.IsPassed(data.Caster, null, null, null, data) ?? true)
			{
				if (!IgnoreAbilityRestrictionForUsage)
				{
					return !data.IsRestricted;
				}
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"Source={Source}, count={Count}, costBonus={CostBonus}, restrictions={Restrictions}, ignorePartAbilityRestrictions={IgnoreAbilityRestrictionForUsage}";
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			int val = Count;
			result.Append(ref val);
			int val2 = CostBonus;
			result.Append(ref val2);
			Hash128 val3 = ClassHasher<EntityFactSource>.GetHash128(Source);
			result.Append(ref val3);
			Hash128 val4 = BlueprintReferenceHasher.GetHash128(Restrictions);
			result.Append(ref val4);
			bool val5 = IgnoreAbilityRestrictionForUsage;
			result.Append(ref val5);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			BonusAbilityData source = new BonusAbilityData();
			result = Unsafe.As<BonusAbilityData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<BonusAbilityData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			int value = Count;
			formatter.UnmanagedField(0, "Count", ref value, state);
			int value2 = CostBonus;
			formatter.UnmanagedField(1, "CostBonus", ref value2, state);
			EntityFactSource value3 = Source;
			formatter.Field(2, "Source", ref value3, state);
			RestrictionsHolder.Reference value4 = Restrictions;
			formatter.Field(3, "Restrictions", ref value4, state);
			bool value5 = IgnoreAbilityRestrictionForUsage;
			formatter.UnmanagedField(4, "IgnoreAbilityRestrictionForUsage", ref value5, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<BonusAbilityData>();
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
					Count = formatter.ReadUnmanaged<int>(state);
					break;
				case 1:
					CostBonus = formatter.ReadUnmanaged<int>(state);
					break;
				case 2:
					Source = formatter.ReadPackable<EntityFactSource>(state);
					break;
				case 3:
					Restrictions = formatter.ReadPackable<RestrictionsHolder.Reference>(state);
					break;
				case 4:
					IgnoreAbilityRestrictionForUsage = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartBonusAbility",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Bonuses", typeof(List<BonusAbilityData>))
		}
	};

	private static LogChannel Logger => BonusAbilityExtension.Logger;

	[JsonProperty]
	[OwlPackInclude]
	private List<BonusAbilityData> m_Bonuses { get; set; } = new List<BonusAbilityData>();


	public bool HasBonusAbilityUsage(AbilityData ability)
	{
		return m_Bonuses.Any((BonusAbilityData x) => x.IsCorrectAbility(ability));
	}

	private BonusAbilityData GetBestBonusAbilityUsage(AbilityData ability)
	{
		BonusAbilityData bonusAbilityData = null;
		for (int i = 0; i < m_Bonuses.Count; i++)
		{
			BonusAbilityData bonusAbilityData2 = m_Bonuses[i];
			if (bonusAbilityData2.IsCorrectAbility(ability) && (bonusAbilityData == null || bonusAbilityData2.CostBonus < bonusAbilityData.CostBonus))
			{
				bonusAbilityData = bonusAbilityData2;
			}
		}
		return bonusAbilityData;
	}

	public void AddBonusAbility(EntityFactSource source, int count, int costBonus, RestrictionsHolder.Reference restrictions, bool ignoreAbilityRestrictionForUsage)
	{
		BonusAbilityData bonusAbilityData = new BonusAbilityData(count, source, costBonus, restrictions, ignoreAbilityRestrictionForUsage);
		m_Bonuses.Add(bonusAbilityData);
		Logger.Log($"Add bonus ability usage. {bonusAbilityData}. Owner={base.Owner}");
	}

	public void RemoveBonusAbility(BlueprintFact source)
	{
		m_Bonuses.RemoveAll((BonusAbilityData p) => p.Source.Blueprint == source);
	}

	private void Reset()
	{
		m_Bonuses.Clear();
		Logger.Log($"Reset bonus ability usages. Owner={base.Owner}");
		RemoveSelfIfEmpty();
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		BonusAbilityData bestBonusAbilityUsage = GetBestBonusAbilityUsage(evt.AbilityData);
		if (bestBonusAbilityUsage != null)
		{
			if (bestBonusAbilityUsage.CostBonus > 0)
			{
				evt.AddCostIncrease(bestBonusAbilityUsage.CostBonus);
			}
			else
			{
				evt.AddCostDecrease(bestBonusAbilityUsage.CostBonus, -1);
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		Reset();
	}

	public void HandleUnitEndInterruptTurn()
	{
		Reset();
	}

	public void HandleUnitJoinCombat()
	{
		Reset();
	}

	public void HandleUnitLeaveCombat()
	{
		Reset();
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (evt.ForceFreeAction)
		{
			return;
		}
		BonusAbilityData bestBonusAbilityUsage = GetBestBonusAbilityUsage(evt.Ability);
		if (bestBonusAbilityUsage != null)
		{
			bestBonusAbilityUsage.Count--;
			if (bestBonusAbilityUsage.Count <= 0)
			{
				m_Bonuses.Remove(bestBonusAbilityUsage);
			}
			Logger.Log($"Reduce bonus ability usage count. {bestBonusAbilityUsage}. Owner={base.Owner}");
			RemoveSelfIfEmpty();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_Bonuses.Count == 0)
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BonusAbilityData> bonuses = m_Bonuses;
		if (bonuses != null)
		{
			for (int i = 0; i < bonuses.Count; i++)
			{
				Hash128 val2 = ClassHasher<BonusAbilityData>.GetHash128(bonuses[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartBonusAbility source = new UnitPartBonusAbility();
		result = Unsafe.As<UnitPartBonusAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartBonusAbility>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<BonusAbilityData> value = m_Bonuses;
		formatter.Field(0, "m_Bonuses", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartBonusAbility>();
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
				m_Bonuses = formatter.ReadPackable<List<BonusAbilityData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
