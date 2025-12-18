using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.Visual.VFXGraph.BloodEmission;

[AddComponentMenu("VFX/Property Binders/Owlcat Blood Binder")]
[VFXBinder("Blood Binder")]
public class BloodBinder : VFXBinderBase
{
	private static class AttributeId
	{
		public static readonly int Position = Shader.PropertyToID("position");

		public static readonly int ScaleX = Shader.PropertyToID("scaleX");

		public static readonly int ScaleY = Shader.PropertyToID("scaleY");

		public static readonly int ScaleZ = Shader.PropertyToID("scaleZ");

		public static readonly int AngleX = Shader.PropertyToID("angleX");

		public static readonly int AngleY = Shader.PropertyToID("angleY");

		public static readonly int AngleZ = Shader.PropertyToID("angleZ");

		public static readonly int Color = Shader.PropertyToID("color");

		public static readonly int SpawnTime = Shader.PropertyToID("bloodSpawnTime");

		public static readonly int SpawnGeneration = Shader.PropertyToID("bloodSpawnGeneration");
	}

	private struct SpawnData
	{
		public Aabb Aabb;

		public float SpawnTime;
	}

	private VisualEffect m_VisualEffect;

	private List<SpawnData> m_SpawnData = new List<SpawnData>();

	private Aabb m_Bounds;

	private float m_KillTime;

	private uint m_KillGeneration;

	private uint m_CurrentGeneration;

	public ExposedProperty SpawnEventName;

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	public ExposedProperty BoundsCenterProperty;

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	public ExposedProperty BoundsSizeProperty;

	[VFXPropertyBinding(new string[] { "System.Single" })]
	public ExposedProperty KillTime;

	[VFXPropertyBinding(new string[] { "System.UInt32" })]
	public ExposedProperty KillGeneration;

	protected override void Awake()
	{
		base.Awake();
		m_VisualEffect = GetComponent<VisualEffect>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_SpawnData.Clear();
		float3 min = -5;
		float3 max = 5;
		m_Bounds = new Aabb(in min, in max);
	}

	public override bool IsValid(VisualEffect component)
	{
		if (component.HasVector3(BoundsCenterProperty) && component.HasVector3(BoundsSizeProperty) && component.HasFloat(KillTime))
		{
			return component.HasUInt(KillGeneration);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetVector3(BoundsCenterProperty, m_Bounds.Center);
		component.SetVector3(BoundsSizeProperty, m_Bounds.Size);
		component.SetFloat(KillTime, m_KillTime);
		component.SetUInt(KillGeneration, m_KillGeneration);
	}

	public override string ToString()
	{
		return "Blood Binder";
	}

	internal void Emit(Vector3 position, Vector3 angles, Vector3 size, Color color)
	{
		float num = (float)Game.Instance.Controllers.TimeController.RealTime.TotalSeconds;
		VFXEventAttribute vFXEventAttribute = m_VisualEffect.CreateVFXEventAttribute();
		vFXEventAttribute.SetVector3(AttributeId.Position, position);
		vFXEventAttribute.SetFloat(AttributeId.ScaleX, size.x);
		vFXEventAttribute.SetFloat(AttributeId.ScaleY, size.y);
		vFXEventAttribute.SetFloat(AttributeId.ScaleZ, size.z);
		vFXEventAttribute.SetFloat(AttributeId.AngleX, angles.x);
		vFXEventAttribute.SetFloat(AttributeId.AngleY, angles.y);
		vFXEventAttribute.SetFloat(AttributeId.AngleZ, angles.z);
		vFXEventAttribute.SetVector3(AttributeId.Color, new Vector3(color.r, color.g, color.b));
		vFXEventAttribute.SetFloat(AttributeId.SpawnTime, num);
		vFXEventAttribute.SetUint(AttributeId.SpawnGeneration, m_CurrentGeneration);
		float3 min = -0.5f;
		float3 max = 0.5f;
		Aabb other = new Aabb(in min, in max);
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.Euler(angles), size);
		other.Transform(in matrix4x);
		if (m_SpawnData.Count == 0)
		{
			m_Bounds = other;
		}
		else
		{
			m_Bounds.Encapsulate(in other);
		}
		m_SpawnData.Add(new SpawnData
		{
			Aabb = other,
			SpawnTime = num
		});
		m_VisualEffect.SendEvent(SpawnEventName, vFXEventAttribute);
	}

	public void KillAll()
	{
		m_KillTime = 0f;
		m_KillGeneration = m_CurrentGeneration;
		m_CurrentGeneration++;
		m_SpawnData.Clear();
		RebuildBounds();
	}

	public void KillOlderThan(float time)
	{
		m_KillTime = time;
		m_KillGeneration = m_CurrentGeneration;
		m_CurrentGeneration++;
		for (int num = m_SpawnData.Count - 1; num >= 0; num--)
		{
			if (m_SpawnData[num].SpawnTime >= m_KillTime)
			{
				m_SpawnData.RemoveAt(num);
			}
		}
		RebuildBounds();
	}

	private void RebuildBounds()
	{
		if (m_SpawnData.Count == 0)
		{
			float3 min = -5;
			float3 max = 5;
			m_Bounds = new Aabb(in min, in max);
			return;
		}
		m_Bounds = m_SpawnData[0].Aabb;
		for (int i = 1; i < m_SpawnData.Count; i++)
		{
			ref Aabb bounds = ref m_Bounds;
			SpawnData spawnData = m_SpawnData[i];
			bounds.Encapsulate(in spawnData.Aabb);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.yellow;
		for (int i = 0; i < m_SpawnData.Count; i++)
		{
			SpawnData spawnData = m_SpawnData[i];
			Gizmos.DrawWireCube(spawnData.Aabb.Center, spawnData.Aabb.Size);
		}
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(m_Bounds.Center, m_Bounds.Size);
		Gizmos.color = color;
	}

	public BloodBinder()
	{
		float3 min = -5;
		float3 max = 5;
		m_Bounds = new Aabb(in min, in max);
		m_KillTime = float.MaxValue;
		m_CurrentGeneration = 1u;
		SpawnEventName = "SpawnBlood";
		BoundsCenterProperty = "BoundsCenter";
		BoundsSizeProperty = "BoundsSize";
		KillTime = "KillTime";
		KillGeneration = "KillGeneration";
		base._002Ector();
	}
}
