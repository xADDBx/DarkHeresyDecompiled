using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class CursorController : IFocusHandler, ISubscriber, IAbilityTargetSelectionUIHandler, IInteractionHighlightUIHandler, IPartyCharacterHoverHandler, IUnitPathManagerHandler, ISubscriber<IMechanicEntity>
{
	private MapObjectView m_MapObjectView;

	private AbstractUnitEntity m_BaseUnitEntity;

	private bool m_PortraitHover;

	private BaseUnitEntity m_PortraitHoveredUnit;

	private CompositeDisposable m_Disposable;

	private bool m_Locked;

	private string m_UpperTextAP;

	private string m_LowerTextMP;

	private BaseCursor CurrentCursor
	{
		get
		{
			if (!Game.Instance.IsControllerMouse)
			{
				return ConsoleCursor.Instance;
			}
			return PCCursor.Instance;
		}
	}

	private bool CastMode => SelectedAbility != null;

	private bool IsPreciseAttack => Game.Instance.Controllers.PreciseAttackController?.HasTarget ?? false;

	public AbilityData SelectedAbility { get; private set; }

	public Vector2 CursorPosition => CurrentCursor.Or(null)?.Position ?? ((Vector2)Input.mousePosition);

	public bool OnGui { get; private set; }

	public bool IsCursorActive { get; private set; }

	public bool CursorHasText
	{
		get
		{
			if ((bool)CurrentCursor)
			{
				return !CurrentCursor.NoTexts;
			}
			return false;
		}
	}

	public void Activate()
	{
		EventBus.Subscribe(this);
		UpdateCursorMode();
		m_Disposable = new CompositeDisposable();
		m_Disposable.Add(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
		m_Disposable.Add(UIVisibilityState.VisibilityPreset.Subscribe(delegate
		{
			CurrentCursor.Or(null)?.SetActive(CurrentCursor.PrevActiveCursorState);
		}));
	}

	public void Deactivate()
	{
		EventBus.Unsubscribe(this);
		m_Disposable?.Dispose();
		SelectedAbility = null;
	}

	public void SetActive(bool active)
	{
		IsCursorActive = active;
		CurrentCursor.Or(null)?.SetActive(active);
	}

	public void SetCursor(CursorType type, bool force = false)
	{
		if ((!m_Locked && !OnGui) || force)
		{
			CurrentCursor.Or(null)?.SetCursor(type);
		}
	}

	public void SetLock(bool @lock)
	{
		m_Locked = @lock;
		if (m_Locked)
		{
			ClearComponents();
		}
	}

	public void ClearCursor(bool force = false)
	{
		if ((!m_Locked && !OnGui) || force)
		{
			if (CastMode && !IsPreciseAttack)
			{
				SetAbilityCursor();
			}
			else
			{
				SetCursor(CursorType.Default, force);
			}
		}
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		SelectedAbility = ability;
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		SelectedAbility = null;
	}

	public void SetAbilityCursor(Sprite abilityIcon)
	{
		if (!m_Locked)
		{
			CurrentCursor.Or(null)?.SetAbilityCursor(abilityIcon);
		}
	}

	public void SetAbilityCursor()
	{
		if (!m_Locked)
		{
			CurrentCursor.Or(null)?.SetAbilityCursor(SelectedAbility.Icon);
		}
	}

	public void SetTexts_APMP(string upperText, string lowerText, bool force = false)
	{
		m_UpperTextAP = upperText;
		m_LowerTextMP = lowerText;
		SetTextsInternal(m_UpperTextAP, m_LowerTextMP, force);
	}

	private void SetTextsInternal(string upperText, string lowerText, bool force = false)
	{
		if ((m_Locked || OnGui) && !force)
		{
			CurrentCursor.Or(null)?.SetTexts(null, null);
		}
		else
		{
			CurrentCursor.Or(null)?.SetTexts(upperText, lowerText);
		}
	}

	public void SetNoMoveIcon(bool noMove, bool force = false)
	{
		if ((m_Locked || OnGui) && !force)
		{
			CurrentCursor.Or(null)?.SetNoMove(noMove: false);
		}
		else
		{
			CurrentCursor.Or(null)?.SetNoMove(noMove);
		}
	}

	private void ClearComponents()
	{
		CurrentCursor.Or(null)?.ClearComponents();
	}

	private void UpdateCursorMode(bool forceFocused = false)
	{
		Cursor.lockState = CursorLockMode.None;
		if ((Application.isFocused || forceFocused) && (SettingsRoot.Graphics.FullScreenMode.GetValue() == FullScreenMode.ExclusiveFullScreen || (bool)SettingsRoot.Graphics.WindowedCursorLock))
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
	}

	void IFocusHandler.OnApplicationFocusChanged(bool isFocused)
	{
		if (!ApplicationFocusEvents.CursorDisabled)
		{
			if (isFocused)
			{
				UpdateCursorMode(forceFocused: true);
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			SetLock(isOn);
			if (isOn)
			{
				SetCursor(CursorType.Info, force: true);
			}
			else
			{
				ClearCursor(force: true);
			}
		}
	}

	public void SetUnitCursor(AbstractUnitEntity unit, bool isHighlighted)
	{
		if (unit == null)
		{
			return;
		}
		m_BaseUnitEntity = (isHighlighted ? unit : null);
		IAbstractUnitEntity entity = Game.Instance.Player.MainCharacter.Entity;
		if (!CastMode)
		{
			if (!isHighlighted)
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.IsDirectlyControllable)
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.IsDeadAndHasLoot && !TurnController.IsInTurnBasedCombat())
			{
				SetCursor(CursorType.Loot);
			}
			else if (m_BaseUnitEntity.LifeState.IsFinallyDead && Game.Instance.Player.UISettings.ShowInspect)
			{
				SetCursor(CursorType.Info);
			}
			else if (TurnController.IsInTurnBasedCombat())
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.SelectClickInteraction((BaseUnitEntity)entity) != null)
			{
				SetCursor(CursorType.Dialog);
			}
		}
	}

	public void SetMapObjectCursor(MapObjectView mapObjectView, bool isHighlighted)
	{
		m_MapObjectView = (isHighlighted ? mapObjectView : null);
		if (CastMode)
		{
			return;
		}
		if (!isHighlighted)
		{
			SetCursor(CursorType.Default);
			return;
		}
		if (mapObjectView is TrapObjectView)
		{
			SetCursor(CursorType.Trap);
			return;
		}
		InteractionLootPart optional = mapObjectView.Data.GetOptional<InteractionLootPart>();
		if (optional != null && optional.HasVisibleTrap())
		{
			SetCursor(CursorType.Trap);
			return;
		}
		AbstractInteractionPart abstractInteractionPart = mapObjectView.Data.Parts.GetAll<AbstractInteractionPart>().FirstOrDefault((AbstractInteractionPart i) => i.Enabled);
		AbstractInteractionPart abstractInteractionPart2 = abstractInteractionPart;
		if (!(abstractInteractionPart2 is InteractionLootPart) && !(abstractInteractionPart2 is InteractionDoorPart))
		{
			if (abstractInteractionPart2 is InteractionSkillCheckPart interactionSkillCheckPart)
			{
				if (interactionSkillCheckPart.InteractThroughVariants)
				{
					goto IL_00c4;
				}
			}
			else if (abstractInteractionPart2 is InteractionDevicePart && !abstractInteractionPart.ShowOvertip)
			{
				SetCursor(CursorType.Gear);
				return;
			}
			if (abstractInteractionPart != null)
			{
				switch (abstractInteractionPart.UIInteractionType)
				{
				case UIInteractionType.None:
					SetCursor(CursorType.Gear);
					break;
				case UIInteractionType.Action:
				case UIInteractionType.Credits:
				case UIInteractionType.Projector:
					SetCursor(CursorType.Gear);
					break;
				case UIInteractionType.Move:
					SetCursor(CursorType.Interact);
					break;
				case UIInteractionType.Info:
				case UIInteractionType.DetectiveTrace:
				case UIInteractionType.DetectiveClue:
					SetCursor(CursorType.Detective);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			return;
		}
		goto IL_00c4;
		IL_00c4:
		if ((bool)mapObjectView.Data.GetOptional<LoreXenosRestrictionPart>() || (bool)mapObjectView.Data.GetOptional<SleightOfHandRestrictionPart>())
		{
			if (abstractInteractionPart.AlreadyUnlocked || abstractInteractionPart is InteractionDoorPart { IsOpen: not false })
			{
				SetCursor(CursorType.Unlock);
			}
			else
			{
				SetCursor(CursorType.Lock);
			}
		}
		else if (abstractInteractionPart is InteractionDoorPart)
		{
			SetCursor(CursorType.Interact);
		}
		else
		{
			SetLootCursor();
		}
	}

	private void SetLootCursor()
	{
		SetCursor((!TurnController.IsInTurnBasedCombat()) ? CursorType.Loot : CursorType.Default);
	}

	public void TrySetMoveCursor(bool state, bool forbidden = false)
	{
		if (CastMode || IsPreciseAttack)
		{
			return;
		}
		if (forbidden)
		{
			SetCursor(CursorType.Restricted);
		}
		else if (!m_MapObjectView && m_BaseUnitEntity == null)
		{
			if (state)
			{
				SetCursor(Game.Instance.IsControllerMouse ? CursorType.Move : CursorType.ConsoleMove);
			}
			else
			{
				ClearCursor();
			}
		}
	}

	public void SetRotateCameraCursor(bool state)
	{
		if (!CastMode)
		{
			if (state)
			{
				SetCursor(CursorType.RotateCamera, force: true);
			}
			else
			{
				ClearCursor(force: true);
			}
		}
	}

	private static bool IsOnNavMesh(Vector3 worldPosition)
	{
		return AstarPath.active?.IsPointOnNavmesh(worldPosition) ?? false;
	}

	private void OnUpdate()
	{
		if (OnGui != PointerController.InGui)
		{
			OnGuiChanged(PointerController.InGui);
		}
		Player player = Game.Instance.Player;
		TrySetMoveCursor(player != null && !player.IsInCombat && IsOnNavMesh(PointerController.WorldPositionForSimulation));
		if (SelectedAbility == null)
		{
			return;
		}
		InteractionHighlightController instance = InteractionHighlightController.Instance;
		if ((instance != null && instance.IsGlobalHighlighting) || IsPreciseAttack)
		{
			return;
		}
		Vector3? pointerPosition = null;
		TargetWrapper targetForDesiredPosition;
		if (!m_PortraitHover)
		{
			PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
			pointerPosition = clickEventsController.WorldPosition;
			targetForDesiredPosition = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, pointerPosition.Value);
		}
		else
		{
			targetForDesiredPosition = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(m_PortraitHoveredUnit.View.gameObject, m_PortraitHoveredUnit.View.transform.position);
		}
		if (CanTarget(SelectedAbility, targetForDesiredPosition, pointerPosition))
		{
			SetTexts_APMP(m_UpperTextAP, m_LowerTextMP, force: true);
			SetAbilityCursor();
			return;
		}
		SetTextsInternal(string.Empty, string.Empty, force: true);
		if (GameUIState.Instance.CurrentFullScreenUIType.Value != 0)
		{
			SetCursor(CursorType.Default, force: true);
			return;
		}
		if (OnGui)
		{
			CursorType? cursorType = CurrentCursor.Or(null)?.CurrentType;
			if (cursorType.HasValue && cursorType.GetValueOrDefault() == CursorType.Default)
			{
				return;
			}
		}
		SetCursor(CursorType.CastRestricted, force: true);
	}

	private bool CanTarget(AbilityData ability, TargetWrapper target, Vector3? pointerPosition)
	{
		if (ability == null || target == null)
		{
			return false;
		}
		AbilityData.UnavailabilityReasonType? unavailabilityReason;
		bool flag = SelectedAbility.CanTargetFromDesiredPosition(target, out unavailabilityReason);
		if (flag && pointerPosition.HasValue && SelectedAbility.IsAoe)
		{
			if (!SelectedAbility.CanCastAoeAtPointerPositionFromDesiredPosition(target, pointerPosition.Value))
			{
				unavailabilityReason = AbilityData.UnavailabilityReasonType.TargetTooFar;
				flag = false;
			}
			else if (SelectedAbility.IsResultPatternEmpty(target))
			{
				unavailabilityReason = AbilityData.UnavailabilityReasonType.Unknown;
				flag = false;
			}
		}
		bool flag2 = !Game.Instance.Player.IsInCombat && unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar;
		bool result = flag || flag2;
		EventBus.RaiseEvent(delegate(IAbilityTargetPossibilityCheck h)
		{
			h.HandleAbilityTargetPossibilityCheck(ability, target, pointerPosition, result);
		});
		return result;
	}

	private void OnGuiChanged(bool newValue)
	{
		bool flag;
		if (newValue)
		{
			CursorType? cursorType = CurrentCursor.Or(null)?.CurrentType;
			if (cursorType.HasValue)
			{
				CursorType valueOrDefault = cursorType.GetValueOrDefault();
				if ((uint)(valueOrDefault - 14) <= 1u)
				{
					flag = true;
					goto IL_0047;
				}
			}
			flag = false;
			goto IL_0047;
		}
		OnGui = false;
		ClearCursor();
		goto IL_006f;
		IL_0047:
		if (flag)
		{
			SetCursor(CursorType.Default);
			ClearComponents();
		}
		OnGui = true;
		goto IL_006f;
		IL_006f:
		CurrentCursor.Or(null)?.OnGuiChanged(OnGui);
	}

	public void HandlePartyCharacterHover(BaseUnitEntity unit, bool hover)
	{
		m_PortraitHover = hover;
		m_PortraitHoveredUnit = unit;
	}

	public void HandlePathAdded(Path path, float cost, List<BaseUnitEntity> enemiesAoO)
	{
		SetNotificationInternal((enemiesAoO.Count > 0) ? UINotificationTexts.Instance.AttackOfOpportunityTrigger.Text : null);
	}

	public void HandlePathRemoved()
	{
		SetNotificationInternal(null);
	}

	public void HandleCurrentNodeChanged(float cost)
	{
	}

	private void SetNotificationInternal(string notificationText, bool force = false)
	{
		if ((m_Locked || OnGui) && !force)
		{
			CurrentCursor.Or(null)?.SetNotification(null);
		}
		else
		{
			CurrentCursor.Or(null)?.SetNotification(notificationText);
		}
	}
}
