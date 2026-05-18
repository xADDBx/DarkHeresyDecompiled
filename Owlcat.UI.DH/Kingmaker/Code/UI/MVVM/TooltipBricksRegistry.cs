using System;
using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[CreateAssetMenu(menuName = "ScriptableObject/Owlcat/UI/TooltipBricksRegistry")]
public class TooltipBricksRegistry : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		[field: SerializeField]
		public string VMTypeName { get; private set; }

		[field: SerializeField]
		public MonoBehaviour ViewPrefab { get; private set; }
	}

	[SerializeField]
	private List<Entry> m_Entries = new List<Entry>();

	private Dictionary<Type, IBrickView> m_RuntimeCache;

	public IBrickView GetViewPrefab(Type vmType)
	{
		if (m_RuntimeCache == null)
		{
			BuildRuntimeCache();
		}
		return m_RuntimeCache.GetValueOrDefault(vmType);
	}

	public IBrickView GetView(Type vmType, bool strictMatching = true)
	{
		if (vmType == null)
		{
			return null;
		}
		IBrickView viewPrefab = GetViewPrefab(vmType);
		if (viewPrefab == null)
		{
			Debug.LogWarning("[TooltipBricksRegistry] No prefab registered for " + vmType.Name);
			return null;
		}
		return (IBrickView)WidgetFactory.GetWidget((MonoBehaviour)viewPrefab, activate: true, strictMatching);
	}

	private void BuildRuntimeCache()
	{
		m_RuntimeCache = new Dictionary<Type, IBrickView>();
		foreach (Entry entry in m_Entries)
		{
			if (!(entry.ViewPrefab == null))
			{
				Type type = Type.GetType(entry.VMTypeName);
				if (type != null && entry.ViewPrefab is IBrickView value)
				{
					m_RuntimeCache[type] = value;
				}
			}
		}
	}
}
