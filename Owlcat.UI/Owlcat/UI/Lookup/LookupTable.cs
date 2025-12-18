using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Owlcat.UI.Lookup;

public abstract class LookupTable<T> : ScriptableObject, ILookupTable, IComparer<Type> where T : ILookupPrefabProvider, new()
{
	[SerializeField]
	private List<LookupTableEntry> m_Table = new List<LookupTableEntry>();

	public async Task<MonoBehaviour> FirstOrDefault(Type type)
	{
		foreach (LookupTableEntry item in m_Table)
		{
			if (item.TypeFullName == type.FullName)
			{
				GameObject gameObject = await item.Prefab.GetRuntimePrefab();
				return (gameObject != null) ? (gameObject.GetComponent<IBindable>() as MonoBehaviour) : null;
			}
		}
		return null;
	}

	public int Compare(Type x, Type y)
	{
		string xFullName = x.FullName;
		string yFullName = y.FullName;
		int num = m_Table.FindIndex((LookupTableEntry e) => e.TypeFullName == xFullName);
		int num2 = m_Table.FindIndex((LookupTableEntry e) => e.TypeFullName == yFullName);
		return num - num2;
	}

	protected IEnumerable<T> GetProviders()
	{
		return (from x in m_Table
			select x.Prefab into x
			where x != null
			select x).Cast<T>();
	}
}
