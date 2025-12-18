using UnityEngine;

namespace Kingmaker;

public class FallingRagdollTimelineStart : MonoBehaviour
{
	[SerializeField]
	private Animator anim01;

	private void Start()
	{
		anim01.enabled = false;
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isKinematic = false;
		}
	}
}
