using UnityEngine;

public class RunningBackgroundDebug : MonoBehaviour
{
	[SerializeField]
	[Tooltip("RunningBackground instance whose biome will be swapped at runtime")]
	private RunningBackground target;

	[SerializeField]
	[Tooltip("Biome config to apply when the Apply checkbox is toggled")]
	private RunningBackgroundBiomeConfig biome;

	[SerializeField]
	[Tooltip("Toggle to apply the biome change at runtime (auto-resets after applying)")]
	private bool apply;

	private void OnValidate()
	{
		if (apply && !(target == null) && !(biome == null))
		{
			apply = false;
			if (Application.isPlaying)
			{
				target.ChangeBiome(biome);
			}
		}
	}
}
