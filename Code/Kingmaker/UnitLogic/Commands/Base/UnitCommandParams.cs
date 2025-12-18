using System;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands.Base;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class UnitCommandParams : IOwlPackable, IOwlPackable<UnitCommandParams>
{
	public enum CommandType
	{
		None,
		Run,
		AddToQueue,
		AddToQueueFirst
	}

	public const int InfiniteRange = 10000;

	[JsonProperty(PropertyName = "ct", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public CommandType Type;

	[JsonProperty(PropertyName = "or", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public EntityRef<BaseUnitEntity> OwnerRef;

	[JsonProperty(PropertyName = "fa", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected bool? m_FreeAction;

	[JsonProperty(PropertyName = "ls", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected bool? m_NeedLoS;

	[JsonProperty(PropertyName = "ar", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected int? m_ApproachRadius;

	[JsonProperty(PropertyName = "fp", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	protected ForcedPath m_ForcedPath;

	[JsonProperty(PropertyName = "mt", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected WalkSpeedType? m_MovementType;

	[JsonProperty(PropertyName = "of", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected bool? m_IsOneFrameCommand;

	[JsonProperty(PropertyName = "sm", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected bool? m_SlowMotionRequired;

	[JsonProperty(PropertyName = "tr", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	public TargetWrapper Target { get; protected set; }

	[JsonProperty(PropertyName = "fc", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool FromCutscene { get; protected set; }

	public bool IsSynchronized { get; set; }

	[JsonProperty(PropertyName = "is", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool InterruptAsSoonAsPossible { get; set; }

	[JsonProperty(PropertyName = "ov", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public float? OverrideSpeed { get; set; }

	[JsonProperty(PropertyName = "ni", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool DoNotInterruptAfterFight { get; set; }

	public bool FreeAction
	{
		get
		{
			return m_FreeAction ?? DefaultFreeAction;
		}
		set
		{
			m_FreeAction = value;
		}
	}

	public int ApproachRadius
	{
		get
		{
			return m_ApproachRadius ?? DefaultApproachRadius;
		}
		set
		{
			m_ApproachRadius = value;
		}
	}

	public bool NeedLoS
	{
		get
		{
			return m_NeedLoS ?? DefaultNeedLoS;
		}
		set
		{
			m_NeedLoS = value;
		}
	}

	[CanBeNull]
	public ForcedPath ForcedPath
	{
		get
		{
			return m_ForcedPath;
		}
		set
		{
			if (value != null && value.error)
			{
				throw new Exception("[ForcedPath] An attempt to set a failed path");
			}
			m_ForcedPath?.Release(this);
			m_ForcedPath = value;
			m_ForcedPath?.Claim(this);
		}
	}

	public WalkSpeedType MovementType
	{
		get
		{
			if (Target?.Entity is AbstractUnitEntity owner && Player.IsForcedToWalk(owner))
			{
				return WalkSpeedType.Walk;
			}
			return m_MovementType ?? DefaultMovementType;
		}
		set
		{
			m_MovementType = value;
		}
	}

	public bool IsOneFrameCommand
	{
		get
		{
			return m_IsOneFrameCommand ?? DefaultIsOneFrameCommand;
		}
		set
		{
			m_IsOneFrameCommand = value;
		}
	}

	public bool SlowMotionRequired
	{
		get
		{
			return m_SlowMotionRequired ?? DefaultSlowMotionRequired;
		}
		set
		{
			m_SlowMotionRequired = value;
		}
	}

	protected virtual bool DefaultFreeAction => false;

	public virtual int DefaultApproachRadius => 1;

	protected virtual bool DefaultNeedLoS => false;

	protected virtual WalkSpeedType DefaultMovementType => WalkSpeedType.Run;

	protected virtual bool DefaultIsOneFrameCommand => false;

	protected virtual bool DefaultSlowMotionRequired => false;

	public virtual bool IsDirectionCorrect => true;

	public void MarkFromCutscene()
	{
		FromCutscene = true;
	}

	protected UnitCommandParams()
	{
	}

	[JsonConstructor]
	protected UnitCommandParams(JsonConstructorMark _)
	{
	}

	protected UnitCommandParams([CanBeNull] TargetWrapper target)
	{
		Target = target;
		FromCutscene = ContextData<NamedParametersContext.ContextData>.Current != null;
	}

	public bool IsOffensiveCommand([NotNull] AbstractUnitEntity executor)
	{
		if (Target?.Entity is BaseUnitEntity baseUnitEntity && this is UnitUseAbilityParams && baseUnitEntity != executor && !baseUnitEntity.LifeState.IsFinallyDead)
		{
			return executor.GetCombatGroupOptional()?.CanAttack(baseUnitEntity) ?? false;
		}
		return false;
	}

	public virtual bool IsUnitEnoughClose([NotNull] AbstractUnitEntity executor)
	{
		Vector3 targetPoint = GetTargetPoint(executor);
		if (!executor.InRangeInCells(targetPoint, ApproachRadius))
		{
			return false;
		}
		if (!NeedLoS)
		{
			return true;
		}
		return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(executor, targetPoint, default(IntRect)) != LosCalculations.CoverType.LosBlocker;
	}

	public virtual Vector3 GetTargetPoint([NotNull] AbstractUnitEntity executor)
	{
		return Target?.Point ?? executor.Position;
	}

	public void AfterDeserialization()
	{
		IsSynchronized = true;
		m_ForcedPath?.Claim(this);
	}

	public virtual bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		return false;
	}

	protected abstract AbstractUnitCommand CreateCommandInternal();

	public AbstractUnitCommand CreateCommand()
	{
		return CreateCommandInternal();
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class UnitCommandParams<T> : UnitCommandParams, IOwlPackable<UnitCommandParams<T>> where T : AbstractUnitCommand
{
	protected UnitCommandParams()
	{
	}

	[JsonConstructor]
	protected UnitCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitCommandParams([CanBeNull] TargetWrapper target)
		: base(target)
	{
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return (AbstractUnitCommand)Activator.CreateInstance(typeof(T), this);
	}

	public new T CreateCommand()
	{
		return (T)CreateCommandInternal();
	}
}
