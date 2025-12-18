using System.Collections.Generic;
using Kingmaker;
using UnityEngine;

namespace Owlcat.Runtime.Visual.SceneHelpers;

public class StaticPrefab : MonoBehaviour
{
	private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

	private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");

	private static readonly int MasksMap = Shader.PropertyToID("_MasksMap");

	public GameObject MechanicsRoot;

	public GameObject CollidersRoot;

	public GameObject FxRoot;

	public GameObject LightsRoot;

	public GameObject VisualRoot;

	public List<SurfaceHitObject> SurfaceHitObjects = new List<SurfaceHitObject>();
}
