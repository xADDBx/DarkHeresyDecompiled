using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Concentration.Events;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Channeling;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Concentration/ChannelingLogic")]
[TypeId("28c5f32eff1b41a0a49e048b1c916cd4")]
public class ChannelingLogic : UnitBuffComponentDelegate, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, ITurnBasedModeHandler
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public EntityRef<InitiativeHolder> InitiativeHolder;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("InitiativeHolder", typeof(EntityRef<InitiativeHolder>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			EntityRef<InitiativeHolder> obj = InitiativeHolder;
			Hash128 val2 = StructHasher<EntityRef<ChannelingLogic.InitiativeHolder>>.GetHash128(ref obj);
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "InitiativeHolder", ref InitiativeHolder, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					InitiativeHolder = formatter.ReadPackable<EntityRef<InitiativeHolder>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class InitiativeHolder : MechanicEntity, ICombatParticipant, IHashable, IOwlPackable<InitiativeHolder>
	{
		[JsonProperty]
		[OwlPackInclude]
		private EntityFactRef<Buff> m_Buff;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "InitiativeHolder",
			OldNames = null,
			Fields = new FieldInfo[15]
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
				new FieldInfo("m_Buff", typeof(EntityFactRef<Buff>))
			}
		};

		public Buff Buff => m_Buff;

		[NotNull]
		public MechanicEntity Unit => ((MechanicEntity)m_Buff.Entity) ?? throw new NullReferenceException();

		public MechanicEntity MaybeUnit => (MechanicEntity)m_Buff.Entity;

		public override bool NeedsView => false;

		public override bool IsInCombat => (m_Buff.Entity?.GetOptional<PartUnitCombatState>())?.IsInCombat ?? false;

		public override bool IsVisibleForPlayer => m_Buff.Entity?.IsVisibleForPlayer ?? false;

		public InitiativeHolder(Buff buff)
			: base("channeling_" + buff.UniqueId, isInGame: true, buff.Blueprint)
		{
			m_Buff = buff;
		}

		private InitiativeHolder(OwlPackConstructorParameter _)
			: base(_)
		{
		}

		protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
		{
			return null;
		}

		protected override IEntityView CreateViewForData()
		{
			return null;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			EntityFactRef<Buff> obj = m_Buff;
			Hash128 val2 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			InitiativeHolder source = new InitiativeHolder(default(OwlPackConstructorParameter));
			result = Unsafe.As<InitiativeHolder, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<InitiativeHolder>(OwlPackTypeInfo);
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
			formatter.Field(14, "m_Buff", ref m_Buff, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InitiativeHolder>();
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
					m_Buff = formatter.ReadPackable<EntityFactRef<Buff>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[Tooltip("Чаннелинг закончится в конце раунда, НО до юнитов, которые IsLastInFight")]
	public bool ActLastInRound;

	[HideIf("ActLastInRound")]
	public ContextValue SkipEnemyTurnsCount;

	public BpRef<BlueprintAbility> Ability;

	[CanBeNull]
	private UnitAnimationManager UnitAnimationManager => base.Owner.View?.AnimationManager;

	protected override void OnActivate()
	{
		if (base.IsReapplying)
		{
			return;
		}
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (!turnController.TurnBasedModeActive)
		{
			PFLog.EntityFact.Error("Can't start channeling outside of turn based mode");
			base.Buff.MarkExpired();
			return;
		}
		InitiativeHolder initiativeHolder = Entity.Initialize(new InitiativeHolder(base.Buff));
		initiativeHolder.Initiative.Value = GetActionInitiative(out var inNextTurn);
		initiativeHolder.Initiative.Roll = initiativeHolder.Initiative.Value;
		initiativeHolder.Initiative.LastTurn = ((turnController.CurrentUnit?.Initiative.Value < initiativeHolder.Initiative.Value || inNextTurn) ? Game.Instance.Controllers.TurnController.GameRound : 0);
		Game.Instance.State.LoadedAreaState.MainState.AddEntityData(initiativeHolder);
		RequestSavableData<ComponentData>().InitiativeHolder = initiativeHolder;
		if (turnController.CurrentUnit == base.Owner)
		{
			base.Owner.Commands.AddToQueue(new UnitEndTurnParams());
		}
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IChannellingStart>)delegate(IChannellingStart h)
		{
			h.HandleChannelingStart();
		}, isCheckRuntime: true);
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartChanneling>().Set(base.Buff, this);
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			base.Owner.GetOptional<PartChanneling>()?.Clear(base.Buff, this);
			InitiativeHolder entity = RequestSavableData<ComponentData>().InitiativeHolder.Entity;
			if (entity != null)
			{
				Game.Instance.Controllers.EntityDestroyer.Destroy(entity);
			}
		}
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IChannellingSuccessfulRelease>)delegate(IChannellingSuccessfulRelease h)
		{
			h.HandleSuccessfulRelease();
		}, isCheckRuntime: true);
		base.Buff.MarkExpired();
		AbilityData abilityData = new AbilityData(Ability, base.Owner);
		TargetWrapper target = ((abilityData.Blueprint.Range == AbilityRange.Personal) ? ((TargetWrapper)base.Owner) : base.Context.SourceClickedTarget);
		if (abilityData.IsPrecise)
		{
			AbilityData sourceAbility = base.Context.SourceAbility;
			if ((object)sourceAbility != null && sourceAbility.IsPrecise)
			{
				abilityData.PreciseBodyPart = base.Context.SourceAbility?.PreciseBodyPart;
			}
		}
		base.Owner.Commands.AddToQueueFirst(new UnitUseAbilityParams(abilityData, target)
		{
			FreeAction = true,
			IgnoreCooldown = true
		});
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			base.Buff.MarkExpired();
		}
	}

	private float GetActionInitiative(out bool inNextTurn)
	{
		inNextTurn = false;
		if (ActLastInRound)
		{
			return GetLastInitiative();
		}
		TurnController turnController = Game.Instance.Controllers.TurnController;
		MechanicEntity currentUnit = turnController.CurrentUnit;
		List<MechanicEntity> list = turnController.CurrentRoundUnitsOrder.ToTempList();
		List<MechanicEntity> source = turnController.TurnOrder.UnitsOrder.ToTempList();
		IEnumerable<MechanicEntity> @this;
		if (base.Owner != currentUnit)
		{
			IEnumerable<MechanicEntity> enumerable = list;
			@this = enumerable;
		}
		else
		{
			@this = list.Skip(1);
		}
		List<MechanicEntity> list2 = @this.ToTempList().ToTempList();
		int num = list2.Count();
		list2 = list2.Concat(source.TakeWhile((MechanicEntity i) => i != currentUnit)).Concat(currentUnit).ToTempList();
		int num2 = Math.Clamp(SkipEnemyTurnsCount.Calculate(base.Context), 1, 10);
		int num3 = 0;
		MechanicEntity mechanicEntity = null;
		for (int j = 0; j < list2.Count; j++)
		{
			MechanicEntity mechanicEntity2 = list2.ToList()[j];
			if (mechanicEntity2 == base.Owner || num3 >= num2)
			{
				mechanicEntity = mechanicEntity2;
				if (j >= num)
				{
					inNextTurn = true;
				}
				break;
			}
			if (base.Owner.IsEnemy(mechanicEntity2) && (!base.Owner.IsPlayerEnemy || mechanicEntity2.IsPlayerFaction))
			{
				num3++;
			}
		}
		if (mechanicEntity == null)
		{
			mechanicEntity = base.Owner;
			inNextTurn = true;
		}
		return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(mechanicEntity.Initiative.Value) + 1);
	}

	private float GetLastInitiative()
	{
		IEnumerable<MechanicEntity> currentRoundUnitsOrder = Game.Instance.Controllers.TurnController.TurnOrder.CurrentRoundUnitsOrder;
		MechanicEntity mechanicEntity = null;
		MechanicEntity mechanicEntity2 = null;
		foreach (MechanicEntity item in currentRoundUnitsOrder)
		{
			if ((bool)item.Features.IsLastInFight)
			{
				mechanicEntity2 = item;
				break;
			}
			mechanicEntity = item;
		}
		if (mechanicEntity2 != null)
		{
			return mechanicEntity2.Initiative.Value + 0.0001f;
		}
		if (mechanicEntity == null)
		{
			mechanicEntity = base.Owner;
		}
		return mechanicEntity.Initiative.Value - 0.0001f;
	}
}
