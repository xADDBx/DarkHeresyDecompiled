using System.Threading.Tasks;
using UnityEngine;

namespace Owlcat.UI.Lookup;

public interface ILookupPrefabProvider
{
	Task<GameObject> GetRuntimePrefab();
}
