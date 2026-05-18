using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Kingmaker.UI.Pointer;

public class CursorController : IFocusHandler, ISubscriber, IAbilityTargetSelectionUIHandler, IInteractionHighlightUIHandler, IPartyCharacterHoverHandler
{
	private MapObjectView m_MapObjectView;

	private AbstractUnitEntity m_BaseUnitEntity;

	private bool m_PortraitHover;

	private BaseUnitEntity m_PortraitHoveredUnit;

	private CompositeDisposable m_Disposable;

	private bool m_Locked;

	private bool m_IsActive = true;

	private string m_UpperTextAP;

	private string m_LowerTextMP;

	[Obsolete("Use instance Position property")]
	public static Vector2 CursorPosition => Game.Instance.CursorController.Position;

	private bool CastMode => SelectedAbility != null;

	private bool IsPreciseAttack => Game.Instance.Controllers.PreciseAttackController?.HasTarget ?? false;

	public AbilityData SelectedAbility { get; private set; }

	public bool OnGui { get; private set; }

	public bool IsCursorActive => m_IsActive;

	public Vector2 Position
	{
		get
		{
			if (!TryGetMouse(out var mouse))
			{
				return Vector2.zero;
			}
			return mouse.position.value;
		}
	}

	public bool CursorHasText
	{
		get
		{
			if (string.IsNullOrEmpty(m_UpperTextAP))
			{
				return !string.IsNullOrEmpty(m_LowerTextMP);
			}
			return true;
		}
	}

	public bool GetMouseButton(MouseButton button)
	{
		if (TryGetMouseButton(out var control, button))
		{
			return control.isPressed;
		}
		return false;
	}

	public bool GetMouseButtonDown(MouseButton button)
	{
		if (TryGetMouseButton(out var control, button))
		{
			return control.wasPressedThisFrame;
		}
		return false;
	}

	public bool GetMouseButtonUp(MouseButton button)
	{
		if (TryGetMouseButton(out var control, button))
		{
			return control.wasReleasedThisFrame;
		}
		return false;
	}

	private bool TryGetMouseButton(out ButtonControl control, MouseButton button)
	{
		control = null;
		if (TryGetMouse(out var mouse))
		{
			control = button switch
			{
				MouseButton.Left => mouse.leftButton, 
				MouseButton.Right => mouse.rightButton, 
				MouseButton.Middle => mouse.middleButton, 
				MouseButton.Forward => mouse.forwardButton, 
				MouseButton.Back => mouse.backButton, 
				_ => null, 
			};
		}
		return control != null;
	}

	private bool TryGetMouse(out Mouse mouse)
	{
		mouse = null;
		if (IsCursorActive && Mouse.current != null && Mouse.current.added && Mouse.current.enabled)
		{
			mouse = Mouse.current;
		}
		return mouse != null;
	}

	public void Activate()
	{
		EventBus.Subscribe(this);
		UpdateCursorMode();
		m_Disposable = new CompositeDisposable();
		m_Disposable.Add(Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(OnUpdate));
		m_Disposable.Add(UIVisibilityState.VisibilityPreset.Subscribe(delegate
		{
			PFLog.UI.Log("Implement visibility change");
		}));
	}

	public void Deactivate()
	{
		EventBus.Unsubscribe(this);
		m_Disposable?.Dispose();
		SelectedAbility = null;
	}

	public void SetActive(bool value)
	{
		if (m_IsActive != value)
		{
			m_IsActive = value;
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleActiveChanged(value);
			});
		}
	}

	public void SetCursor(CursorType type, bool force = false)
	{
		if ((!m_Locked && !OnGui) || force)
		{
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleTypeChanged(type);
			});
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

	public void SetInvalidCursor()
	{
		if (!m_Locked)
		{
			IUnitInteraction unitInteractionFrom = UtilityUnit.GetUnitInteractionFrom(m_BaseUnitEntity);
			if (TurnController.IsInTurnBasedCombat())
			{
				SetCursor((unitInteractionFrom == null) ? CursorType.Restricted : CursorType.Gear);
			}
			else
			{
				SetCursor(CursorType.Restricted);
			}
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
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleTextsChanged(upperText, lowerText);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleTextsChanged(upperText, lowerText);
			});
		}
	}

	public void SetNoMoveIcon(bool noMove, bool force = false)
	{
		if ((m_Locked || OnGui) && !force)
		{
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleNoMoveChanged(value: false);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleNoMoveChanged(noMove);
			});
		}
	}

	private void ClearComponents()
	{
		SetCursor(CursorType.Default, force: true);
		SetTextsInternal(null, null, force: true);
		SetNoMoveIcon(noMove: false, force: true);
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
		if (CastMode)
		{
			return;
		}
		if (!isHighlighted)
		{
			SetCursor(CursorType.Default);
			return;
		}
		if (m_BaseUnitEntity.IsDirectlyControllable)
		{
			SetCursor(CursorType.Default);
			return;
		}
		if (m_BaseUnitEntity.IsDeadAndHasLoot && !TurnController.IsInTurnBasedCombat())
		{
			SetCursor(CursorType.Loot);
			return;
		}
		if (m_BaseUnitEntity.LifeState.IsFinallyDead && Game.Instance.Player.UISettings.ShowInspect)
		{
			SetCursor(CursorType.Info);
			return;
		}
		IUnitInteraction unitInteraction = m_BaseUnitEntity.SelectClickInteraction((BaseUnitEntity)entity);
		if (TurnController.IsInTurnBasedCombat())
		{
			SetCursor((unitInteraction != null) ? CursorType.Gear : CursorType.Default);
		}
		else if (unitInteraction != null)
		{
			SetCursor(CursorType.Dialog);
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
				case UIInteractionType.AreaTransition:
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

	private void SetAbilityCursor()
	{
		if (!HasAbilityTarget(out var target, out var pointerPosition))
		{
			SetAbilityCursor(SelectedAbility, isRestricted: true);
			return;
		}
		if (CanTarget(SelectedAbility, target, pointerPosition))
		{
			SetTexts_APMP(m_UpperTextAP, m_LowerTextMP, force: true);
			SetAbilityCursor(SelectedAbility, isRestricted: false);
			return;
		}
		SetTextsInternal(string.Empty, string.Empty, force: true);
		if (GameUIState.Instance.CurrentFullScreenUIType.Value != 0)
		{
			SetCursor(CursorType.Default, force: true);
		}
		else if (!OnGui)
		{
			SetAbilityCursor(SelectedAbility, isRestricted: true);
		}
	}

	private void SetAbilityCursor(AbilityData selectedAbility, bool isRestricted)
	{
		if (!m_Locked && !(selectedAbility == null))
		{
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleTypeChanged(CursorType.Cast, selectedAbility.Icon);
			});
			CursorType cursorType = (isRestricted ? CursorType.CastRestricted : CursorType.Cast);
			EventBus.RaiseEvent(delegate(ICursorControllerHandler h)
			{
				h.HandleTypeChanged(cursorType, selectedAbility.Icon);
			});
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
			SetInvalidCursor();
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
		bool state = player != null && !player.IsInCombat && IsOnNavMesh(PointerController.WorldPositionForSimulation) && Game.Instance.CurrentGameMode?.Type != GameModeType.Dialog;
		TrySetMoveCursor(state);
		if (SelectedAbility == null)
		{
			return;
		}
		InteractionHighlightController instance = InteractionHighlightController.Instance;
		if (instance == null || !instance.IsGlobalHighlighting)
		{
			if (IsPreciseAttack)
			{
				SetCursor(CursorType.Default);
			}
			else
			{
				SetAbilityCursor();
			}
		}
	}

	private bool HasAbilityTarget(out TargetWrapper target, out Vector3? pointerPosition)
	{
		if (!m_PortraitHover)
		{
			PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
			pointerPosition = clickEventsController.WorldPosition;
			target = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, pointerPosition.Value);
			return target != null;
		}
		IUnitEntityView unitEntityView = m_PortraitHoveredUnit?.View;
		if (unitEntityView == null)
		{
			target = null;
			pointerPosition = null;
			return false;
		}
		target = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(unitEntityView.gameObject, unitEntityView.transform.position);
		pointerPosition = null;
		return target != null;
	}

	private bool CanTarget(AbilityData ability, TargetWrapper target, Vector3? pointerPosition)
	{
		AbilityData.UnavailabilityReasonType? reason;
		bool result = ability.CanCastAbility(target, pointerPosition, out reason);
		if (!result && reason == AbilityData.UnavailabilityReasonType.TargetTooFar && Game.Instance.Controllers.TurnController.TurnBasedModeActive && AbilityApproachHelper.IsApproachCandidate(ability, target) && AbilityApproachHelper.TryFindApproachNode(ability, target, out var _))
		{
			result = true;
		}
		EventBus.RaiseEvent(delegate(IAbilityTargetPossibilityCheck h)
		{
			h.HandleAbilityTargetPossibilityCheck(ability, target, pointerPosition, result);
		});
		return result;
	}

	private void OnGuiChanged(bool newValue)
	{
		if (newValue)
		{
			SetCursor(CursorType.Default);
			ClearComponents();
			OnGui = true;
		}
		else
		{
			OnGui = false;
			ClearCursor();
		}
	}

	public void HandlePartyCharacterHover(BaseUnitEntity unit, bool hover)
	{
		m_PortraitHover = hover;
		m_PortraitHoveredUnit = unit;
	}
}
