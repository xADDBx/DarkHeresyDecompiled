using System;
using System.Threading.Tasks;
using Owlcat.UI.Lookup;
using UnityEngine;

namespace Owlcat.UI;

[CreateAssetMenu(fileName = "DefaultLookupTable", menuName = "ScriptableObject/Owlcat/UI/DefaultLookupTable")]
public class DefaultLookupTable : LookupTable<DefaultLookupTable.Provider>
{
	[Serializable]
	public class Provider : ILookupPrefabProvider
	{
		[SerializeField]
		private GameObject m_Prefab;

		public Task<GameObject> GetRuntimePrefab()
		{
			return Task.FromResult(m_Prefab);
		}
	}
}
