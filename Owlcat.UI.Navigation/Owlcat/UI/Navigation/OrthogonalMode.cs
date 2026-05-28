using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI.Commands;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Owlcat.UI.Navigation;

internal class OrthogonalMode : FocusLayerNavigation.INavigationMode
{
	private readonly EventSystem m_EventSystem;

	private readonly BaseEventData m_EventData;

	private readonly FocusLayer m_Layer;

	private readonly FocusCommandProviderCollection m_Hierarhy;

	private readonly INavigationGraph m_Graph;

	private readonly IPointerProvider m_Pointer;

	private readonly Command m_Submit;

	private readonly Command m_Cancel;

	private readonly Command m_MoveUp;

	private readonly Command m_MoveRight;

	private readonly Command m_MoveDown;

	private readonly Command m_MoveLeft;

	private GameObject m_LastSelectedGameObject;

	public OrthogonalMode(EventSystem eventSystem, FocusLayer layer, FocusCommandProviderCollection hierarhy, IPointerProvider pointer)
	{
		m_EventSystem = eventSystem;
		m_EventData = new BaseEventData(m_EventSystem);
		m_Layer = layer;
		m_Hierarhy = hierarhy;
		m_Pointer = pointer;
		m_Graph = new CompositeNavigationGraph(new ExplicitNavigationGraph(), new HierarhyNavigationGraph(m_Layer.gameObject), new FloatNavigationGraph(m_Layer.gameObject));
		m_Submit = new Command("gamepad:buttonSouth;keyboard:return", OnNavigationSubmit);
		m_Cancel = new Command("gamepad:buttonEast;keyboard:esc", OnNavigationCancel);
		m_MoveUp = new Command("gamepad:up;keyboard:upArrow", (Action)delegate
		{
			OnNavigationMove(Vector2.up);
		});
		m_MoveRight = new Command("gamepad:right;keyboard:rightArrow", (Action)delegate
		{
			OnNavigationMove(Vector2.right);
		});
		m_MoveDown = new Command("gamepad:down;keyboard:downArrow", (Action)delegate
		{
			OnNavigationMove(Vector2.down);
		});
		m_MoveLeft = new Command("gamepad:left;keyboard:leftArrow", (Action)delegate
		{
			OnNavigationMove(Vector2.left);
		});
	}

	public void OnEnter()
	{
		m_Layer.Add(m_Submit);
		m_Layer.Add(m_Cancel);
		m_Layer.Add(m_MoveUp);
		m_Layer.Add(m_MoveRight);
		m_Layer.Add(m_MoveDown);
		m_Layer.Add(m_MoveLeft);
		m_Pointer.Enabled = false;
	}

	public void OnExit()
	{
		m_Layer.Remove(m_Submit);
		m_Layer.Remove(m_Cancel);
		m_Layer.Remove(m_MoveUp);
		m_Layer.Remove(m_MoveRight);
		m_Layer.Remove(m_MoveDown);
		m_Layer.Remove(m_MoveLeft);
		m_EventSystem.SetSelectedGameObject(null);
	}

	private void OnNavigationMove(Vector2 direction)
	{
		if (m_Graph.TryGet(m_EventSystem.currentSelectedGameObject, direction, out var result))
		{
			TrySelect(result);
		}
	}

	private void OnNavigationSubmit()
	{
		ExecuteEvents.Execute(m_EventSystem.currentSelectedGameObject, GetEventData(), ExecuteEvents.submitHandler);
	}

	private void OnNavigationCancel()
	{
		ExecuteEvents.Execute(m_EventSystem.currentSelectedGameObject, GetEventData(), ExecuteEvents.cancelHandler);
	}

	private BaseEventData GetEventData()
	{
		m_EventData.Reset();
		return m_EventData;
	}

	public void OnUpdate()
	{
		UpdateSelection();
		UpdateHierarhy();
	}

	private void UpdateSelection()
	{
		GameObject currentSelectedGameObject = m_EventSystem.currentSelectedGameObject;
		bool flag = currentSelectedGameObject != null && currentSelectedGameObject.activeInHierarchy && currentSelectedGameObject.transform.IsChildOf(m_Layer.transform);
		if (flag && currentSelectedGameObject.TryGetComponent<Selectable>(out var component))
		{
			flag = component.IsInteractable();
		}
		if (flag && currentSelectedGameObject.TryGetComponent<OwlcatSelectable>(out var component2))
		{
			flag = component2.Interactable;
		}
		if (!flag)
		{
			Debug.LogWarningFormat("Selection '{0}' is invalid, try to find valid selection", currentSelectedGameObject);
			if (!TrySelect(m_LastSelectedGameObject) && !(from t in m_Layer.GetComponentsInChildren<RectTransform>()
				select t.gameObject).Any(TrySelect))
			{
				m_EventSystem.SetSelectedGameObject(null);
			}
		}
	}

	private bool TrySelect(GameObject gameObject)
	{
		if (gameObject == null || !gameObject.activeInHierarchy)
		{
			return false;
		}
		int num;
		if (!m_EventSystem.TrySelect(m_Layer.gameObject, gameObject, (Selectable i) => i.IsInteractable()))
		{
			num = (m_EventSystem.TrySelect(m_Layer.gameObject, gameObject, (OwlcatSelectable i) => i.Interactable) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0098;
			}
		}
		else
		{
			num = 1;
		}
		m_LastSelectedGameObject = m_EventSystem.currentSelectedGameObject;
		goto IL_0098;
		IL_0098:
		return (byte)num != 0;
	}

	private void UpdateHierarhy()
	{
		List<ICommandProvider> value;
		using (CollectionPool<List<ICommandProvider>, ICommandProvider>.Get(out value))
		{
			GameObject currentSelectedGameObject = m_EventSystem.currentSelectedGameObject;
			if (currentSelectedGameObject != null)
			{
				FocusUtility.GetHierarhyNonAlloc(m_Layer.gameObject, currentSelectedGameObject, value);
			}
			m_Hierarhy.Reset(value);
		}
	}
}
