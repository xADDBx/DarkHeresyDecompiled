using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI;
using UnityEngine;

namespace Kingmaker.Utility;

[Serializable]
public class InterfaceContextHelper
{
	public List<ContextRow> ContextRows;

	public InterfaceContextHelper()
	{
		try
		{
			ContextRows = new List<ContextRow>();
			MonoBehaviour[] source = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			string text = "";
			string text2 = "";
			foreach (KeyValuePair<string, string> kvp in Game.Instance.BugReportContext.GetVMNameToContext())
			{
				try
				{
					MonoBehaviour monoBehaviour = source.FirstOrDefault((MonoBehaviour x) => IsType(x.GetType(), kvp.Key));
					if (!(monoBehaviour == null))
					{
						if (IsVisible(monoBehaviour.transform))
						{
							text = text + kvp.Value + "\n";
						}
						else
						{
							text2 = text2 + kvp.Value + "\n";
						}
					}
				}
				catch
				{
				}
			}
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Visible Interfaces", text)
			}));
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Overlapped Interfaces", text2)
			}));
		}
		catch (Exception ex)
		{
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Exception", ex.Message)
			}));
		}
	}

	private bool IsType(Type tMono, string vmName)
	{
		if (tMono.Name == vmName)
		{
			return true;
		}
		Type baseType = tMono.BaseType;
		if ((object)baseType != null && baseType.IsGenericType && baseType.GetGenericArguments().Length != 0)
		{
			return baseType.GetGenericArguments().Any((Type a) => a.Name == vmName);
		}
		return false;
	}

	private bool IsVisible(Transform transform)
	{
		RectTransform component = transform.GetComponent<RectTransform>();
		if (component != null && CountCornersVisibleFrom(component) > 2)
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			if (IsVisible(item))
			{
				return true;
			}
		}
		return false;
	}

	private int CountCornersVisibleFrom(RectTransform rectTransform)
	{
		Camera instance = UICamera.Instance;
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		int num = 0;
		Vector3[] array2 = array;
		foreach (Vector3 position in array2)
		{
			Vector3 point = instance.WorldToScreenPoint(position);
			if (rect.Contains(point))
			{
				num++;
			}
		}
		return num;
	}
}
