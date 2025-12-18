using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	private Animator animator;

	[Range(-10f, 10f)]
	public float manualSlope;

	[Range(0f, 2f)]
	public int manualMoveType;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		int value = ((!Input.GetKey(KeyCode.W)) ? manualMoveType : ((!Input.GetKey(KeyCode.LeftShift)) ? 1 : 2));
		animator.SetInteger("MoveType", value);
		animator.SetFloat("Slope", manualSlope);
	}
}
