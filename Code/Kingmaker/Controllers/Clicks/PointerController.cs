using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.View.Mechanics.Entities.Covers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View;
using Kingmaker.Visual;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

namespace Kingmaker.Controllers.Clicks;

public class PointerController : IControllerEnable, IController, IControllerDisable, IControllerTick, IPlayerAbilitiesHandler, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler
{
	private class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit h1, RaycastHit h2)
		{
			return -h1.point.y.CompareTo(h2.point.y);
		}
	}

	public const int LeftButton = 0;

	public const int RightButton = 1;

	private const float VeryQuickDragTime = 0.07f;

	private const float MouseDragThreshold = 4f;

	private static readonly RaycastHitComparer HitComparer = new RaycastHitComparer();

	public bool SkipDeactivation;

	private bool m_MouseDown;

	private MouseButton m_MouseDownButton;

	private IClickEventHandler m_MouseDownHandler;

	private GameObject m_MouseDownOn;

	private Vector2 m_MouseDownCoord;

	private Vector3 m_MouseDownWorldPosition;

	private Vector3 m_MouseDownGroundPosition;

	private float m_MouseButtonTime;

	private bool m_MouseDrag;

	private int m_DragFrames;

	private readonly List<IDetectHover> m_HoverComponents = new List<IDetectHover>();

	private readonly IClickEventHandler[] m_ClickHandlers;

	private bool m_IsTurnBased;

	private RaycastHit[] m_Hits = new RaycastHit[16];

	public static bool DebugThisFrame { get; set; }

	public PointerMode Mode { get; private set; }

	public GameObject PointerOn { get; private set; }

	public Vector3 WorldPosition { get; private set; }

	public Vector3 GroundPosition { get; private set; }

	public bool GamePadConfirm { get; set; }

	public bool GamePadDecline { get; set; }

	public static Vector3 WorldPositionForSimulation { get; private set; }

	private static CursorController Cursor => Game.Instance.CursorController;

	private static MultiplySelection MultiSelection
	{
		get
		{
			if (!UtilityGame.IsGlobalMap())
			{
				return MultiplySelection.Instance;
			}
			return null;
		}
	}

	private Camera MainCamera => CameraStackManager.Instance.ActiveMainCamera;

	[Cheat(Name = "pointer_debug")]
	public static bool PointerDebug { get; set; }

	public string UIObjectsRaycasts { get; private set; }

	public List<string> Hits { get; private set; }

	public static bool InGui
	{
		get
		{
			bool flag = (bool)EventSystem.current && EventSystem.current.IsPointerOverGameObject();
			if (Game.Instance.IsControllerGamepad)
			{
				flag = flag || !Game.Instance.CursorController.IsCursorActive;
			}
			return flag;
		}
	}

	public PointerController(params IClickEventHandler[] clickHandlers)
	{
		string[] array = (from pair in (from c in clickHandlers
				group c by c.GetType()).ToDictionary((IGrouping<Type, IClickEventHandler> g) => g.Key, (IGrouping<Type, IClickEventHandler> g) => g.Count())
			where pair.Value > 1
			select pair.Key.Name).ToArray();
		if (array.Any())
		{
			throw new Exception(string.Format("Found more then one instances of clickHandlers: " + string.Join(", ", array)));
		}
		m_ClickHandlers = clickHandlers;
	}

	public TickType GetTickType()
	{
		return TickType.BeginOfFrame;
	}

	public void Tick()
	{
		TickPointerDebug();
		if (DebugThisFrame)
		{
			DebugThisFrame = false;
		}
		bool isControllerGamepad = Game.Instance.IsControllerGamepad;
		bool flag = true;
		bool inGui = InGui;
		Vector2 position = Cursor.Position;
		Vector3 worldPosition = Vector3.zero;
		Vector3? groundPosition = null;
		GameObject resultGameObject = null;
		IClickEventHandler resultHandler = null;
		if (!inGui)
		{
			SelectClickObject(position, out resultGameObject, out worldPosition, out groundPosition, out resultHandler);
			WorldPositionForSimulation = WorldPosition;
			if (resultGameObject != null)
			{
				WorldPosition = worldPosition;
				GroundPosition = groundPosition ?? worldPosition;
			}
		}
		if (!Cursor.GetMouseButton(m_MouseDownButton) && m_MouseDown)
		{
			m_MouseDown = false;
			if (m_MouseDrag && m_DragFrames < 2)
			{
				m_MouseDrag = false;
				if ((bool)MultiSelection)
				{
					MultiSelection.Cancel();
				}
			}
			bool flag2 = InteractionHighlightController.Instance?.IsGlobalHighlighting ?? false;
			if (m_MouseDownButton == MouseButton.Right && !flag2 && Mode != 0)
			{
				ClearPointerMode();
			}
			else if (m_MouseDrag && Mode == PointerMode.Default)
			{
				if (m_MouseDownButton == MouseButton.Left)
				{
					if ((bool)MultiSelection)
					{
						MultiSelection.SelectEntities();
					}
				}
				else if (m_MouseDownHandler is IDragClickEventHandler dragClickEventHandler && m_MouseDownOn != null && dragClickEventHandler.OnClick(m_MouseDownOn, m_MouseDownWorldPosition, worldPosition))
				{
					EventBus.RaiseEvent(delegate(IClickMarkHandler h)
					{
						h.OnClickHandled(m_MouseDownWorldPosition);
					});
				}
			}
			else if (flag && m_MouseDownHandler != null && m_MouseDownOn != null)
			{
				Vector3 clickPosition = ((m_MouseDownHandler is ClickGroundHandler) ? m_MouseDownGroundPosition : m_MouseDownWorldPosition);
				if (m_MouseDownHandler.OnClick(m_MouseDownOn, clickPosition, (int)m_MouseDownButton))
				{
					EventBus.RaiseEvent(delegate(IClickMarkHandler h)
					{
						h.OnClickHandled(clickPosition);
					});
				}
			}
			m_MouseDownOn = null;
			m_MouseDrag = false;
		}
		if (PointerOn != resultGameObject)
		{
			OnHoverChanged(PointerOn, resultGameObject);
			PointerOn = resultGameObject;
		}
		if (!isControllerGamepad && m_MouseDown && Vector2.Distance(m_MouseDownCoord, position) > 4f && !m_MouseDrag && Mode == PointerMode.Default)
		{
			m_MouseDrag = true;
			m_DragFrames = 0;
			if (m_MouseDownButton == MouseButton.Left)
			{
				if ((bool)MultiSelection && MultiSelection.ShouldMultiSelect)
				{
					MultiSelection.CreateBoxSelection(m_MouseDownCoord);
				}
			}
			else if (m_MouseDownHandler is IDragClickEventHandler dragClickEventHandler2)
			{
				dragClickEventHandler2.OnStartDrag(m_MouseDownOn, m_MouseDownWorldPosition);
			}
		}
		if (m_MouseDrag && Time.unscaledTime - m_MouseButtonTime >= 0.07f)
		{
			if (m_MouseDownButton == MouseButton.Left && (bool)MultiSelection)
			{
				MultiSelection.DragBoxSelection();
			}
			m_DragFrames++;
		}
		if (!m_MouseDown && (Cursor.GetMouseButtonDown(MouseButton.Left) || Cursor.GetMouseButtonDown(MouseButton.Right)) && !inGui)
		{
			m_MouseDownButton = ((!Cursor.GetMouseButtonDown(MouseButton.Left)) ? MouseButton.Right : MouseButton.Left);
			m_MouseDown = true;
			m_MouseDownOn = resultGameObject;
			m_MouseDownHandler = resultHandler;
			m_MouseDownCoord = position;
			m_MouseDownWorldPosition = WorldPosition;
			m_MouseDownGroundPosition = GroundPosition;
			m_MouseButtonTime = Time.unscaledTime;
		}
		if (!isControllerGamepad && m_MouseDown && m_MouseDownButton == MouseButton.Right && !TurnController.IsInTurnBasedCombat() && m_MouseDownHandler is IDragClickEventHandler dragClickEventHandler3 && m_MouseDownOn != null)
		{
			dragClickEventHandler3.OnDrag(m_MouseDownOn, m_MouseDownWorldPosition, worldPosition);
		}
		if (!m_MouseDown)
		{
			MultiSelection?.Cancel();
		}
	}

	public void SetPointerMode(PointerMode mode)
	{
		if (Mode != mode)
		{
			if (Mode == PointerMode.Default)
			{
				EscHotkeyManager.Instance.Subscribe(ClearPointerMode);
			}
			else if (mode == PointerMode.Default)
			{
				EscHotkeyManager.Instance.Unsubscribe(ClearPointerMode);
			}
			if (Mode == PointerMode.Ability)
			{
				GetHandler<ClickWithSelectedAbilityHandler>().DropAbility();
			}
			PointerMode oldMode = Mode;
			Mode = mode;
			EventBus.RaiseEvent(delegate(IPointerModeHandler h)
			{
				h.OnPointerModeChanged(oldMode, mode);
			});
		}
	}

	public void ClearPointerMode()
	{
		SetPointerMode(PointerMode.Default);
	}

	public void OnEnable()
	{
		m_IsTurnBased = Game.Instance.Controllers.TurnController.TurnBasedModeActive;
	}

	public void OnDisable()
	{
		if (SkipDeactivation)
		{
			SkipDeactivation = false;
			return;
		}
		if ((bool)PointerOn)
		{
			OnHoverChanged(PointerOn, null);
		}
		if (m_MouseDrag)
		{
			MultiSelection.SelectEntities();
		}
		m_MouseDown = false;
		m_MouseDrag = false;
		PointerOn = null;
		ClearPointerMode();
	}

	private void OnHoverChanged(GameObject oldHover, GameObject newHover)
	{
		EventBus.RaiseEvent(delegate(IMouseHoverHandler h)
		{
			h.OnHoverObjectChanged(oldHover, newHover);
		});
		if ((bool)oldHover)
		{
			oldHover.GetComponents(m_HoverComponents);
			foreach (IDetectHover hoverComponent in m_HoverComponents)
			{
				hoverComponent.HandleHoverChange(isHover: false);
			}
		}
		if (!newHover)
		{
			return;
		}
		newHover.GetComponents(m_HoverComponents);
		foreach (IDetectHover hoverComponent2 in m_HoverComponents)
		{
			hoverComponent2.HandleHoverChange(isHover: true);
		}
	}

	private void SelectClickObject(Vector2 mousePosition, out GameObject resultGameObject, out Vector3 worldPosition, out Vector3? groundPosition, out IClickEventHandler resultHandler)
	{
		worldPosition = Vector3.zero;
		groundPosition = null;
		resultGameObject = null;
		resultHandler = null;
		if (MainCamera == null)
		{
			return;
		}
		int num = Physics.RaycastNonAlloc(MainCamera.ScreenPointToRay(mousePosition), m_Hits, MainCamera.farClipPlane, 70014209);
		if (num == m_Hits.Length)
		{
			PFLog.Default.Warning($"Reached limit for hits count, increasing to {m_Hits.Length * 2}");
			Array.Resize(ref m_Hits, m_Hits.Length * 2);
		}
		Array.Sort(m_Hits, 0, num, HitComparer);
		if (PointerDebug)
		{
			if (Hits == null)
			{
				List<string> list2 = (Hits = new List<string>());
			}
			Hits.Clear();
		}
		float num2 = 0f;
		float num3 = float.MaxValue;
		int num4 = -1;
		int num5 = -1;
		bool flag = false;
		Vector3? vector = null;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = m_Hits[i];
			float num6 = CalcColliderPriority(raycastHit);
			if (num6 <= 0f)
			{
				continue;
			}
			bool flag2 = IsGround(raycastHit.collider.gameObject);
			if (flag && flag2)
			{
				continue;
			}
			if (flag2)
			{
				groundPosition = raycastHit.point;
				flag = true;
			}
			IClickEventHandler clickEventHandler = null;
			float num7 = 0f;
			GameObject clickableObject = GetClickableObject(raycastHit);
			if ((bool)clickableObject)
			{
				for (int j = 0; j < m_ClickHandlers.Length; j++)
				{
					clickEventHandler = m_ClickHandlers[j];
					if (clickEventHandler.GetMode() == Mode)
					{
						try
						{
							num7 = clickEventHandler.GetPriority(clickableObject, raycastHit.point).Priority;
						}
						catch (Exception ex)
						{
							PFLog.Default.Exception(ex);
						}
						if (num7 > 0f)
						{
							break;
						}
					}
				}
			}
			if (PointerDebug)
			{
				Hits.Add(string.Format("{1}{2} Hit: {0} go={3}, {4}", raycastHit.collider.name, IsGround(raycastHit.collider.gameObject) ? "(G)" : "", IsSecondary(raycastHit.collider.gameObject) ? "(S)" : "", clickableObject ? clickableObject.name : "<null>", ((bool)clickableObject && num7 > 0f) ? "can click" : "can't click"));
				num4++;
			}
			if (num7 <= 0f)
			{
				if (PointerDebug)
				{
					Hits[num4] += "can't click";
				}
				continue;
			}
			float num8 = num7 * 10f + num6;
			if (PointerDebug)
			{
				Hits[num4] += num8;
			}
			bool num9 = IsGround(clickableObject) && clickableObject.layer != 0;
			bool flag3 = (bool)resultGameObject && IsGround(resultGameObject) && resultGameObject.layer != 0;
			if (num9 && raycastHit.distance < num3 - 0.1f)
			{
				num8 = num2;
				if (PointerDebug)
				{
					Hits[num4] += " ground-above";
				}
			}
			if (flag3 && raycastHit.distance > num3 + 0.1f)
			{
				num8 = -1f;
				if (PointerDebug)
				{
					Hits[num4] += " below-ground";
				}
			}
			if (num8 >= num2)
			{
				worldPosition = raycastHit.point;
				groundPosition = vector;
				resultGameObject = clickableObject;
				resultHandler = clickEventHandler;
				num2 = num8;
				num3 = raycastHit.distance;
				if (PointerDebug)
				{
					num5 = num4;
				}
			}
		}
		if (PointerDebug && num5 >= 0)
		{
			Hits[num5] = "* " + Hits[num5] + " *";
		}
	}

	private GameObject GetClickableObject(RaycastHit hit)
	{
		GameObject gameObject = hit.collider.gameObject;
		if (IsGround(gameObject) && !IsThinCover(gameObject))
		{
			if (m_IsTurnBased)
			{
				BaseUnitEntity baseUnitEntity = hit.point.GetNearestNodeXZUnwalkable()?.GetFirstUnit();
				if (baseUnitEntity == null)
				{
					return gameObject;
				}
				if (SizePathfindingHelper.IsCloserThanHalfCell(baseUnitEntity, hit.point))
				{
					return baseUnitEntity.View.gameObject;
				}
			}
			return gameObject;
		}
		if (HasClickComponents(gameObject, out var detectClicks))
		{
			return detectClicks.Target;
		}
		EntityViewBase componentInParent = gameObject.GetComponentInParent<EntityViewBase>();
		if ((bool)componentInParent)
		{
			if (componentInParent.Data is MechanicEntity { IsInPlayerParty: not false } mechanicEntity && componentInParent.Data != null && SizePathfindingHelper.IsCloserThanHalfCell(mechanicEntity, hit.point))
			{
				return componentInParent.gameObject;
			}
			if (!m_IsTurnBased || componentInParent.Data is MechanicEntity { IsDead: false })
			{
				return componentInParent.gameObject;
			}
		}
		return null;
	}

	private static bool IsGround(GameObject go)
	{
		if ((bool)go)
		{
			return go.IsLayerMask(Layers.WalkableMask);
		}
		return false;
	}

	private static bool IsSecondary(GameObject go)
	{
		return go.CompareTag("SecondarySelection");
	}

	private static bool IsCover(GameObject go)
	{
		return go.GetComponentInParent<CoverEntityView>() != null;
	}

	private static bool IsThinCover(GameObject go)
	{
		return go.GetComponentInParent<ThinCoverEntityView>() != null;
	}

	private static bool HasClickComponents(GameObject go, out IDetectClicks detectClicks)
	{
		detectClicks = go.GetComponentNonAlloc<IDetectClicks>();
		return detectClicks != null;
	}

	private float CalcColliderPriority(RaycastHit h)
	{
		if (!h.collider)
		{
			return float.MinValue;
		}
		GameObject gameObject = h.collider.gameObject;
		if (IsGround(gameObject))
		{
			return 0.2f - h.distance * 0.0001f;
		}
		if (IsCover(gameObject) || IsThinCover(gameObject) || HasClickComponents(gameObject, out var _))
		{
			return 0.5f - h.distance * 0.0001f;
		}
		if (IsSecondary(gameObject))
		{
			Camera camera = Game.Instance.GetCamera();
			UnitEntityView componentInParent = gameObject.GetComponentInParent<UnitEntityView>();
			Vector3 position = (componentInParent ? (componentInParent.transform.position + Vector3.up * ConfigRoot.Instance.Prefabs.CoreColliderHeight) : h.collider.transform.position);
			Vector3 vector = camera.WorldToScreenPoint(position);
			Vector3 vector2 = camera.WorldToScreenPoint(h.point);
			float sqrMagnitude = (vector - vector2).sqrMagnitude;
			return 0.8f - sqrMagnitude * 1E-06f;
		}
		return 0.9f - h.distance * 0.0001f;
	}

	public T GetHandler<T>() where T : IClickEventHandler
	{
		IClickEventHandler[] clickHandlers = m_ClickHandlers;
		foreach (IClickEventHandler clickEventHandler in clickHandlers)
		{
			if (clickEventHandler is T)
			{
				return (T)clickEventHandler;
			}
		}
		return default(T);
	}

	public void SimulateClick(GameObject onObject, bool muteEvents)
	{
		Vector3 position = onObject.transform.position;
		IClickEventHandler clickEventHandler = null;
		float num = float.MinValue;
		IClickEventHandler[] clickHandlers = m_ClickHandlers;
		foreach (IClickEventHandler clickEventHandler2 in clickHandlers)
		{
			float priority = clickEventHandler2.GetPriority(onObject, position).Priority;
			if (priority > num || clickEventHandler == null)
			{
				clickEventHandler = clickEventHandler2;
				num = priority;
			}
		}
		clickEventHandler?.OnClick(onObject, position, 0, simulate: true, muteEvents);
	}

	private void TickPointerDebug()
	{
		if (!PointerDebug)
		{
			return;
		}
		if (InGui)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current)
			{
				position = Cursor.Position
			};
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, list);
			UIObjectsRaycasts = string.Empty;
			using List<RaycastResult>.Enumerator enumerator = list.GetEnumerator();
			if (enumerator.MoveNext())
			{
				RaycastResult current = enumerator.Current;
				Transform parent = current.gameObject.GetComponent<MonoBehaviour>().transform.parent;
				string text = string.Empty;
				while (parent != null)
				{
					text = text + parent.gameObject.name + " - ";
					parent = parent.parent;
				}
				UIObjectsRaycasts = UIObjectsRaycasts + current.gameObject.name + " (" + text + ")";
			}
			return;
		}
		UIObjectsRaycasts = string.Empty;
	}

	void IPlayerAbilitiesHandler.HandleAbilityAdded(Ability ability)
	{
	}

	void IPlayerAbilitiesHandler.HandleAbilityRemoved(Ability ability)
	{
		if (Mode == PointerMode.Ability)
		{
			ClickWithSelectedAbilityHandler handler = GetHandler<ClickWithSelectedAbilityHandler>();
			bool num = handler.Ability != null && handler.Ability.Blueprint.SameAbility(ability.Blueprint);
			bool flag = handler.Ability?.Caster == ability.Owner;
			if (num && flag)
			{
				ClearPointerMode();
			}
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		HandleTurnBasedMode(isTurnBased);
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		HandleTurnBasedMode(isTurnBased: true);
	}

	private void HandleTurnBasedMode(bool isTurnBased)
	{
		m_IsTurnBased = isTurnBased;
		ClearPointerMode();
	}
}
