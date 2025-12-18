using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
	private Animator animator;

	private CharacterController controller;

	private void Start()
	{
		animator = GetComponent<Animator>();
		controller = GetComponent<CharacterController>();
	}

	private void Update()
	{
		float magnitude = controller.velocity.magnitude;
		animator.SetFloat("Speed", magnitude);
	}
}
