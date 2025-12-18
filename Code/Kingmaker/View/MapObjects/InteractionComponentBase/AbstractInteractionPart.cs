using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Interaction;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractInteractionPart : ViewBasedPart, IDestructionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IPartyCombatHandler, IHashable, IOwlPackable<AbstractInteractionPart>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected int m_LastCombatRoundInteractionAttempt = -1;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool AlreadyUnlocked { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public virtual bool AlreadyVisited { get; set; }

	public virtual bool CanBeForceShown => true;

	public new MapObjectView View => (MapObjectView)base.View;

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	public bool AlreadyInteractedInThisCombatRound
	{
		get
		{
			TurnController turnController = Game.Instance.Controllers.TurnController;
			if (turnController != null && turnController.TurnBasedModeActive)
			{
				return turnController.CombatRound == m_LastCombatRoundInteractionAttempt;
			}
			return false;
		}
	}

	public abstract InteractionType Type { get; }

	public abstract UIInteractionType UIInteractionType { get; }

	public abstract int ApproachRadius { get; }

	public abstract float OvertipRevealDistance { get; }

	[CanBeNull]
	public abstract List<BaseUnitEntity> UnitsCanInteract { get; }

	public abstract bool Enabled { get; set; }

	public abstract int ActionPointsCost { get; }

	public abstract UnitAnimationInteractionType UseAnimationState { get; }

	public abstract bool ShowOvertip { get; }

	public abstract float OvertipVerticalCorrection { get; }

	public abstract bool ShowHighlight { get; }

	public abstract bool NotInCombat { get; }

	[CanBeNull]
	public abstract string InteractionStopSound { get; }

	[CanBeNull]
	public abstract InteractionSettings.InteractWithToolFXData InteractWithMeltaChargeFXData { get; }

	[CanBeNull]
	public abstract TrapObjectData Trap { get; }

	protected abstract bool UnlimitedInteractionsPerRound { get; }

	public InteractionProcess Interact(BaseUnitEntity user)
	{
		if (!CanInteract())
		{
			PFLog.Default.Error("{0} can't interact with {1}", user, this);
			return InteractionProcess.Finished;
		}
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController.TurnBasedModeActive)
		{
			m_LastCombatRoundInteractionAttempt = turnController.CombatRound;
		}
		return InteractInternal(user);
	}

	protected abstract InteractionProcess InteractInternal(BaseUnitEntity user);

	protected List<InteractionRestrictionPart> GetRestrictions()
	{
		if (!(this is IHasInteractionVariantActors) || !((IHasInteractionVariantActors)this).InteractThroughVariants || ContextData<InteractionVariantData>.Current?.VariantActor == null)
		{
			return Owner.Parts.GetAll<InteractionRestrictionPart>().ToList();
		}
		if (ContextData<InteractionVariantData>.Current?.VariantActor is InteractionRestrictionPart item)
		{
			return new List<InteractionRestrictionPart> { item };
		}
		return new List<InteractionRestrictionPart>();
	}

	public virtual bool CanInteract()
	{
		if (Enabled && (!NotInCombat || !Game.Instance.Controllers.TurnController.TurnBasedModeActive))
		{
			if (!UnlimitedInteractionsPerRound)
			{
				return !AlreadyInteractedInThisCombatRound;
			}
			return true;
		}
		return false;
	}

	public virtual void OnUnitLeftProximity(BaseUnitEntity unit)
	{
	}

	public abstract bool HasVisibleTrap();

	[CanBeNull]
	public virtual BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		return SelectUnitInternal(units.Where(CanBeSelected).ToTempList(), muteEvents, variantActor);
	}

	public List<BaseUnitEntity> SelectAllUnits(ReadonlyList<BaseUnitEntity> units)
	{
		return units.Where(CanBeSelected).ToTempList();
	}

	protected virtual bool CanBeSelected(BaseUnitEntity unit)
	{
		if (unit.CanAct)
		{
			if (!unit.CanMove)
			{
				return IsEnoughCloseForInteraction(unit);
			}
			return true;
		}
		return false;
	}

	private BaseUnitEntity SelectUnitInternal(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		if (units.Count <= 0)
		{
			return null;
		}
		if (Trap != null && HasVisibleTrap())
		{
			BaseUnitEntity baseUnitEntity = Trap.SelectUnit(units, muteEvents);
			if (baseUnitEntity != null)
			{
				return baseUnitEntity;
			}
		}
		List<InteractionRestrictionPart> list = GetRestrictions();
		if (variantActor != null)
		{
			list = list.Where((InteractionRestrictionPart r) => r == variantActor).ToList();
		}
		foreach (InteractionRestrictionPart item in list)
		{
			BaseUnitEntity result = null;
			int num = -1;
			foreach (BaseUnitEntity item2 in units)
			{
				if (units.Count <= 1 || !item2.IsPet)
				{
					int userPriority = item.GetUserPriority(item2);
					if (userPriority > num)
					{
						num = userPriority;
						result = item2;
					}
				}
			}
			if (num >= 0)
			{
				if (num == 0)
				{
					break;
				}
				return result;
			}
		}
		Vector3 p = View.Or(null)?.ViewTransform.position ?? Vector3.zero;
		if (units.Count <= 1)
		{
			return units.FirstOrDefault();
		}
		return units.Where((BaseUnitEntity u) => !u.IsPet).Aggregate((BaseUnitEntity u1, BaseUnitEntity u2) => (!(u1.SqrDistanceTo(p) <= u2.SqrDistanceTo(p))) ? u2 : u1);
	}

	public abstract void PlayStartSound(BaseUnitEntity user);

	public static bool IsEnoughCloseForInteraction(Vector3 unitPosition, Vector3 eyePosition, Vector3 interactionPosition, int approachRadius)
	{
		bool num = Mathf.Min(Math.Abs(eyePosition.y - interactionPosition.y), Math.Abs(unitPosition.y - interactionPosition.y)) < GraphParamsMechanicsCache.GridCellSize * 2f;
		unitPosition = (num ? unitPosition.ToXZ() : unitPosition);
		eyePosition = (num ? eyePosition.ToXZ() : eyePosition);
		if (num)
		{
			interactionPosition = interactionPosition.ToXZ();
		}
		return Mathf.RoundToInt(UnitMovementAgentBase.GetDistanceToSegment(unitPosition, eyePosition, interactionPosition) / GraphParamsMechanicsCache.GridCellSize) <= approachRadius;
	}

	public virtual bool IsEnoughCloseForInteraction(BaseUnitEntity unit, Vector3? position = null)
	{
		Vector3 vector = position ?? unit.Position;
		Vector3 eyePosition = (position.HasValue ? (position.Value + LosCalculations.EyeShift) : unit.EyePosition);
		if (unit.IsInCombat)
		{
			vector = (Vector3)unit.GetInnerNodeNearestToTarget(vector.GetNearestNodeXZUnwalkable(), Owner.Position).position;
		}
		Vector3 position2 = Owner.Position;
		return IsEnoughCloseForInteraction(vector, eyePosition, position2, ApproachRadius);
	}

	public bool IsEnoughCloseForInteractionFromDesiredPosition(BaseUnitEntity unit)
	{
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(unit);
		return IsEnoughCloseForInteraction(unit, desiredPosition);
	}

	public bool HasEnoughActionPoints(BaseUnitEntity unit)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			return unit.CombatState.ActionPoints >= ActionPointsCost;
		}
		return true;
	}

	public virtual void HandleDestructionSuccess(MapObjectView mapObjectView)
	{
		if (mapObjectView == View)
		{
			SetUnlocked();
		}
	}

	public virtual void HandleDestructionFail(MapObjectView mapObjectView)
	{
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		m_LastCombatRoundInteractionAttempt = -1;
	}

	public void SetUnlocked()
	{
		AlreadyUnlocked = true;
	}

	public virtual void SetVisited()
	{
		AlreadyVisited = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = AlreadyUnlocked;
		result.Append(ref val2);
		bool val3 = AlreadyVisited;
		result.Append(ref val3);
		result.Append(ref m_LastCombatRoundInteractionAttempt);
		return result;
	}

	public abstract override void Serialize<TFormatter>(TFormatter formatter, SerializerState state);

	public abstract override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state);
}
