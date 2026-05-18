using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Animation;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntity : Entity, IEntityPartsManagerDelegate, IInitiativeHolder, IMechanicEntity, IEntity, IDisposable, IHashable, IOwlPackable<MechanicEntity>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected Initiative m_Initiative;

	[JsonProperty]
	[OwlPackInclude]
	protected BlueprintMechanicEntityFact m_OriginalBlueprint;

	[JsonProperty]
	[OwlPackInclude]
	protected BlueprintMechanicEntityFact m_Blueprint;

	private MechanicActor m_Actor;

	private PartMechanicFeatures m_Features;

	private int m_GraphVersionIndex;

	private NNInfo m_CurrentNode;

	private GridNode m_CurrentUnwalkableNode;

	[JsonProperty]
	[OwlPackInclude]
	public MechanicEntityFact MainFact { get; protected set; }

	public BlueprintMechanicEntityFact Blueprint => m_Blueprint;

	public BlueprintMechanicEntityFact OriginalBlueprint => m_OriginalBlueprint;

	public new EntityRef<MechanicEntity> Ref => this;

	public BuffCollection Buffs { get; private set; }

	public Initiative Initiative => m_Initiative;

	public new IMechanicEntityView View => (IMechanicEntityView)base.View;

	public virtual float Corpulence => 0.3f;

	public virtual IntRect SizeRect => this.GetSizeRect();

	public Vector3 EyePosition => base.Position + LosCalculations.EyeShift;

	[CanBeNull]
	public virtual UnitMovementAgent MaybeMovementAgent => null;

	[CanBeNull]
	public virtual AnimationManager MaybeAnimationManager => null;

	public virtual bool IsInLockControlCutscene => false;

	public MechanicActor Actor => m_Actor;

	public PartMechanicFeatures Features => m_Features ?? (m_Features = GetRequired<PartMechanicFeatures>());

	public bool IsInPlayerParty => this.GetCombatGroupOptional()?.IsPlayerParty ?? false;

	public bool IsPlayerFaction => this.GetFactionOptional()?.IsPlayer ?? false;

	public bool IsPlayerEnemy => this.GetFactionOptional()?.IsPlayerEnemy ?? false;

	public bool IsNeutral => this.GetFactionOptional()?.Neutral ?? true;

	public virtual bool IsInCombat => this.GetCombatStateOptional()?.IsInCombat ?? false;

	public virtual bool IsDirectlyControllable => this.GetFactionOptional()?.IsDirectlyControllable ?? false;

	public bool IsConscious => this.GetLifeStateOptional()?.IsConscious ?? false;

	public bool IsDead
	{
		get
		{
			PartLifeState lifeStateOptional = this.GetLifeStateOptional();
			if (lifeStateOptional == null)
			{
				return false;
			}
			return lifeStateOptional.State == UnitLifeState.Dead;
		}
	}

	public bool IsDeadOrUnconscious
	{
		get
		{
			UnitLifeState? unitLifeState = this.GetLifeStateOptional()?.State;
			if (unitLifeState.HasValue)
			{
				UnitLifeState valueOrDefault = unitLifeState.GetValueOrDefault();
				if ((uint)(valueOrDefault - 1) <= 1u)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsHelpless
	{
		get
		{
			PartLifeState lifeStateOptional = this.GetLifeStateOptional();
			if (lifeStateOptional == null || lifeStateOptional.IsConscious)
			{
				return Features.Sleeping;
			}
			return true;
		}
	}

	public bool IsAble => !IsProne;

	public bool IsProne
	{
		get
		{
			AbstractUnitCommand abstractUnitCommand = GetOptional<PartUnitCommands>()?.Current;
			if (abstractUnitCommand == null || !abstractUnitCommand.FromCutscene)
			{
				PartLifeState lifeStateOptional = this.GetLifeStateOptional();
				if ((lifeStateOptional == null || lifeStateOptional.IsConscious) && !Features.Prone)
				{
					return Features.Sleeping;
				}
				return true;
			}
			return false;
		}
	}

	public virtual bool CanRotate
	{
		get
		{
			if (!IsProne && !IsHelpless)
			{
				return !Features.RotationForbidden;
			}
			return false;
		}
	}

	public bool CanMove
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene))
			{
				if (!IsHelpless && IsAble && !Features.CantMove && (!Features.CantMoveInCombat || !IsInCombat) && !GetOptional<UnitPartForceMove>())
				{
					UnitPartEncumbrance optional = GetOptional<UnitPartEncumbrance>();
					if (optional == null)
					{
						return true;
					}
					return optional.Value != Encumbrance.Overload;
				}
				return false;
			}
			return true;
		}
	}

	public virtual bool CanAct
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene))
			{
				if (!IsHelpless && IsAble)
				{
					return !Features.CantAct;
				}
				return false;
			}
			return true;
		}
	}

	public virtual bool CanActInTurnBased => CanAct;

	public virtual bool CanDodge => false;

	public virtual bool CanDodgeWithMove => false;

	public Size Size => OriginalSize;

	public virtual Size OriginalSize => Size.Medium;

	[CanBeNull]
	public virtual string Name
	{
		get
		{
			object obj = this.GetDescriptionOptional()?.Name;
			if (obj == null)
			{
				if (Blueprint.Name.IsNullOrEmpty())
				{
					return null;
				}
				obj = Blueprint.Name;
			}
			return (string)obj;
		}
	}

	public virtual bool IsCheater => false;

	public virtual Type RequiredBlueprintType => typeof(BlueprintMechanicEntityFact);

	public virtual bool BlockOccupiedNodes => true;

	[Obsolete("Use PartHealth.IsForbidDirectHpDamage / IsCountHpAsArmor / IsCountAsDestructible instead.")]
	public virtual bool IsMechanism => GetOptional<PartMechanism>() != null;

	public NNInfo CurrentNode
	{
		get
		{
			if (m_CurrentNode.node == null || m_GraphVersionIndex != GraphParamsMechanicsCache.GraphVersionIndex)
			{
				m_CurrentNode = ((AstarPath.active != null) ? ObstacleAnalyzer.GetNearestNode(base.Position) : default(NNInfo));
				m_GraphVersionIndex = GraphParamsMechanicsCache.GraphVersionIndex;
			}
			return m_CurrentNode;
		}
		protected set
		{
			m_CurrentNode = value;
		}
	}

	public GridNode CurrentUnwalkableNode
	{
		get
		{
			if (m_CurrentUnwalkableNode == null || m_GraphVersionIndex != GraphParamsMechanicsCache.GraphVersionIndex)
			{
				m_CurrentUnwalkableNode = ((AstarPath.active != null) ? ObstacleAnalyzer.GetNearestNodeXZUnwalkable(base.Position) : null);
				m_GraphVersionIndex = GraphParamsMechanicsCache.GraphVersionIndex;
			}
			return m_CurrentUnwalkableNode;
		}
		protected set
		{
			m_CurrentUnwalkableNode = value;
		}
	}

	public Vector3 Center => SizePathfindingHelper.FromMechanicsToViewPosition(this, base.Position);

	public virtual bool CanBeAttackedDirectly => false;

	public virtual bool IsInSquad => this.GetSquadOptional()?.IsInSquad ?? false;

	public virtual bool IsSquadLeader => this.GetSquadOptional()?.IsLeader ?? false;

	public virtual BlueprintBodyPart DefaultBodyPart => ConfigRoot.Instance.SystemMechanics.FallbackBodyPart;

	public virtual IEnumerable<BlueprintBodyPart> BodyParts => Enumerable.Repeat(DefaultBodyPart, 1);

	public virtual bool UseArmorOfEquipment => false;

	public virtual bool IsCompanion => false;

	protected MechanicEntity(IMechanicEntityConfig config)
		: this(config.EntityId, config.IsInGameBySettings, config.MechanicFactBlueprint)
	{
	}

	protected MechanicEntity(string uniqueId, bool isInGame, [NotNull] BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame)
	{
		if (blueprint == null)
		{
			throw new Exception("MechanicEntity: blueprint is missing");
		}
		CheckBlueprintType(blueprint);
		m_Blueprint = (m_OriginalBlueprint = blueprint);
	}

	protected MechanicEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartMechanicFeatures>();
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Buffs = Facts.EnsureFactProcessor<BuffCollection>();
		Buffs.SetSubscribedOnEventBus(base.IsInGame);
		Buffs.SetActiveForce(active: true);
		if (m_Initiative == null)
		{
			m_Initiative = new Initiative();
		}
		m_Actor = new MechanicActor(this);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		MainFact = Facts.Add(CreateMainFact(OriginalBlueprint));
	}

	[CanBeNull]
	protected virtual MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MechanicEntityFactBlueprinted(blueprint, null);
	}

	private void CheckBlueprintType([NotNull] BlueprintMechanicEntityFact blueprint)
	{
		Type requiredBlueprintType = RequiredBlueprintType;
		if (blueprint.GetType() != requiredBlueprintType && !blueprint.GetType().IsSubclassOf(requiredBlueprintType))
		{
			throw new Exception(GetType().Name + ": invalid blueprint type " + blueprint.GetType().Name + ", expected type " + requiredBlueprintType.Name);
		}
	}

	void IEntityPartsManagerDelegate.OnPartAppears(EntityPart part)
	{
		OnPartUpdated(part, removed: false);
	}

	void IEntityPartsManagerDelegate.OnPartDisappears(EntityPart part)
	{
		OnPartUpdated(part, removed: true);
	}

	protected virtual void OnPartUpdated(EntityPart part, bool removed)
	{
	}

	public void HandleSpawn()
	{
		if (HoldingState == null)
		{
			PFLog.Default.ErrorWithReport("It is unsafe to spawn entities which not in game state yet");
		}
		try
		{
			OnSpawn();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		Buffs.SetSubscribedOnEventBus(base.IsInGame);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		Buffs.SpawnBuffsFxs();
	}

	protected override void OnPreSave()
	{
	}

	protected override void OnPostLoad()
	{
	}

	protected virtual void OnSpawn()
	{
		UpdateFogOfWarState();
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnDispose()
	{
	}

	protected override void OnPositionChanged()
	{
		base.OnPositionChanged();
		CurrentNode = default(NNInfo);
		CurrentUnwalkableNode = null;
	}

	protected override Vector3 ViewPositionToEntityPosition(Vector3 viewPosition)
	{
		return viewPosition + GetViewToEntityPositionOffset(SizeRect);
	}

	protected override Vector3 EntityPositionToViewPosition(Vector3 entityPosition)
	{
		return entityPosition - GetViewToEntityPositionOffset(SizeRect);
	}

	public static Vector3 GetViewToEntityPositionOffset(IntRect sizeRect)
	{
		float x = ((sizeRect.Width % 2 == 1) ? 0f : 0.5f);
		float z = ((sizeRect.Height % 2 == 1) ? 0f : 0.5f);
		return -new Vector3(x, 0f, z) * 1.Cells().Meters;
	}

	public IUnitInteraction SelectClickInteraction(BaseUnitEntity initiator)
	{
		return GetOptional<PartUnitInteractions>()?.SelectClickInteraction(initiator);
	}

	public void SetFakeBlueprint([CanBeNull] BlueprintMechanicEntityFact fake)
	{
		if (fake == null)
		{
			m_Blueprint = OriginalBlueprint;
			return;
		}
		CheckBlueprintType(fake);
		CheckBlueprintType(fake);
		m_Blueprint = fake;
	}

	public void DifficultyChanged()
	{
		OnDifficultyChanged();
	}

	protected virtual void OnDifficultyChanged()
	{
	}

	public virtual StatBaseValue GetStatBaseValue(StatType type)
	{
		return 0;
	}

	public bool IsEnemy(IMechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.IsEnemy((MechanicEntity)entity) ?? false;
	}

	public bool IsAlly(IMechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.IsAlly(entity) ?? false;
	}

	public bool CanAttack(MechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.CanAttack(entity) ?? true;
	}

	[CanBeNull]
	public virtual ItemEntityWeapon GetFirstWeapon()
	{
		return null;
	}

	[CanBeNull]
	public virtual ItemEntityWeapon GetSecondWeapon()
	{
		return null;
	}

	public bool HasLOS(MechanicEntity entity)
	{
		return this.GetVisionOptional()?.HasLOS(entity) ?? false;
	}

	[CanBeNull]
	public EntityFact AddFact([CanBeNull] BlueprintMechanicEntityFact blueprint, IEvalContext parentContext = null, BuffDuration? duration = null)
	{
		if (blueprint == null)
		{
			return null;
		}
		if (!duration.HasValue)
		{
			return Facts.Add(blueprint.CreateFact(parentContext, null));
		}
		return Facts.Add(blueprint.CreateFact(parentContext, duration.Value));
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append(GetType().Name);
		builder.Append("[");
		builder.Append(OriginalBlueprint.name);
		builder.Append(" as ");
		builder.Append(Blueprint.name);
		builder.Append("] ");
		builder.Append('#');
		builder.Append(base.UniqueId);
		return builder.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<Initiative>.GetHash128(m_Initiative);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(m_OriginalBlueprint);
		result.Append(ref val3);
		Hash128 val4 = SimpleBlueprintHasher.GetHash128(m_Blueprint);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<MechanicEntityFact>.GetHash128(MainFact);
		result.Append(ref val5);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntity<TBlueprint> : MechanicEntity, IHashable, IOwlPackable<MechanicEntity<TBlueprint>> where TBlueprint : BlueprintMechanicEntityFact
{
	public new TBlueprint OriginalBlueprint => (TBlueprint)base.OriginalBlueprint;

	public new TBlueprint Blueprint => (TBlueprint)base.Blueprint;

	public override Type RequiredBlueprintType => typeof(TBlueprint);

	protected MechanicEntity(IMechanicEntityConfig config)
		: base(config)
	{
	}

	protected MechanicEntity(string uniqueId, bool isInGame, [NotNull] TBlueprint blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected MechanicEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
