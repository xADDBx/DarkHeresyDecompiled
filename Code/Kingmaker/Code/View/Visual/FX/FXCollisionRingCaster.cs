using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Kingmaker.Code.View.Visual.FX;

public class FXCollisionRingCaster : MonoBehaviour
{
	[SerializeField]
	private List<Renderer> m_Renderers = new List<Renderer>();

	[SerializeField]
	private float m_CastRadius = 10f;

	[SerializeField]
	private LayerMask m_CastLayers;

	private const string m_TextureNameParam = "_CollisionTexture";

	private const string m_CastCenterNameParam = "_CastCenter";

	private const string m_CastRadiusNameParam = "_CastRadius";

	private const int m_CastSectors = 256;

	private Texture2D m_CollisionTextureCache;

	public List<Renderer> Renderers => m_Renderers;

	public void CreateAndAssignTexture(bool debug = false)
	{
		if (m_CollisionTextureCache == null)
		{
			m_CollisionTextureCache = new Texture2D(256, 1, GraphicsFormat.R8_UNorm, TextureCreationFlags.None);
		}
		Transform castCenter = base.transform;
		ref float castRadius = ref m_CastRadius;
		int castSectors = 256;
		ref LayerMask castLayers = ref m_CastLayers;
		ref Texture2D collisionTextureCache = ref m_CollisionTextureCache;
		float coneAngle = 360f;
		float centerAngle = 0f;
		CreateRaycastTexture(in castCenter, in castRadius, in castSectors, in castLayers, ref collisionTextureCache, in coneAngle, in centerAngle, debug);
		foreach (Renderer renderer in m_Renderers)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetTexture("_CollisionTexture", m_CollisionTextureCache);
			materialPropertyBlock.SetFloat("_CastRadius", m_CastRadius);
			materialPropertyBlock.SetVector("_CastCenter", new Vector2(base.transform.position.x, base.transform.position.z));
			renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	private static void CreateRaycastTexture(in Transform castCenter, in float castRadius, in int castSectors, in LayerMask castLayers, ref Texture2D texture, in float coneAngle, in float centerAngle, bool debug = false)
	{
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(castSectors, Allocator.TempJob);
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(castSectors, Allocator.TempJob);
		float num = coneAngle / (float)castSectors;
		QueryParameters queryParameters = new QueryParameters(castLayers);
		float num2 = centerAngle - coneAngle / 2f;
		for (int i = 0; i < castSectors; i++)
		{
			float f = (num2 + (float)i * num) * (MathF.PI / 180f);
			float num3 = Mathf.Sin(f);
			float num4 = Mathf.Cos(f);
			Vector3 normalized = (Vector3.right * num3 + Vector3.forward * num4).normalized;
			if (debug)
			{
				Debug.DrawLine(castCenter.position, castCenter.position + normalized * castRadius, Color.red, 1f);
			}
			commands[i] = new RaycastCommand(castCenter.position, normalized, queryParameters, castRadius);
		}
		if (debug)
		{
			float f2 = centerAngle * (MathF.PI / 180f);
			float num5 = Mathf.Sin(f2);
			float num6 = Mathf.Cos(f2);
			Vector3 normalized2 = (Vector3.right * num5 + Vector3.forward * num6).normalized;
			Debug.DrawLine(castCenter.position, castCenter.position + normalized2 * castRadius, Color.green, 1f);
		}
		RaycastCommand.ScheduleBatch(commands, results, 1, 1).Complete();
		for (int j = 0; j < results.Length; j++)
		{
			RaycastHit raycastHit = results[j];
			float num7 = 1f;
			if (raycastHit.collider != null)
			{
				num7 = raycastHit.distance / castRadius;
			}
			Color color = new Color(num7, num7, num7);
			texture.SetPixel(j, 0, color);
		}
		texture.Apply();
		results.Dispose();
		commands.Dispose();
	}
}
