using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class Buff : UnitFact<BlueprintBuff>, IInitiativeHolder, IFactWithRanks, IBuff, IHashable, IOwlPackable<Buff>
{
	public static class Scope
	{
		public sealed class Caster : SimpleContextData<MechanicEntity, Caster>
		{
		}

		public sealed class RankDelta : SimpleContextData<int, RankDelta>
		{
		}

		public sealed class Parent : SimpleContextData<MechanicEntityFact, Parent>
		{
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private int m_DurationInRounds;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private bool m_Expired;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_CombatInitiativeHolder;

	private IAbstractUnitEntityView m_ParticleEffectOwner;

	private List<GameObject> m_ManagedEffects;

	private List<GameObject> m_TrailFxObjects;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Buff",
		OldNames = null,
		Fields = new FieldInfo[21]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_ParentContext", typeof(MechanicsContext)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("m_DurationInRounds", typeof(int)),
			new FieldInfo("m_Expired", typeof(bool)),
			new FieldInfo("m_CombatInitiativeHolder", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("EndCondition", typeof(BuffEndCondition)),
			new FieldInfo("ExpireMoment", typeof(BuffExpireMoment)),
			new FieldInfo("Initiative", typeof(Initiative)),
			new FieldInfo("Rank", typeof(int)),
			new FieldInfo("RoundNumber", typeof(int)),
			new FieldInfo("PlayedFirstHitSound", typeof(bool)),
			new FieldInfo("IsSuppressed", typeof(bool)),
			new FieldInfo("DisabledBecauseOfNotCasterTurn", typeof(bool))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public BuffEndCondition EndCondition { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public BuffExpireMoment ExpireMoment { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public Initiative Initiative { get; private set; } = new Initiative();


	[JsonProperty]
	[OwlPackInclude]
	public int Rank { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int RoundNumber { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool PlayedFirstHitSound { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IsSuppressed { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool DisabledBecauseOfNotCasterTurn { get; set; }

	public int DurationInRounds => m_DurationInRounds;

	public int ExpirationInRounds
	{
		get
		{
			if (!IsPermanent)
			{
				return Math.Max(0, DurationInRounds - RoundNumber);
			}
			return int.MaxValue;
		}
	}

	public bool IsExpired => m_Expired;

	public bool IsPermanent => DurationInRounds == 0;

	public bool IsProne => base.Blueprint == ConfigRoot.Instance.SystemMechanics.ProneCommonBuff;

	public bool IsDisabled => base.Blueprint == ConfigRoot.Instance.SystemMechanics.DisabledCommonBuff;

	public override string Name
	{
		get
		{
			if (!base.Name.IsNullOrEmpty())
			{
				return base.Name;
			}
			EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
			if (!(entityFactSource != null))
			{
				return "";
			}
			return ((ItemEntity)entityFactSource.Entity)?.Name ?? "";
		}
	}

	public override string Description
	{
		get
		{
			string description = base.Description;
			string description2;
			if (!description.IsNullOrEmpty())
			{
				description2 = description;
			}
			else
			{
				EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
				description2 = ((!(entityFactSource != null)) ? "" : (((ItemEntity)entityFactSource.Entity)?.Description ?? ""));
			}
			return UtilityAbilities.GetLongOrShortText(description2, state: false);
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled && !IsSuppressed)
			{
				return !DisabledBecauseOfNotCasterTurn;
			}
			return false;
		}
	}

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden)
			{
				return base.Blueprint.IsHiddenInUI;
			}
			return true;
		}
	}

	Initiative IInitiativeHolder.Initiative => Initiative;

	public BlueprintAbilityFXSettings FXSettings => base.Blueprint.FXSettings;

	public bool ShouldBeDisabledOutOfCasterTurn => base.Blueprint.GetComponent<DisableBuffOutOfCasterTurn>() != null;

	MechanicEntity IBuff.Caster => base.Context.MaybeCaster ?? base.Owner;

	TargetWrapper IBuff.Target => base.Owner;

	BlueprintAbilityFXSettings IBuff.FXSettings => FXSettings;

	public string FlavorText => base.Blueprint.FlavorText;

	protected override bool SupportsMultipleSources()
	{
		if (base.Blueprint.HasRanks)
		{
			return base.Blueprint.Ranks > 1;
		}
		return false;
	}

	public Buff([NotNull] BlueprintBuff blueprint, IEvalContext parentContext, BuffDuration duration, int rank = 1)
		: base(blueprint, parentContext)
	{
		if (rank < 1)
		{
			throw new ArgumentOutOfRangeException("rank");
		}
		EndCondition = duration.EndCondition;
		ExpireMoment = duration.ExpireMoment;
		SetDuration(duration);
		Rank = rank;
	}

	private Buff(JsonConstructorMark _)
	{
	}

	private Buff()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		IsSuppressed = base.Owner.GetOptional<UnitPartBuffSuppress>()?.IsSuppressed(this) ?? false;
		UpdateCombatInitiative();
	}

	public void UpdateCombatInitiative()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		MechanicEntity currentUnit = turnController.CurrentUnit;
		MechanicEntity mechanicEntity;
		if (currentUnit != null)
		{
			Initiative initiative = currentUnit.Initiative;
			if (initiative != null && initiative.InterruptingOrder > 0)
			{
				mechanicEntity = turnController.CurrentRoundUnitsOrder.Skip(1).FirstOrDefault();
				goto IL_004c;
			}
		}
		mechanicEntity = turnController.CurrentUnit;
		goto IL_004c;
		IL_004c:
		MechanicEntity mechanicEntity2 = mechanicEntity;
		MechanicEntity mechanicEntity3 = base.Blueprint.Initiative switch
		{
			BlueprintBuff.InitiativeType.ByCaster => base.Context.MaybeCaster ?? base.Owner, 
			BlueprintBuff.InitiativeType.ByOwner => base.Owner, 
			BlueprintBuff.InitiativeType.Current => mechanicEntity2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		bool flag = turnController.InCombat && (mechanicEntity3?.IsInCombat ?? false);
		Initiative initiative2 = (flag ? mechanicEntity3.Initiative : null);
		Initiative.Value = initiative2?.Value ?? 0f;
		Initiative.Order = initiative2?.Order ?? 0;
		m_CombatInitiativeHolder = (flag ? mechanicEntity3 : null);
		if (initiative2 != null && initiative2.TurnOrderPriority >= mechanicEntity2?.Initiative.TurnOrderPriority)
		{
			Initiative.LastTurn = turnController.GameRound;
		}
	}

	protected override void OnComponentsDidActivated()
	{
		base.OnComponentsDidActivated();
		SpawnParticleEffect();
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		ClearParticleEffect();
	}

	public void NextRound()
	{
		RoundNumber++;
		CallComponents(delegate(ITickEachRound l)
		{
			l.OnNewRound();
		});
	}

	public void OnRemove()
	{
		m_ParticleEffectOwner = null;
		CallComponents(delegate(IBuffRemoved l)
		{
			l.OnRemoved();
		});
	}

	private List<GameObject> SureTrailFxList()
	{
		return m_TrailFxObjects ?? (m_TrailFxObjects = new List<GameObject>());
	}

	public void SpawnParticleEffect()
	{
		if (base.Owner?.View != null && base.Owner.View.Data != null && !IsParticleEffectValid())
		{
			base.EventBus.RaiseEvent<IBuffEffectHandler>(BuffEffectsSpawnHandler);
			SpawnFxFromBuffComponent();
			m_ParticleEffectOwner = base.Owner.View;
		}
	}

	public void SpawnFxFromBuffComponent()
	{
		if (base.Context.ClickedTarget.Entity?.View == null || base.Context.MaybeCaster?.View == null)
		{
			return;
		}
		foreach (BuffSpawnFx component in base.Blueprint.GetComponents<BuffSpawnFx>())
		{
			component.Spawn(base.Context, base.Context.ClickedTarget, component.DestroyOnDeAttach ? SureTrailFxList() : null);
		}
	}

	private void BuffEffectsSpawnHandler(IBuffEffectHandler h)
	{
		GameObject[] array = h.OnBuffEffectApplied(this);
		if (m_ManagedEffects == null)
		{
			m_ManagedEffects = new List<GameObject>(array.Length);
		}
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null && !gameObject.TryGetComponent<AutoDestroy>(out var _))
			{
				m_ManagedEffects.Add(gameObject);
			}
		}
	}

	private bool IsParticleEffectValid()
	{
		if (m_ManagedEffects == null || m_ManagedEffects.Count == 0)
		{
			return false;
		}
		if (m_ManagedEffects.Any((GameObject effect) => GameObjectsPool.IsInPool(effect)))
		{
			return false;
		}
		if (m_ParticleEffectOwner != base.Owner.View)
		{
			return false;
		}
		return true;
	}

	public void ClearParticleEffect()
	{
		base.EventBus.RaiseEvent(delegate(IBuffEffectHandler h)
		{
			h.OnBuffEffectRemoved(this);
		});
		if (m_ManagedEffects == null || m_ManagedEffects.Count <= 0)
		{
			return;
		}
		foreach (GameObject managedEffect in m_ManagedEffects)
		{
			FxHelper.Destroy(managedEffect);
		}
		m_ParticleEffectOwner = null;
		m_ManagedEffects.Clear();
	}

	public void Remove()
	{
		base.Manager?.Remove(this);
	}

	public void MarkExpired()
	{
		m_Expired = true;
	}

	protected override void OnDetach()
	{
		if (m_TrailFxObjects != null)
		{
			foreach (GameObject trailFxObject in m_TrailFxObjects)
			{
				FxHelper.Destroy(trailFxObject);
			}
			m_TrailFxObjects = null;
		}
		m_ParticleEffectOwner = null;
		ClearParticleEffect();
		base.OnDetach();
	}

	public override int GetRank()
	{
		return Rank;
	}

	public void AddRank(int count = 1, MechanicEntity caster = null)
	{
		if (caster == null)
		{
			caster = SimpleContextData<MechanicEntity, Scope.Caster>.Current ?? base.Caster;
		}
		if (count <= 0)
		{
			return;
		}
		int num = Math.Clamp(Rank + count, 0, base.Blueprint.MaxRank);
		if (num <= Rank)
		{
			return;
		}
		count = num - Rank;
		try
		{
			m_IsReapplying.Retain();
			bool isActive = base.IsActive;
			if (isActive)
			{
				Deactivate();
			}
			Rank = num;
			if (isActive)
			{
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
		if (base.Owner != null)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
			{
				h.HandleBuffRankIncreased(this, count, caster);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveRank(int count = 1, MechanicEntity caster = null)
	{
		if (caster == null)
		{
			caster = SimpleContextData<MechanicEntity, Scope.Caster>.Current ?? base.Caster;
		}
		if (count <= 0)
		{
			return;
		}
		int num = Math.Clamp(Rank - count, 0, base.Blueprint.MaxRank);
		if (num >= Rank)
		{
			return;
		}
		if (num < 1)
		{
			Remove();
			return;
		}
		count = Math.Abs(num - Rank);
		try
		{
			m_IsReapplying.Retain();
			bool isActive = base.IsActive;
			if (isActive)
			{
				Deactivate();
			}
			Rank = num;
			if (isActive)
			{
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
		if (base.Owner != null)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
			{
				h.HandleBuffRankDecreased(this, count, caster);
			}, isCheckRuntime: true);
		}
	}

	public void SetDuration(BuffDuration duration)
	{
		m_DurationInRounds = duration.Rounds?.Value ?? 0;
	}

	public void SetDuration(int durationInRounds)
	{
		m_DurationInRounds = durationInRounds;
	}

	public void Prolong(Rounds? rounds)
	{
		if (IsPermanent)
		{
			return;
		}
		if (!rounds.HasValue || rounds == Rounds.Infinity)
		{
			SetDuration(null);
			return;
		}
		Rounds rounds2 = ExpirationInRounds.Rounds();
		Rounds value = rounds2;
		Rounds? rounds3 = rounds;
		if (value < rounds3)
		{
			IncreaseDuration(rounds - rounds2);
		}
	}

	public void MakePermanent()
	{
		m_DurationInRounds = 0;
	}

	public void IncreaseDuration(Rounds? rounds)
	{
		if (!IsPermanent)
		{
			if (!rounds.HasValue || rounds == Rounds.Infinity)
			{
				SetDuration(null);
				return;
			}
			int value = DurationInRounds.Rounds().Value + rounds.Value.Value;
			SetDuration(new Rounds(value));
		}
	}

	public void UpdateIsExpired(Initiative.Event @event, MechanicEntity sourceEntity)
	{
		if (EndCondition != 0 && !Game.Instance.Player.IsInCombat)
		{
			MarkExpired();
			return;
		}
		MechanicEntity entity = m_CombatInitiativeHolder.Entity;
		MechanicEntity mechanicEntity = ((entity != null && entity.IsInCombat) ? entity : null);
		bool flag = ((mechanicEntity != null) ? (sourceEntity == mechanicEntity) : (Initiative.Value >= sourceEntity?.Initiative.Value || @event == Initiative.Event.RoundEnd));
		bool flag2 = ExpireMoment switch
		{
			BuffExpireMoment.TurnStart => @event == Initiative.Event.TurnPreStart || mechanicEntity == null, 
			BuffExpireMoment.TurnEnd => @event == Initiative.Event.TurnEnd || mechanicEntity == null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if ((ExpirationInRounds <= 0 || (ExpirationInRounds == 1 && @event == Initiative.Event.TurnPreStart)) && flag && flag2)
		{
			MarkExpired();
		}
	}

	public void RemoveSource(EntityFact fact, BlueprintComponent component = null)
	{
		EntityFactSource source = base.Sources.FirstItem((EntityFactSource i) => i.IsFrom(fact, component));
		RemoveSource(source);
	}

	public override IUIDataProvider SelectUIData(UIDataType type)
	{
		IUIDataProvider iUIDataProvider = base.SelectUIData(type);
		if (iUIDataProvider == null)
		{
			MechanicsContext parentContext = m_ParentContext;
			if (parentContext == null)
			{
				return null;
			}
			iUIDataProvider = parentContext.SelectUIData(type);
		}
		return iUIDataProvider;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_DurationInRounds);
		result.Append(ref m_Expired);
		EntityRef<MechanicEntity> obj = m_CombatInitiativeHolder;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		BuffEndCondition val3 = EndCondition;
		result.Append(ref val3);
		BuffExpireMoment val4 = ExpireMoment;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<Initiative>.GetHash128(Initiative);
		result.Append(ref val5);
		int val6 = Rank;
		result.Append(ref val6);
		int val7 = RoundNumber;
		result.Append(ref val7);
		bool val8 = PlayedFirstHitSound;
		result.Append(ref val8);
		bool val9 = IsSuppressed;
		result.Append(ref val9);
		bool val10 = DisabledBecauseOfNotCasterTurn;
		result.Append(ref val10);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Buff source = new Buff();
		result = Unsafe.As<Buff, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Buff>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.Field(8, "m_ParentContext", ref m_ParentContext, state);
		formatter.Field(9, "m_Context", ref m_Context, state);
		formatter.UnmanagedField(10, "m_DurationInRounds", ref m_DurationInRounds, state);
		formatter.UnmanagedField(11, "m_Expired", ref m_Expired, state);
		formatter.Field(12, "m_CombatInitiativeHolder", ref m_CombatInitiativeHolder, state);
		BuffEndCondition value5 = EndCondition;
		formatter.EnumField(13, "EndCondition", ref value5, state);
		BuffExpireMoment value6 = ExpireMoment;
		formatter.EnumField(14, "ExpireMoment", ref value6, state);
		Initiative value7 = Initiative;
		formatter.Field(15, "Initiative", ref value7, state);
		int value8 = Rank;
		formatter.UnmanagedField(16, "Rank", ref value8, state);
		int value9 = RoundNumber;
		formatter.UnmanagedField(17, "RoundNumber", ref value9, state);
		bool value10 = PlayedFirstHitSound;
		formatter.UnmanagedField(18, "PlayedFirstHitSound", ref value10, state);
		bool value11 = IsSuppressed;
		formatter.UnmanagedField(19, "IsSuppressed", ref value11, state);
		bool value12 = DisabledBecauseOfNotCasterTurn;
		formatter.UnmanagedField(20, "DisabledBecauseOfNotCasterTurn", ref value12, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Buff>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_ParentContext = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 9:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 10:
				m_DurationInRounds = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				m_Expired = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_CombatInitiativeHolder = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 13:
				EndCondition = formatter.ReadEnum<BuffEndCondition>(state);
				break;
			case 14:
				ExpireMoment = formatter.ReadEnum<BuffExpireMoment>(state);
				break;
			case 15:
				Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 16:
				Rank = formatter.ReadUnmanaged<int>(state);
				break;
			case 17:
				RoundNumber = formatter.ReadUnmanaged<int>(state);
				break;
			case 18:
				PlayedFirstHitSound = formatter.ReadUnmanaged<bool>(state);
				break;
			case 19:
				IsSuppressed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 20:
				DisabledBecauseOfNotCasterTurn = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
