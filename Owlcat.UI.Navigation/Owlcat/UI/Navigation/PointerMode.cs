using System.Collections.Generic;
using Owlcat.UI.Commands;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Owlcat.UI.Navigation;

internal class PointerMode : FocusLayerNavigation.INavigationMode
{
	private readonly EventSystem m_EventSystem;

	private readonly PointerEventData m_EventData;

	private readonly FocusLayer m_Layer;

	private readonly FocusCommandProviderCollection m_Hierarhy;

	private readonly IPointerProvider m_Pointer;

	public PointerMode(EventSystem eventSystem, FocusLayer layer, FocusCommandProviderCollection hierarhy, IPointerProvider pointer)
	{
		m_EventSystem = eventSystem;
		m_EventData = new PointerEventData(eventSystem);
		m_Layer = layer;
		m_Hierarhy = hierarhy;
		m_Pointer = pointer;
	}

	public void OnEnter()
	{
		m_Pointer.Enabled = true;
	}

	public void OnUpdate()
	{
		List<RaycastResult> value;
		using (CollectionPool<List<RaycastResult>, RaycastResult>.Get(out value))
		{
			m_EventData.position = m_Pointer.Position;
			m_EventSystem.RaycastAll(m_EventData, value);
			List<ICommandProvider> value2;
			using (CollectionPool<List<ICommandProvider>, ICommandProvider>.Get(out value2))
			{
				foreach (RaycastResult item in value)
				{
					FocusUtility.GetHierarhyNonAlloc(m_Layer.gameObject, item.gameObject, value2);
				}
				m_Hierarhy.Reset(value2);
			}
		}
	}

	public void OnLateUpdate()
	{
		GameObject currentSelectedGameObject = m_EventSystem.currentSelectedGameObject;
		if (currentSelectedGameObject != null && currentSelectedGameObject.transform.IsChildOf(m_Layer.transform))
		{
			m_EventSystem.SetSelectedGameObject(null);
		}
	}
}
