using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.ResourceLinks;
using Owlcat.UI.Lookup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[CreateAssetMenu(menuName = "ScriptableObject/Owlcat/UI/PrefabLinkLookupTable")]
public class PrefabLinkLookupTable : LookupTable<PrefabLinkLookupTable.Provider>, IEnumerable<PrefabLink>, IEnumerable
{
	[Serializable]
	public class Provider : ILookupPrefabProvider
	{
		[RequireSeparateBundle]
		[SerializeField]
		private PrefabLink m_Prefab = new PrefabLink();

		public PrefabLink Prefab => m_Prefab;

		public Task<GameObject> GetRuntimePrefab()
		{
			return m_Prefab.LoadAsync();
		}
	}

	IEnumerator<PrefabLink> IEnumerable<PrefabLink>.GetEnumerator()
	{
		return (from x in GetProviders()
			select x.Prefab).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return (from x in GetProviders()
			select x.Prefab).GetEnumerator();
	}
}
