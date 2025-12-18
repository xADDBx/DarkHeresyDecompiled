using UnityEngine;

public class CharacterMovement1 : MonoBehaviour
{
	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			animator.SetInteger("MoveType", 0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			animator.SetInteger("MoveType", 1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			animator.SetInteger("MoveType", 2);
		}
	}
}
