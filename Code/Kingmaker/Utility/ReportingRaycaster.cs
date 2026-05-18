using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.UI.Pointer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Utility;

public class ReportingRaycaster : MonoBehaviour
{
	public static ReportingRaycaster Instance { get; private set; }

	public string GetJiraLabel(string feature)
	{
		return feature ?? string.Empty;
	}

	public string GetFeatureName()
	{
		Vector2 position = ((Input.GetJoystickNames().Length != 0) ? new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f) : CursorController.CursorPosition);
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = position
		};
		List<RaycastResult> list = new List<RaycastResult>();
		CanvasGroup canvasGroup = null;
		IMainCanvas instance = MainCanvasFacade.Instance;
		if (instance != null)
		{
			CanvasGroup canvasGroup2 = instance.GetCanvasGroup();
			if ((object)canvasGroup2 != null && !canvasGroup2.blocksRaycasts)
			{
				canvasGroup2.blocksRaycasts = true;
				canvasGroup = canvasGroup2;
			}
		}
		try
		{
			EventSystem.current.RaycastAll(eventData, list);
		}
		finally
		{
			if ((bool)canvasGroup)
			{
				canvasGroup.blocksRaycasts = false;
			}
		}
		foreach (RaycastResult item in list)
		{
			Transform parent = item.gameObject.transform;
			while (parent != null)
			{
				string uIContext = Game.Instance.BugReportContext.GetUIContext(parent.gameObject);
				if (uIContext != null)
				{
					return uIContext;
				}
				parent = parent.parent;
			}
		}
		return string.Empty;
	}

	public string GetUnderMouseBlueprintName()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = CursorController.CursorPosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			string blueprintName = GetBlueprintName(item.gameObject.transform);
			if (!string.IsNullOrEmpty(blueprintName))
			{
				return blueprintName;
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				blueprintName = GetBlueprintName(parent.gameObject.transform);
				if (!string.IsNullOrEmpty(blueprintName))
				{
					return blueprintName;
				}
				parent = parent.parent;
			}
		}
		return string.Empty;
	}

	private string GetBlueprintName(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour parent in components)
		{
			string uIBlueprintName = Game.Instance.BugReportContext.GetUIBlueprintName(parent);
			if (!string.IsNullOrEmpty(uIBlueprintName))
			{
				return uIBlueprintName;
			}
		}
		return string.Empty;
	}

	public BlueprintScriptableObject GetUnderMouseBlueprint()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = CursorController.CursorPosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			BlueprintScriptableObject blueprint = GetBlueprint(item.gameObject.transform);
			if (blueprint != null)
			{
				return blueprint;
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				blueprint = GetBlueprint(parent.gameObject.transform);
				if (blueprint != null)
				{
					return blueprint;
				}
				parent = parent.parent;
			}
		}
		return null;
	}

	public string GetOtherUiFeatureName()
	{
		return BugReportService.GetOtherUIFeatureName();
	}

	private BlueprintScriptableObject GetBlueprint(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour parent in components)
		{
			BlueprintScriptableObject blueprint = Game.Instance.BugReportContext.GetBlueprint(parent);
			if (blueprint != null)
			{
				return blueprint;
			}
		}
		return null;
	}

	public void OnEnable()
	{
		Instance = this;
	}

	public void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
