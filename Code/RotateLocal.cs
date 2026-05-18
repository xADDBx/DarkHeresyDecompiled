using UnityEngine;

public class RotateLocal : MonoBehaviour
{
	[Header("Rotation Settings")]
	[SerializeField]
	private float speedDegPerSec = 90f;

	[Header("Axis Settings")]
	[SerializeField]
	private Vector3 localAxis = Vector3.up;

	private float _currentRotationAngle;

	private Transform _myTransform;

	private Quaternion _baseLocalRotation;

	private void Awake()
	{
		_myTransform = base.transform;
	}

	private void Start()
	{
		_baseLocalRotation = _myTransform.localRotation;
	}

	private void Update()
	{
		if (!(localAxis.sqrMagnitude <= 0f))
		{
			Vector3 normalized = localAxis.normalized;
			_currentRotationAngle += speedDegPerSec * Time.deltaTime;
			_currentRotationAngle %= 360f;
			Quaternion quaternion = Quaternion.AngleAxis(_currentRotationAngle, normalized);
			_myTransform.localRotation = _baseLocalRotation * quaternion;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (localAxis.sqrMagnitude <= 0f)
		{
			return;
		}
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		if (componentInChildren == null)
		{
			return;
		}
		float num = componentInChildren.bounds.extents.magnitude * 1f;
		if (!(num <= 0f))
		{
			Vector3 vector = base.transform.TransformDirection(localAxis.normalized);
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector);
			Vector3 position = base.transform.position;
			Gizmos.color = Color.green;
			float num2 = 5f;
			Vector3 from = position + quaternion * Quaternion.AngleAxis(0f, Vector3.up) * Vector3.forward * num;
			for (float num3 = num2; num3 <= 360f; num3 += num2)
			{
				Quaternion quaternion2 = quaternion * Quaternion.AngleAxis(num3, Vector3.up);
				Vector3 vector2 = position + quaternion2 * Vector3.forward * num;
				Gizmos.DrawLine(from, vector2);
				from = vector2;
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(position - vector * num * 1.5f, position + vector * num * 1.5f);
		}
	}
}
