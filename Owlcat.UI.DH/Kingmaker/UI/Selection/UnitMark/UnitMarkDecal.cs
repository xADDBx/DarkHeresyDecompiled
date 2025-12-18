using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Selection.UnitMark;

[Serializable]
public class UnitMarkDecal
{
	public GameObject GameObject;

	public MeshRenderer DecalMeshRenderer;

	public Material MaterialSizeStandard;

	[FormerlySerializedAs("MaterailSizeBig")]
	public Material MaterialSizeBig;

	public void SetActive(bool state)
	{
		GameObject.SetActive(state);
	}

	public void SetBigSize(bool isBig)
	{
		DecalMeshRenderer.material = (isBig ? MaterialSizeBig : MaterialSizeStandard);
	}

	public void SetMaterial(Material material)
	{
		DecalMeshRenderer.material = material;
	}
}
