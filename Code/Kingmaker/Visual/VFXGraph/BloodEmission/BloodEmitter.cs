using System;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Visual.VFXGraph.BloodEmission;

public class BloodEmitter : MonoBehaviour
{
	private float m_CurrentTime;

	[SerializeField]
	private GameObject m_BloodSplatterPrefab;

	public float Delay;

	[Range(0f, 1f)]
	public float Direction;

	[Range(0f, 90f)]
	public float ConeAngle;

	[Range(1f, 10f)]
	public float Distance = 1f;

	public Gradient RandomColor = new Gradient();

	[MinMaxSlider(0.1f, 5f)]
	public Vector2 RandomSize = new Vector2(1f, 1f);

	[IntLayerMask]
	public int RaycastLayerMask = -1;

	private void OnEnable()
	{
		m_CurrentTime = 0f;
		_ = MonoSingleton<BloodEmissionDelegate>.Instance;
	}

	private void Update()
	{
		m_CurrentTime += Time.deltaTime;
		if (m_CurrentTime >= Delay)
		{
			EmitBlood();
			base.enabled = false;
		}
	}

	public void EmitBlood()
	{
		Vector3 randomDirectionInsideCone = GetRandomDirectionInsideCone();
		if (Physics.Raycast(new Ray(base.transform.position, randomDirectionInsideCone), out var hitInfo, Distance, RaycastLayerMask))
		{
			Vector3 point = hitInfo.point;
			Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.up, hitInfo.normal).eulerAngles;
			Vector3 emitSize = Vector3.one * UnityEngine.Random.Range(RandomSize.x, RandomSize.y);
			Color linear = RandomColor.Evaluate(UnityEngine.Random.value).linear;
			MonoSingleton<BloodEmissionDelegate>.Instance.Emit(point, eulerAngles, emitSize, linear, m_BloodSplatterPrefab);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.green;
		Quaternion q = Quaternion.Slerp(b: Quaternion.FromToRotation(Vector3.down, Vector3.back), a: Quaternion.identity, t: Direction);
		Matrix4x4 matrix4x = Matrix4x4.Rotate(q);
		_ = base.transform.localToWorldMatrix * matrix4x;
		Vector3 directionWorldSpace = GetDirectionWorldSpace();
		Gizmos.DrawLine(base.transform.position, base.transform.position + directionWorldSpace * Distance);
		if (Physics.Raycast(new Ray(base.transform.position, directionWorldSpace), out var hitInfo, Distance))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(hitInfo.point, 0.05f);
			Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
		}
		Gizmos.color = color;
	}

	public Vector3 GetDirectionWorldSpace()
	{
		Quaternion b = Quaternion.FromToRotation(Vector3.down, Vector3.back);
		b = Quaternion.Slerp(Quaternion.identity, b, Direction);
		Matrix4x4 matrix4x = Matrix4x4.Rotate(b);
		return (base.transform.localToWorldMatrix * matrix4x).MultiplyVector(Vector3.down).normalized;
	}

	public Vector3 GetRandomDirectionInsideCone()
	{
		float num = Mathf.Tan(MathF.PI / 180f * ConeAngle * 0.5f) * Distance;
		Vector2 vector = UnityEngine.Random.insideUnitCircle * num;
		Vector3 vector2 = new Vector3(vector.x, 0f - Distance, vector.y);
		Quaternion b = Quaternion.FromToRotation(Vector3.down, Vector3.back);
		b = Quaternion.Slerp(Quaternion.identity, b, Direction);
		Matrix4x4 matrix4x = Matrix4x4.Rotate(b);
		return (base.transform.localToWorldMatrix * matrix4x).MultiplyVector(vector2).normalized;
	}
}
