using System;
using UnityEngine;

namespace Owlcat.UI.Lookup;

[Serializable]
internal class LookupTableEntry
{
	[SerializeField]
	private string m_TypeFullName;

	[SerializeReference]
	private ILookupPrefabProvider m_Prefab;

	public string TypeFullName => m_TypeFullName;

	public ILookupPrefabProvider Prefab => m_Prefab;

	public LookupTableEntry(Type type, ILookupPrefabProvider prefab)
		: this(type.FullName, prefab)
	{
	}

	public LookupTableEntry(string typeFullName, ILookupPrefabProvider prefab)
	{
		m_TypeFullName = typeFullName;
		m_Prefab = prefab;
	}
}
