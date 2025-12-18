using UnityEngine;

namespace Kingmaker;

public class AnimatorClear : MonoBehaviour
{
	[SerializeField]
	private Animator anim01;

	private void Start()
	{
		anim01.runtimeAnimatorController = null;
	}

	private void Update()
	{
	}
}
