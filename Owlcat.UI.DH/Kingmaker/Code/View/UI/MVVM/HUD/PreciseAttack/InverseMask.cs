using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;

public class InverseMask : MonoBehaviour, IMaterialModifier
{
	private static readonly int _stencilComp = Shader.PropertyToID("_StencilComp");

	public Material GetModifiedMaterial(Material baseMaterial)
	{
		Material material = new Material(baseMaterial);
		material.SetFloat(_stencilComp, Convert.ToSingle(CompareFunction.NotEqual));
		return material;
	}
}
