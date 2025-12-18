using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Inspect;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Mechanics.Entities;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using R3;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickWithSelectedAbilityHandler : IClickEventHandler
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ReturnToOriginPart : BaseUnitPart, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable, IOwlPackable<ReturnToOriginPart>
	{
		[JsonProperty(IsReference = false)]
		[OwlPackInclude]
		public Vector3 Origin;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ReturnToOriginPart",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Origin", typeof(Vector3))
			}
		};

		public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
		{
			if (base.Owner == command.Executor && command.Executor.Commands.Queue.Empty())
			{
				base.Owner.Position = Origin;
				base.Owner.Remove<ReturnToOriginPart>();
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Origin);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ReturnToOriginPart source = new ReturnToOriginPart();
			result = Unsafe.As<ReturnToOriginPart, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ReturnToOriginPart>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Origin", ref Origin, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ReturnToOriginPart>();
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
					Origin = formatter.ReadPackable<Vector3>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public AbilityData Ability { get; private set; }

	public PointerMode GetMode()
	{
		return PointerMode.Ability;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (Ability == null)
		{
			return 0f;
		}
		if (Input.GetMouseButtonUp(1))
		{
			return 1f;
		}
		bool flag = gameObject.IsLayerMask(Layers.WalkableMask);
		if (Ability.TargetAnchor == AbilityTargetAnchor.Point)
		{
			bool flag2 = KeyboardAccess.IsCtrlHold();
			if (!SettingsRoot.Controls.ConvertSnapLogic)
			{
				flag2 = !flag2;
			}
			if (flag2 && !flag)
			{
				return 0f;
			}
		}
		TargetWrapper targetForDesiredPosition = GetTargetForDesiredPosition(gameObject, worldPosition);
		UnitPartPersonalEnemy unitPartPersonalEnemy = targetForDesiredPosition?.Entity?.GetOptional<UnitPartPersonalEnemy>();
		if (unitPartPersonalEnemy != null && !unitPartPersonalEnemy.IsCurrentlyTargetable)
		{
			return 0f;
		}
		if (targetForDesiredPosition != null && Ability.CanTargetFromDesiredPosition(targetForDesiredPosition) && targetForDesiredPosition.Entity != null)
		{
			float result = (Ability.CanTargetPoint ? 1f : 0f);
			if (targetForDesiredPosition.Entity.IsDeadOrUnconscious && !Ability.Blueprint.CanCastToDeadTarget)
			{
				return result;
			}
			if (targetForDesiredPosition.Entity.IsEnemy(Ability.Caster) && !Ability.Blueprint.CanTargetEnemies)
			{
				return result;
			}
			if (!targetForDesiredPosition.Entity.IsEnemy(Ability.Caster) && !Ability.Blueprint.CanTargetFriends)
			{
				return result;
			}
			return 2f;
		}
		return 1f;
	}

	public TargetWrapper GetTargetForDesiredPosition(GameObject gameObject, Vector3 worldPosition)
	{
		return GetTarget(gameObject, worldPosition, Ability, Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Ability.Caster));
	}

	public TargetWrapper GetTarget(GameObject gameObject, Vector3 worldPosition, AbilityData ability, Vector3 casterPosition)
	{
		if (ability == null)
		{
			return null;
		}
		MechanicEntity mechanicEntity = ((gameObject != null) ? (gameObject.GetComponentNonAlloc<MechanicEntityView>() ?? gameObject.GetComponentInParent<MechanicEntityView>()) : null).Or(null)?.Data ?? worldPosition.GetNearestNodeXZUnwalkable()?.GetFirstUnit();
		switch (ability.TargetAnchor)
		{
		case AbilityTargetAnchor.Owner:
			return ability.Caster;
		case AbilityTargetAnchor.Unit:
			if (mechanicEntity == null || !mechanicEntity.CanBeAttackedDirectly)
			{
				return null;
			}
			return new TargetWrapper(mechanicEntity);
		case AbilityTargetAnchor.Point:
		{
			GridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
			Vector3 vector = worldPosition;
			vector = ((ability.GetPatternSettings() == null) ? AoEPatternHelper.GetGridAdjustedPosition(vector) : AoEPatternHelper.GetActualCastPosition(ability.Caster, nearestNodeXZUnwalkable, vector, ability.MinRangeCells, ability.RangeCells));
			Vector3 vector2 = vector - nearestNodeXZUnwalkable.Vector3Position();
			Quaternion quaternion = ((vector2 != Vector3.zero) ? Quaternion.LookRotation(vector2) : Quaternion.identity);
			MechanicEntity entity = ((mechanicEntity != null && mechanicEntity.CanBeAttackedDirectly && (ability.IsCharge || ability.Blueprint.CanTargetEnemies || ability.Blueprint.CanTargetFriends)) ? mechanicEntity : null);
			return new TargetWrapper(vector, quaternion.eulerAngles.y, entity);
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		if (Ability == null)
		{
			return false;
		}
		InteractionHighlightController instance = InteractionHighlightController.Instance;
		if (instance != null && instance.IsGlobalHighlighting)
		{
			AbstractUnitEntityView targetUnit = gameObject.GetComponent<AbstractUnitEntityView>();
			if (button == 1 && targetUnit != null && InspectUnitsHelper.IsInspectAllow(targetUnit.EntityData))
			{
				EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
				{
					h.HandleUnitRightClick(targetUnit.Data);
				});
				return false;
			}
			EventBus.RaiseEvent(delegate(IAbilityRangeManualUIHandler h)
			{
				h.HandleSetRangeToCasterPositionManual(Ability, null);
			});
			return false;
		}
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Ability.Caster);
		TargetWrapper target = GetTarget(gameObject, worldPosition, Ability, desiredPosition);
		if (ShouldHandleAbilityCastFail(target, worldPosition, out var unavailabilityReason))
		{
			if (unavailabilityReason.HasValue)
			{
				string restrictionText = Ability.GetUnavailabilityReasonString(unavailabilityReason.Value, desiredPosition, target);
				EventBus.RaiseEvent(delegate(ICursorNotificationUIHandler h)
				{
					h.HandleNotification(restrictionText, WarningNotificationFormat.Attention);
				});
				EventBus.RaiseEvent(delegate(IClickActionHandler h)
				{
					h.OnAbilityCastRefused(Ability, target, null);
				});
				UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
				return false;
			}
			IAbilityTargetRestriction failedRestriction = null;
			IAbilityTargetRestriction[] targetRestrictions = Ability.Blueprint.TargetRestrictions;
			foreach (IAbilityTargetRestriction abilityTargetRestriction in targetRestrictions)
			{
				if (!abilityTargetRestriction.IsTargetRestrictionPassed(Ability, target, Ability.Caster.Position))
				{
					failedRestriction = abilityTargetRestriction;
					string restrictionText = abilityTargetRestriction.GetAbilityTargetRestrictionUIText(Ability, target, desiredPosition);
					if (restrictionText.IsNullOrEmpty())
					{
						restrictionText = ConfigRoot.Instance.LocalizedTexts.Reasons.UnavailableGeneric;
					}
					EventBus.RaiseEvent(delegate(ICursorNotificationUIHandler h)
					{
						h.HandleNotification(restrictionText, WarningNotificationFormat.Attention);
					});
					break;
				}
			}
			EventBus.RaiseEvent(delegate(IClickActionHandler h)
			{
				h.OnAbilityCastRefused(Ability, target, failedRestriction);
			});
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
			return false;
		}
		AbilityData ability = Ability;
		if ((object)ability != null && ability.IsPrecise)
		{
			(from d in Game.Instance.Controllers.PreciseAttackController.RequestBodyPartOrDefault(ability, target).Take(1)
				where d.BodyPart != null
				select d).Subscribe(delegate(PreciseAttackController.TargetData d)
			{
				ability.PreciseBodyPart = d.BodyPart;
				UseAbility(ability, d.Target, shouldApproach: false);
			});
			return true;
		}
		bool shouldApproach = unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar;
		UseAbility(Ability, target, shouldApproach);
		return true;
	}

	private static void UseAbility(AbilityData ability, TargetWrapper target, bool shouldApproach)
	{
		UnitCommandsRunner.TryUnitUseAbility(ability, target, shouldApproach);
		UISounds.Instance.Sounds.Combat.CombatGridConfirmActionClick.Play();
		Game.Instance.GameCommandQueue.ClearPointerMode();
	}

	private bool ShouldHandleAbilityCastFail(TargetWrapper target, Vector3 clickPosition, out AbilityData.UnavailabilityReasonType? unavailabilityReason)
	{
		unavailabilityReason = null;
		if (target == null)
		{
			if (!Ability.CanTargetPoint)
			{
				unavailabilityReason = AbilityData.UnavailabilityReasonType.NullTarget;
				return true;
			}
			return true;
		}
		if (Ability.IsPrecise && !(target.Entity is BaseUnitEntity))
		{
			unavailabilityReason = AbilityData.UnavailabilityReasonType.TargetCannotBeAttackedByPreciseAttack;
			return true;
		}
		bool flag = Ability.CanTargetFromDesiredPosition(target, out unavailabilityReason);
		if (flag && Ability.IsAoe)
		{
			if (!Ability.CanCastAoeAtPointerPositionFromDesiredPosition(target, clickPosition))
			{
				flag = false;
				unavailabilityReason = AbilityData.UnavailabilityReasonType.TargetTooFar;
			}
			else if (Ability.IsResultPatternEmpty(target))
			{
				flag = false;
				unavailabilityReason = AbilityData.UnavailabilityReasonType.Unknown;
			}
		}
		if (unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar && !Game.Instance.Player.IsInCombat)
		{
			return false;
		}
		return !flag;
	}

	public void SetAbility([NotNull] AbilityData ability)
	{
		if (Ability != null)
		{
			if (ability == Ability)
			{
				Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
				return;
			}
			DropAbility();
		}
		Game.Instance.Controllers.ClickEventsController.SetPointerMode(PointerMode.Ability);
		Ability = ability;
		EventBus.RaiseEvent(delegate(IAbilityTargetSelectionUIHandler h)
		{
			h.HandleAbilityTargetSelectionStart(ability);
		});
	}

	public void DropAbility()
	{
		if (!(Ability == null))
		{
			Ability = null;
			EventBus.RaiseEvent(delegate(IAbilityTargetSelectionUIHandler h)
			{
				h.HandleAbilityTargetSelectionEnd(Ability);
			});
		}
	}
}
