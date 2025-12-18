using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractUnitEntity : MechanicEntity<BlueprintUnit>, PartStatsContainer.IOwner, IEntityPartOwner<PartStatsContainer>, IEntityPartOwner, PartUnitCommands.IOwner, IEntityPartOwner<PartUnitCommands>, PartUnitAsks.IOwner, IEntityPartOwner<PartUnitAsks>, PartMovable.IOwner, IEntityPartOwner<PartMovable>, PartUnitViewSettings.IOwner, IEntityPartOwner<PartUnitViewSettings>, PartHealth.IOwner, IEntityPartOwner<PartHealth>, PartLifeState.IOwner, IEntityPartOwner<PartLifeState>, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IHashable, IOwlPackable<AbstractUnitEntity>
{
	public interface IUnitAsleepHandler : ISubscriber<IEntity>, ISubscriber
	{
		void OnIsSleepingChanged(bool sleeping);
	}

	private PartStatsContainer m_Stats;

	private PartUnitCommands m_Commands;

	private PartMovable m_Movable;

	private PartMorale m_Morale;

	private PartLifeState m_LifeState;

	private float m_DesiredOrientation;

	[JsonProperty]
	[OwlPackInclude]
	protected string m_SelectedVoGuid;

	public readonly ObservableCountFlag Passive = new ObservableCountFlag();

	public readonly CountableFlag ControlledByDirector = new CountableFlag();

	public readonly CountableFlag Sleepless = new CountableFlag();

	private EntityRef<UnitGroupView.UnitGroupData> m_GroupRef;

	private bool m_IsSleeping;

	public PartStatsContainer Stats
	{
		get
		{
			if (m_Stats == null)
			{
				m_Stats = GetRequired<PartStatsContainer>();
			}
			return m_Stats;
		}
	}

	public PartUnitCommands Commands
	{
		get
		{
			if (m_Commands == null)
			{
				m_Commands = GetRequired<PartUnitCommands>();
			}
			return m_Commands;
		}
	}

	public PartMovable Movable
	{
		get
		{
			if (m_Movable == null)
			{
				m_Movable = GetRequired<PartMovable>();
			}
			return m_Movable;
		}
	}

	public PartMorale Morale
	{
		get
		{
			if (m_Morale == null)
			{
				m_Morale = GetRequired<PartMorale>();
			}
			return m_Morale;
		}
	}

	public abstract PartUnitAsks Asks { get; }

	public abstract PartUnitViewSettings ViewSettings { get; }

	public abstract PartHealth Health { get; }

	public PartLifeState LifeState
	{
		get
		{
			if (m_LifeState == null)
			{
				m_LifeState = GetRequired<PartLifeState>();
			}
			return m_LifeState;
		}
	}

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 SpawnPosition { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool HoldState { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float DesiredOrientation
	{
		get
		{
			return m_DesiredOrientation;
		}
		set
		{
			if (m_DesiredOrientation != value)
			{
				m_DesiredOrientation = value;
			}
		}
	}

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public StatefulRandom Random { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	public float FlyHeight { get; set; }

	public virtual bool IsExtra => false;

	[CanBeNull]
	public CutsceneControlledUnit CutsceneControlledUnit { get; set; }

	public float AwakeTimer { get; set; } = -1f;


	public int AwakeTimerTicks { get; set; }

	public new AbstractUnitEntityView View => (AbstractUnitEntityView)base.View;

	public override bool SetTransformFromConfigOnLoad => false;

	public override bool SetViewTransform => false;

	public override bool IsAffectedByFogOfWar => false;

	public virtual bool IsDeadAndHasLoot => false;

	public virtual bool LootViewed => false;

	public virtual bool IsPreviewUnit => false;

	public virtual bool AreHandsBusyWithAnimation => false;

	public UnitAnimationManager AnimationManager => View.AnimationManager;

	public override UnitAnimationManager MaybeAnimationManager => ObjectExtensions.Or(View, null)?.AnimationManager;

	public UnitMovementAgentBase MovementAgent => ObjectExtensions.Or(MaybeMovementAgent, null) ?? throw new Exception("MovementAgent is missing");

	public override UnitMovementAgentBase MaybeMovementAgent => ObjectExtensions.Or(View, null)?.MovementAgent;

	public Vector3 OrientationDirection => base.Forward;

	public string CharacterName => this.GetDescriptionOptional()?.Name ?? base.Blueprint.Name;

	public Transform ViewTransform => View.ViewTransform;

	public Gender Gender => this.GetDescriptionOptional()?.Gender ?? base.Blueprint.Gender;

	public int HitPointsLeft => this.GetHealthOptional()?.HitPointsLeft ?? 1;

	public bool IsSleepingWithouTimers => m_IsSleeping;

	public bool IsSleeping
	{
		get
		{
			if (m_IsSleeping && AwakeTimer < 0f)
			{
				return AwakeTimerTicks <= 0;
			}
			return false;
		}
		set
		{
			if (m_IsSleeping != value)
			{
				m_IsSleeping = value;
				if (!value)
				{
					AwakeTimer = -1f;
				}
				if ((bool)View)
				{
					View.UpdateViewActive();
				}
				NotifyIsSleepingChanged(value);
			}
		}
	}

	public bool FreezeOutsideCamera { get; set; }

	public ShieldType ShieldType { get; set; }

	public SurfaceType SurfaceType => base.Blueprint.VisualSettings.SurfaceType;

	public override bool IsViewActive
	{
		get
		{
			if (!base.IsViewActive)
			{
				return false;
			}
			if (IsSleeping && !LifeState.IsDeathRevealed)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsAwake => Game.Instance.EntityPools.AllAwakeUnits.Contains(this);

	public override BlueprintBodyPart DefaultBodyPart => base.Blueprint.BodyParts.FirstOrDefault((BpRef<BlueprintBodyPart> i) => i.Blueprint.Tags.HasAnyFlag(BodyPartTags.Default));

	public override IEnumerable<BlueprintBodyPart> BodyParts => base.Blueprint.BodyParts.Dereference();

	public bool AllowToBuildPathThroughCustomLinks
	{
		get
		{
			if (!base.IsPlayerFaction)
			{
				BlueprintArmyType army = base.Blueprint.Army;
				if (army == null || !army.IsHumanoid)
				{
					PartDetectiveServoSkull optional = GetOptional<PartDetectiveServoSkull>();
					if (optional != null)
					{
						BaseUnitEntity leader = optional.Leader;
						if (leader != null)
						{
							return leader.AllowToBuildPathThroughCustomLinks;
						}
					}
					return false;
				}
			}
			return true;
		}
	}

	public bool TraverseThroughCustomLinksImmediately => GetOptional<PartDetectiveServoSkull>() != null;

	public string VoGuid
	{
		get
		{
			if (!string.IsNullOrEmpty(m_SelectedVoGuid))
			{
				return m_SelectedVoGuid;
			}
			return base.Blueprint.VoId.Guid;
		}
	}

	public override bool UseArmorOfEquipment => base.Blueprint.UseArmorOfEquipment;

	public override bool IsCompanion => base.Blueprint.IsCompanion;

	[CanBeNull]
	public UnitGroupView.UnitGroupData Group => m_GroupRef;

	public virtual float GetWarhammerMovementApPerCellThreateningArea()
	{
		return base.Blueprint.WarhammerMovementApPerCellThreateningArea;
	}

	protected virtual void OnNodeChanged(GraphNode oldNode)
	{
	}

	private void NotifyIsSleepingChanged(bool value)
	{
		EventBus.RaiseEvent((IEntity)this, (Action<IUnitAsleepHandler>)delegate(IUnitAsleepHandler v)
		{
			v.OnIsSleepingChanged(value);
		}, isCheckRuntime: true);
	}

	public void SelectVoGuid(string guid)
	{
		m_SelectedVoGuid = guid;
	}

	protected AbstractUnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected AbstractUnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected AbstractUnitEntity()
	{
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (ObjectExtensions.Or(View, null)?.MovementAgent != null)
		{
			View.MovementAgent.Stop();
		}
		using (ContextData<SpawnedUnitData>.Request().Setup(this))
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)this, (Action<IUnitSpawnHandler>)delegate(IUnitSpawnHandler h)
			{
				h.HandleUnitSpawned();
			}, isCheckRuntime: true);
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		if (View != null && View.RigidbodyController != null)
		{
			GetOrCreate<PartSavedRagdollState>().SaveRagdollState(View.RigidbodyController);
		}
		if (View != null && View.DismembermentManager != null && View.DismembermentManager.Dismembered)
		{
			GetOrCreate<SavedDismembermentState>().SaveDismembermentState(View.DismembermentManager);
		}
	}

	protected override void OnDestroy()
	{
		EventBus.RaiseEvent((IAbstractUnitEntity)this, (Action<IUnitHandler>)delegate(IUnitHandler h)
		{
			h.HandleUnitDestroyed();
		}, isCheckRuntime: true);
		base.OnDestroy();
	}

	protected override void OnDispose()
	{
		if (Health != null)
		{
			Health.LastHandledDamage = null;
		}
		if (!ContextData<SceneEntitiesState.DisposeInProgress>.Current && base.Blueprint?.AssetGuid == "00ac5fe6a92a434aa89518306180b30e")
		{
			PFLog.History.System.Log("HighFactotum disposed\n" + StackTraceUtility.ExtractStackTrace());
			PFLog.System.ErrorWithReport("HighFactotum disposed. It's OK if it is Rogue Trader's hallucinations, otherwise report to programmers IMMEDIATELY");
		}
		base.OnDispose();
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		uint seed = (((bool)ContextData<UnitHelper.PreviewUnit>.Current || IsPreviewUnit) ? PFStatefulRandom.PreviewUnitRandom.uintValue : PFStatefulRandom.UnitRandom.uintValue);
		if (Random == null)
		{
			StatefulRandom statefulRandom2 = (Random = new StatefulRandom("Unit " + base.UniqueId, seed));
		}
	}

	protected override void OnPositionChanged()
	{
		GraphNode node = base.CurrentNode.node;
		base.OnPositionChanged();
		WakeForPositionUpdate();
		if (node == null || node != base.CurrentNode.node)
		{
			OnNodeChanged(node);
		}
	}

	protected override Vector3 ViewPositionToEntityPosition(Vector3 viewPosition)
	{
		return SizePathfindingHelper.FromViewToMechanicsPosition(this, viewPosition);
	}

	protected override Vector3 EntityPositionToViewPosition(Vector3 entityPosition)
	{
		return SizePathfindingHelper.FromMechanicsToViewPosition(this, entityPosition);
	}

	public void Wake(float time = 0f)
	{
		AwakeTimer = Mathf.Max(time, AwakeTimer);
	}

	public void WakeForPositionUpdate()
	{
		AwakeTimerTicks = Mathf.Max(3, AwakeTimerTicks);
	}

	public void Translocate(Vector3 position, float? orientation)
	{
		base.Position = position;
		if (orientation.HasValue)
		{
			SetOrientation(orientation.Value);
		}
		if (View != null)
		{
			View.ViewTransform.position = position;
			View.ForcePlaceAboveGround();
			if (orientation.HasValue)
			{
				View.ViewTransform.rotation = Quaternion.Euler(0f, orientation.Value, 0f);
			}
		}
	}

	public void SetOrientation(float value)
	{
		m_Orientation = value;
		DesiredOrientation = value;
	}

	public void UpdateSlowRotation(float maxAngle)
	{
		_ = m_Orientation;
		m_Orientation = Mathf.MoveTowardsAngle(m_Orientation, DesiredOrientation, maxAngle);
	}

	public float GetOrientationTo(Vector3 point)
	{
		if (point == base.Position)
		{
			return DesiredOrientation;
		}
		Vector3 forward = point - base.Position;
		forward.y = 0f;
		return Quaternion.LookRotation(forward).eulerAngles.y;
	}

	public void TurnTo(Vector3 point)
	{
		DesiredOrientation = GetOrientationTo(point);
	}

	public void ForceTurnTo(Vector3 point)
	{
		SetOrientation(GetOrientationTo(point));
		if (Game.Instance.IsPaused && View != null)
		{
			View.ViewTransform.rotation = Quaternion.Euler(0f, base.Orientation, 0f);
		}
	}

	public void SetPositionWithoutWaking(Vector3 pos)
	{
		float awakeTimer = AwakeTimer;
		base.Position = pos;
		AwakeTimer = awakeTimer;
	}

	public void SetGroup(EntityRef<UnitGroupView.UnitGroupData> groupRef)
	{
		m_GroupRef = groupRef;
	}

	public abstract void MarkExtra();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Vector3 val2 = SpawnPosition;
		result.Append(ref val2);
		bool val3 = HoldState;
		result.Append(ref val3);
		float val4 = DesiredOrientation;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<StatefulRandom>.GetHash128(Random);
		result.Append(ref val5);
		float val6 = FlyHeight;
		result.Append(ref val6);
		result.Append(m_SelectedVoGuid);
		return result;
	}
}
