using UnityEngine;

namespace Kingmaker;

public class RotationRuntimeFreezer : MonoBehaviour
{
	[field: SerializeField]
	public Vector3 targetRotation { get; private set; }

	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.Euler(targetRotation);
	}
}
