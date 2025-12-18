using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides.Vfx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

public class VfxController : OverridableControllerBase<VfxComponent, VfxGraphOverride>
{
	private class VfxEffectController
	{
		private VfxEffect m_Effect;

		private VfxPositionSpace m_PositionSpace;

		private List<VisualEffect> m_VfxInstances = new List<VisualEffect>();

		private int2 m_GridSize = 0;

		private float3 m_CellSize;

		private float m_VisibleSize;

		private bool m_IsInitialized;

		public VfxEffectController(VfxEffect effect, GlobalEffectContext context)
		{
			m_Effect = effect;
			Init(context);
		}

		private void Init(GlobalEffectContext context)
		{
			m_PositionSpace = m_Effect.PositionSpace;
			if (m_Effect.VfxPrefab != null)
			{
				if (!m_Effect.VfxPrefab.TryGetComponent<VisualEffect>(out var _))
				{
					Debug.LogError("VFX prefab " + m_Effect.VfxPrefab.name + " does not have a VisualEffect component. Please add one.", m_Effect.VfxPrefab);
					return;
				}
				switch (m_PositionSpace)
				{
				case VfxPositionSpace.GlobalEffectSpace:
					InitGlobalEffectSpace(context);
					break;
				case VfxPositionSpace.CameraSpace:
					InitCameraSpace(context);
					break;
				}
			}
			m_IsInitialized = true;
		}

		private void InitGlobalEffectSpace(GlobalEffectContext context)
		{
			GameObject gameObject = Object.Instantiate(m_Effect.VfxPrefab, context.GlobalEffect.transform);
			if (m_PositionSpace == VfxPositionSpace.GlobalEffectSpace)
			{
				gameObject.transform.localPosition = m_Effect.Position;
			}
			VisualEffect component = gameObject.GetComponent<VisualEffect>();
			m_VfxInstances.Add(component);
			component.Play();
		}

		private void InitCameraSpace(GlobalEffectContext context)
		{
			switch (m_Effect.PositionType)
			{
			case PositionType.Single:
			{
				VisualEffect component3 = Object.Instantiate(m_Effect.VfxPrefab, context.GlobalEffect.transform).GetComponent<VisualEffect>();
				m_VfxInstances.Add(component3);
				component3.Play();
				break;
			}
			case PositionType.Grid:
			{
				Camera[] allCameras = Camera.allCameras;
				float maxFov = allCameras.Max((Camera c) => (!(c != null)) ? 0f : c.fieldOfView);
				Camera camera = allCameras.FirstOrDefault((Camera c) => c != null && c.fieldOfView >= maxFov);
				m_VisibleSize = CalculateVisibleSize(camera, m_Effect.DistanceFromCamera);
				VisualEffect component = m_Effect.VfxPrefab.GetComponent<VisualEffect>();
				if (!(component != null) || !component.HasVector3(m_Effect.BoundsProperty))
				{
					break;
				}
				m_CellSize = component.GetVector3(m_Effect.BoundsProperty);
				m_GridSize = new int2(Mathf.CeilToInt(m_VisibleSize / m_CellSize.x), Mathf.CeilToInt(m_VisibleSize / m_CellSize.z));
				for (int i = 0; i < m_GridSize.y; i++)
				{
					for (int j = 0; j < m_GridSize.x; j++)
					{
						VisualEffect component2 = Object.Instantiate(m_Effect.VfxPrefab, context.GlobalEffect.transform).GetComponent<VisualEffect>();
						m_VfxInstances.Add(component2);
						component2.Play();
					}
				}
				break;
			}
			}
		}

		private static float CalculateVisibleSize(Camera camera, float distanceFromCamera)
		{
			float num = math.abs(math.acos(math.dot(-camera.transform.forward, new float3(0f, 1f, 0f)))) + math.radians(90f);
			float num2 = math.radians(camera.fieldOfView * 0.5f);
			float x = math.radians(180f) - (num2 + num);
			float num3 = math.radians(180f) - num;
			float num4 = num2;
			float x2 = math.radians(180f) - (num4 + num3);
			float num5 = distanceFromCamera * math.sin(num2) / math.sin(x);
			float num6 = distanceFromCamera * math.sin(num4) / math.sin(x2);
			float num7 = num5 + num6;
			return math.length(new float2(num7 * camera.aspect, num7));
		}

		public void CleanUp()
		{
			if (!m_IsInitialized)
			{
				return;
			}
			foreach (VisualEffect vfxInstance in m_VfxInstances)
			{
				if (vfxInstance != null)
				{
					Object.DestroyImmediate(vfxInstance.gameObject);
				}
			}
			m_VfxInstances.Clear();
			m_IsInitialized = false;
		}

		public void Update(VfxGraphOverride vfxOverride, GlobalEffectContext context)
		{
			if (!m_IsInitialized)
			{
				return;
			}
			UpdatePosition(context);
			foreach (VisualEffect vfxInstance in m_VfxInstances)
			{
				if (vfxInstance != null)
				{
					vfxOverride.Apply(vfxInstance);
				}
			}
		}

		private void UpdatePosition(GlobalEffectContext context)
		{
			if (m_PositionSpace != m_Effect.PositionSpace)
			{
				Reset(context);
			}
			switch (m_PositionSpace)
			{
			case VfxPositionSpace.GlobalEffectSpace:
			{
				foreach (VisualEffect vfxInstance in m_VfxInstances)
				{
					if (vfxInstance != null)
					{
						vfxInstance.transform.localPosition = m_Effect.Position;
					}
				}
				break;
			}
			case VfxPositionSpace.CameraSpace:
				switch (m_Effect.PositionType)
				{
				case PositionType.Single:
					UpdateSinglePosition(context);
					break;
				case PositionType.Grid:
					UpdateGridPosition(context);
					break;
				}
				break;
			}
		}

		private void UpdateSinglePosition(GlobalEffectContext context)
		{
			foreach (VisualEffect vfxInstance in m_VfxInstances)
			{
				if (vfxInstance != null)
				{
					Camera camera = context.Camera;
					if (camera != null)
					{
						vfxInstance.transform.position = camera.transform.position + camera.transform.forward * m_Effect.DistanceFromCamera;
					}
				}
			}
		}

		private static Vector3 GetCenterInHorizontalCrossSectionInDistance(Camera camera, float distanceFromCamera)
		{
			Transform transform = camera.transform;
			Vector3 vector = Vector3.Project(transform.forward, Vector3.down);
			Vector3 normalized = (transform.forward - vector).normalized;
			float num = math.acos(Vector3.Dot(Vector3.down, transform.forward));
			float num2 = math.radians(camera.fieldOfView * 0.5f);
			Vector3 vector2 = normalized * (vector.magnitude * math.tan(num + num2));
			Vector3 vector3 = normalized * (vector.magnitude * math.tan(num - num2));
			Vector3 vector4 = (vector2 + vector3) * 0.5f;
			return transform.position + (vector + vector4) * distanceFromCamera;
		}

		private void UpdateGridPosition(GlobalEffectContext context)
		{
			Vector3 vector = context.Camera.transform.position + context.Camera.transform.forward * m_Effect.DistanceFromCamera;
			Vector3 vector2 = vector - new Vector3(m_VisibleSize / 2f, 0f, m_VisibleSize / 2f);
			int num = Mathf.RoundToInt(vector2.x / m_CellSize.x);
			int num2 = Mathf.RoundToInt(vector2.z / m_CellSize.z);
			for (int i = 0; i < m_GridSize.y; i++)
			{
				for (int j = 0; j < m_GridSize.x; j++)
				{
					int num3 = num + j;
					int num4 = num2 + i;
					int index = i * m_GridSize.x + j;
					m_VfxInstances[index].transform.position = new Vector3((float)num3 * m_CellSize.x, vector.y, (float)num4 * m_CellSize.z);
				}
			}
		}

		private void Reset(GlobalEffectContext context)
		{
			CleanUp();
			Init(context);
		}
	}

	private List<VfxEffectController> m_EffectControllers = new List<VfxEffectController>();

	public VfxController(VfxComponent component)
		: base(component)
	{
	}

	public override void Initialize(GlobalEffectContext context)
	{
		foreach (VfxEffect vfxEffect in base.Component.VfxEffects)
		{
			if (vfxEffect.VfxPrefab != null)
			{
				m_EffectControllers.Add(new VfxEffectController(vfxEffect, context));
			}
		}
	}

	public override void CleanUp()
	{
		foreach (VfxEffectController effectController in m_EffectControllers)
		{
			effectController.CleanUp();
		}
		m_EffectControllers.Clear();
	}

	public override void Update(GlobalEffectContext context)
	{
		if (!(base.VolumeOverride != null) || !(context.Camera.tag == base.Component.CameraTag))
		{
			return;
		}
		foreach (VfxEffectController effectController in m_EffectControllers)
		{
			effectController.Update(base.VolumeOverride, context);
		}
	}
}
