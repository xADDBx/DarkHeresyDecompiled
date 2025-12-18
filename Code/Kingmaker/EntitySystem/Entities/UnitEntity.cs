using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitEntity : BaseUnitEntity, IAreaHandler, ISubscriber, IUnitEntity, IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IHashable, IOwlPackable<UnitEntity>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitEntity",
		OldNames = null,
		Fields = new FieldInfo[28]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("SpawnPosition", typeof(Vector3)),
			new FieldInfo("HoldState", typeof(bool)),
			new FieldInfo("DesiredOrientation", typeof(float)),
			new FieldInfo("Random", typeof(StatefulRandom)),
			new FieldInfo("FlyHeight", typeof(float)),
			new FieldInfo("m_SelectedVoGuid", typeof(string)),
			new FieldInfo("IsSelected", typeof(bool)),
			new FieldInfo("IsLink", typeof(bool)),
			new FieldInfo("TimeToNextRoundTick", typeof(float)),
			new FieldInfo("CachedPerceptionRoll", typeof(int)),
			new FieldInfo("LastRestTime", typeof(TimeSpan?)),
			new FieldInfo("m_AppliedUpgraders", typeof(List<BlueprintUnitUpgrader>)),
			new FieldInfo("m_IsExtra", typeof(bool)),
			new FieldInfo("SpawnFromPsychicPhenomena", typeof(bool))
		}
	};

	public override Type RequiredBlueprintType => typeof(BlueprintUnitFact);

	public override bool BlockOccupiedNodes => base.LifeState.IsConscious;

	public override PartInventory Inventory => GetRequired<PartInventory>();

	public override PartFaction Faction => GetRequired<PartFaction>();

	public override PartUnitProficiency Proficiencies => GetRequired<PartUnitProficiency>();

	public override PartUnitBody Body => GetRequired<PartUnitBody>();

	public override PartAbilityResourceCollection AbilityResources => GetRequired<PartAbilityResourceCollection>();

	public override PartUnitProgression Progression => GetRequired<PartUnitProgression>();

	public override PartUnitAsks Asks => GetRequired<PartUnitAsks>();

	public override PartUnitViewSettings ViewSettings => GetRequired<PartUnitViewSettings>();

	public override PartUnitDescription Description => GetRequired<PartUnitDescription>();

	public override PartVision Vision => GetRequired<PartVision>();

	[Obsolete]
	public override PartUnitStealth Stealth => GetRequired<PartUnitStealth>();

	public override PartUnitCombatState CombatState => GetRequired<PartUnitCombatState>();

	public override PartCombatGroup CombatGroup => GetRequired<PartCombatGroup>();

	public override PartStatsAttributes Attributes => GetRequired<PartStatsAttributes>();

	public override PartStatsSkills Skills => GetRequired<PartStatsSkills>();

	public override PartHealth Health => GetRequired<PartHealth>();

	public UnitEntity(UnitEntityView view)
		: this(view.UniqueId, view.IsInGameBySettings, view.Blueprint)
	{
	}

	public UnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected UnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitEntity()
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartStatsAttributes>();
		GetOrCreate<PartStatsSkills>();
		GetOrCreate<PartUnitViewSettings>();
		GetOrCreate<PartUnitCommands>();
		GetOrCreate<PartUnitCombatState>();
		GetOrCreate<PartCohesion>();
		GetOrCreate<PartFaction>();
		GetOrCreate<PartCombatGroup>();
		GetOrCreate<PartVision>();
		GetOrCreate<PartUnitStealth>();
		GetOrCreate<PartMorale>();
		GetOrCreate<PartUnitProgression>();
		GetOrCreate<PartHealth>();
		GetOrCreate<PartArmor>();
		GetOrCreate<PartLifeState>();
		GetOrCreate<PartMovable>();
		GetOrCreate<PartUnitProficiency>();
		GetOrCreate<PartAbilityResourceCollection>();
		GetOrCreate<PartInventory>();
		GetOrCreate<PartUnitBody>();
		GetOrCreate<PartUnitAsks>();
		GetOrCreate<PartUnitDescription>();
		GetOrCreate<PartTwoWeaponFighting>();
		GetOrCreate<UnitPronePart>();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAreaBeginUnloading()
	{
		if (base.LifeState.IsFinallyDead && !base.Features.SuppressedDecomposition && !IsDeadAndHasLoot && !Faction.IsPlayer)
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(this);
		}
	}

	public override ItemEntityWeapon GetFirstWeapon()
	{
		ItemEntityWeapon maybeWeapon = Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon == null)
		{
			maybeWeapon = Body.SecondaryHand.MaybeWeapon;
			if (maybeWeapon == null)
			{
				WeaponSlot weaponSlot = Body.AdditionalLimbs.FirstItem((WeaponSlot l) => l.MaybeWeapon != null);
				if (weaponSlot == null)
				{
					return null;
				}
				maybeWeapon = weaponSlot.MaybeWeapon;
			}
		}
		return maybeWeapon;
	}

	public override ItemEntityWeapon GetSecondWeapon()
	{
		return Body.SecondaryHand.MaybeWeapon;
	}

	protected override void OnNodeChanged(GraphNode oldNode)
	{
		base.OnNodeChanged(oldNode);
		EventBus.RaiseEvent((IBaseUnitEntity)this, (Action<IUnitNodeChangedHandler>)delegate(IUnitNodeChangedHandler h)
		{
			h.HandleUnitNodeChanged(oldNode);
		}, isCheckRuntime: true);
	}

	protected override void OnApplyPostLoadFixes()
	{
		try
		{
			List<Feature> list = Facts.GetAll<Feature>().ToTempList();
			List<Ability> list2 = Facts.GetAll<Ability>().ToTempList();
			if (list.Count > 0 || list2.Count > 0)
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (ItemSlot allSlot in Body.AllSlots)
				{
					if (allSlot.Active)
					{
						if (allSlot.MaybeItem != null)
						{
							hashSet.Add(allSlot.MaybeItem.UniqueId);
						}
						else if (allSlot is WeaponSlot { MaybeWeapon: { } maybeWeapon })
						{
							hashSet.Add(maybeWeapon.UniqueId);
						}
					}
					if (allSlot.MaybeItem == null && allSlot is HandSlot { MaybeWeapon: { } maybeWeapon2 })
					{
						hashSet.Add(maybeWeapon2.UniqueId);
					}
				}
				while (list.Count > 0 || list2.Count > 0)
				{
					try
					{
						if (list.Count > 0)
						{
							Feature feature = list[0];
							IItemEntity sourceItem = feature.SourceItem;
							if (sourceItem != null && !hashSet.Contains(sourceItem.UniqueId))
							{
								Facts.Remove(feature);
							}
							list.RemoveAt(0);
						}
					}
					catch (Exception ex)
					{
						PFLog.Entity.Exception(ex);
					}
					try
					{
						if (list2.Count <= 0)
						{
							continue;
						}
						Ability ability = list2[list2.Count - 1];
						IItemEntity sourceItem2 = ability.SourceItem;
						EntityFactSource firstSource = ability.FirstSource;
						bool flag = sourceItem2 == null && firstSource == null;
						if ((sourceItem2 != null && !hashSet.Contains(sourceItem2.UniqueId)) || flag)
						{
							Facts.Remove(ability);
						}
						else
						{
							for (int num = list2.Count - 2; num >= 0; num--)
							{
								Ability ability2 = list2[num];
								IItemEntity sourceItem3 = ability2.SourceItem;
								EntityFactSource firstSource2 = ability2.FirstSource;
								bool flag2 = sourceItem3 == null && firstSource2 == null;
								bool flag3 = sourceItem3 != null && !hashSet.Contains(sourceItem3.UniqueId);
								if ((ability.Blueprint == ability2.Blueprint && sourceItem2 != null && sourceItem3 != null && sourceItem2.UniqueId.Equals(sourceItem3.UniqueId)) || flag3 || flag2)
								{
									Facts.Remove(ability2);
									list2.Remove(ability2);
								}
							}
						}
						list2.Remove(ability);
					}
					catch (Exception ex2)
					{
						PFLog.Entity.Exception(ex2);
					}
				}
			}
		}
		catch (Exception ex3)
		{
			PFLog.Entity.Exception(ex3);
		}
		base.OnApplyPostLoadFixes();
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
		UnitEntity source = new UnitEntity();
		result = Unsafe.As<UnitEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		Vector3 value3 = base.SpawnPosition;
		formatter.Field(14, "SpawnPosition", ref value3, state);
		bool value4 = base.HoldState;
		formatter.UnmanagedField(15, "HoldState", ref value4, state);
		float value5 = base.DesiredOrientation;
		formatter.UnmanagedField(16, "DesiredOrientation", ref value5, state);
		StatefulRandom value6 = base.Random;
		formatter.Field(17, "Random", ref value6, state);
		float value7 = base.FlyHeight;
		formatter.UnmanagedField(18, "FlyHeight", ref value7, state);
		formatter.StringField(19, "m_SelectedVoGuid", ref m_SelectedVoGuid, state);
		bool value8 = base.IsSelected;
		formatter.UnmanagedField(20, "IsSelected", ref value8, state);
		bool value9 = base.IsLink;
		formatter.UnmanagedField(21, "IsLink", ref value9, state);
		float value10 = base.TimeToNextRoundTick;
		formatter.UnmanagedField(22, "TimeToNextRoundTick", ref value10, state);
		int value11 = base.CachedPerceptionRoll;
		formatter.UnmanagedField(23, "CachedPerceptionRoll", ref value11, state);
		TimeSpan? value12 = base.LastRestTime;
		formatter.NullableField(24, "LastRestTime", ref value12, state);
		formatter.Field(25, "m_AppliedUpgraders", ref m_AppliedUpgraders, state);
		formatter.UnmanagedField(26, "m_IsExtra", ref m_IsExtra, state);
		bool value13 = base.SpawnFromPsychicPhenomena;
		formatter.UnmanagedField(27, "SpawnFromPsychicPhenomena", ref value13, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitEntity>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				base.SpawnPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 15:
				base.HoldState = formatter.ReadUnmanaged<bool>(state);
				break;
			case 16:
				base.DesiredOrientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 17:
				base.Random = formatter.ReadPackable<StatefulRandom>(state);
				break;
			case 18:
				base.FlyHeight = formatter.ReadUnmanaged<float>(state);
				break;
			case 19:
				m_SelectedVoGuid = formatter.ReadString(state);
				break;
			case 20:
				base.IsSelected = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				base.IsLink = formatter.ReadUnmanaged<bool>(state);
				break;
			case 22:
				base.TimeToNextRoundTick = formatter.ReadUnmanaged<float>(state);
				break;
			case 23:
				base.CachedPerceptionRoll = formatter.ReadUnmanaged<int>(state);
				break;
			case 24:
				base.LastRestTime = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			case 25:
				m_AppliedUpgraders = formatter.ReadPackable<List<BlueprintUnitUpgrader>>(state);
				break;
			case 26:
				m_IsExtra = formatter.ReadUnmanaged<bool>(state);
				break;
			case 27:
				base.SpawnFromPsychicPhenomena = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
