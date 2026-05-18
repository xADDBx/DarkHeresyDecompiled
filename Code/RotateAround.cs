using UnityEngine;

public class RotateAround : MonoBehaviour
{
	[Header("Target Settings")]
	[SerializeField]
	private Transform target;

	[Header("Orbit Settings")]
	[SerializeField]
	private float speed = 50f;

	[Header("Axis Settings")]
	[SerializeField]
	[Range(-180f, 180f)]
	private float axisTiltAngle;

	[SerializeField]
	private Vector3 tiltReferenceAxis = Vector3.right;

	private float _radius;

	private float _currentOrbitAngle;

	private Transform _myTransform;

	private void Awake()
	{
		_myTransform = base.transform;
	}

	private void Start()
	{
		if (!(target == null))
		{
			Vector3 vector = _myTransform.position - target.position;
			_radius = vector.magnitude;
			Vector3 vector2 = Quaternion.Inverse(Quaternion.AngleAxis(axisTiltAngle, tiltReferenceAxis.normalized)) * vector;
			_currentOrbitAngle = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
		}
	}

	private void Update()
	{
		if (!(target == null))
		{
			_currentOrbitAngle += speed * Time.deltaTime;
			_currentOrbitAngle %= 360f;
			Quaternion quaternion = Quaternion.AngleAxis(_currentOrbitAngle, Vector3.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(axisTiltAngle, tiltReferenceAxis.normalized) * quaternion;
			_myTransform.position = target.position + quaternion2 * Vector3.forward * _radius;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!(target == null))
		{
			Quaternion quaternion = Quaternion.AngleAxis(axisTiltAngle, tiltReferenceAxis.normalized);
			float num = (Application.isPlaying ? _radius : Vector3.Distance(base.transform.position, target.position));
			Gizmos.color = Color.green;
			float num2 = 5f;
			Vector3 from = target.position + quaternion * Quaternion.AngleAxis(0f, Vector3.up) * Vector3.forward * num;
			for (float num3 = num2; num3 <= 360f; num3 += num2)
			{
				Quaternion quaternion2 = quaternion * Quaternion.AngleAxis(num3, Vector3.up);
				Vector3 vector = target.position + quaternion2 * Vector3.forward * num;
				Gizmos.DrawLine(from, vector);
				from = vector;
			}
			Vector3 vector2 = quaternion * Vector3.up;
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(target.position - vector2 * num * 1.5f, target.position + vector2 * num * 1.5f);
		}
	}
}
