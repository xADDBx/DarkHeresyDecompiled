using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

public abstract class OwlcatTerrainBase : MonoBehaviour
{
	public virtual Texture2DArray SplatArray { get; }
}
