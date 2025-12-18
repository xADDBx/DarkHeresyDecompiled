using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
	public Animator animator;

	private bool isRunning;

	[ContextMenu("Toggle Run/Walk")]
	public void ToggleAnimation()
	{
		isRunning = !isRunning;
		animator.SetBool("isRunning", isRunning);
	}
}
