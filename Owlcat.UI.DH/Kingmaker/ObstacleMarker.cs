using System.Collections;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker;

public class ObstacleMarker : MonoBehaviour, IDetectClicks
{
	[HideInInspector]
	public LosCalculations.CoverType Type;

	[HideInInspector]
	public GridObstacle OwnerObstacle;

	[HideInInspector]
	public Renderer ObstacleRenderer;

	[HideInInspector]
	public Collider RaycastCollider;

	private float m_Height;

	private float m_HalfWidth;

	private Mesh m_OwnedMesh;

	private MaterialPropertyBlock m_PropertyBlock;

	private Coroutine m_FadeCoroutine;

	private float m_CurrentAlpha;

	private bool m_IsEnabled;

	private static readonly int s_AlphaScaleProperty = Shader.PropertyToID("_AlphaScale");

	public GameObject Target
	{
		get
		{
			if (!((Object)(object)OwnerObstacle != null))
			{
				return base.gameObject;
			}
			return ((Component)(object)OwnerObstacle).gameObject;
		}
	}

	public Vector3 LeftEndpoint { get; private set; }

	public Vector3 RightEndpoint { get; private set; }

	public float BaseY => base.transform.position.y;

	public float TopY => BaseY + m_Height;

	public void Initialize(MeshFilter meshFilter, Renderer renderer, Collider collider, Vector3 leftEndpoint, Vector3 rightEndpoint, float halfWidth, float height, LosCalculations.CoverType type)
	{
		Type = type;
		ObstacleRenderer = renderer;
		RaycastCollider = collider;
		LeftEndpoint = leftEndpoint;
		RightEndpoint = rightEndpoint;
		m_HalfWidth = halfWidth;
		m_Height = height;
		m_PropertyBlock = new MaterialPropertyBlock();
		m_OwnedMesh = BuildInitialMesh(halfWidth, height);
		meshFilter.sharedMesh = m_OwnedMesh;
		SetAlpha(0f);
		if ((bool)ObstacleRenderer)
		{
			ObstacleRenderer.enabled = false;
		}
	}

	public void UpdateEndpoints(Quaternion rotation)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = rotation * Vector3.right;
		LeftEndpoint = position - vector * m_HalfWidth;
		RightEndpoint = position + vector * m_HalfWidth;
	}

	public void SetCornerYs(float leftBottomYWorld, float leftTopYWorld, float rightBottomYWorld, float rightTopYWorld)
	{
		if (!(m_OwnedMesh == null))
		{
			float y = base.transform.position.y;
			Vector3[] vertices = m_OwnedMesh.vertices;
			vertices[0] = new Vector3(0f - m_HalfWidth, leftBottomYWorld - y, 0f);
			vertices[1] = new Vector3(0f - m_HalfWidth, leftTopYWorld - y, 0f);
			vertices[2] = new Vector3(m_HalfWidth, rightBottomYWorld - y, 0f);
			vertices[3] = new Vector3(m_HalfWidth, rightTopYWorld - y, 0f);
			m_OwnedMesh.vertices = vertices;
			m_OwnedMesh.RecalculateNormals();
			m_OwnedMesh.RecalculateBounds();
		}
	}

	private static Mesh BuildInitialMesh(float halfWidth, float height)
	{
		Mesh mesh = new Mesh
		{
			name = "ObstacleMarker_Quad"
		};
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - halfWidth, 0f, 0f),
			new Vector3(0f - halfWidth, height, 0f),
			new Vector3(halfWidth, 0f, 0f),
			new Vector3(halfWidth, height, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	public void EnableMarker(bool enable, float fadeSpeed = 10f)
	{
		if (base.gameObject.activeSelf && m_IsEnabled != enable)
		{
			m_IsEnabled = enable;
			if (m_FadeCoroutine != null)
			{
				StopCoroutine(m_FadeCoroutine);
			}
			m_FadeCoroutine = StartCoroutine(FadeToAlpha(enable ? 1f : 0f, fadeSpeed));
		}
	}

	private IEnumerator FadeToAlpha(float targetAlpha, float fadeSpeed)
	{
		while (!Mathf.Approximately(m_CurrentAlpha, targetAlpha))
		{
			m_CurrentAlpha = Mathf.MoveTowards(m_CurrentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
			SetAlpha(m_CurrentAlpha);
			yield return null;
		}
		m_CurrentAlpha = targetAlpha;
		SetAlpha(m_CurrentAlpha);
		m_FadeCoroutine = null;
	}

	private void SetAlpha(float alpha)
	{
		if ((bool)ObstacleRenderer)
		{
			ObstacleRenderer.enabled = alpha > 0.01f;
			ObstacleRenderer.GetPropertyBlock(m_PropertyBlock);
			m_PropertyBlock.SetFloat(s_AlphaScaleProperty, alpha);
			ObstacleRenderer.SetPropertyBlock(m_PropertyBlock);
		}
		if ((bool)RaycastCollider)
		{
			RaycastCollider.enabled = alpha > 0.5f;
		}
	}

	private void OnDestroy()
	{
		if (m_OwnedMesh != null)
		{
			Object.Destroy(m_OwnedMesh);
		}
	}

	void IDetectClicks.HandleClick()
	{
	}
}
