using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Obsolete]
[TypeId("7f31f8af9da744a44897d34065a8f677")]
public class BlueprintControllableProjectile : BlueprintScriptableObject
{
	[SerializeField]
	private PrefabLink m_OnCreatureCastPrefab;

	[SerializeField]
	[ValidateNotNull]
	private PrefabLink m_OnCreaturePrefab;

	[SerializeField]
	private float m_HeightOffset;

	[SerializeField]
	private float m_RotationLifetime = 1f;

	[SerializeField]
	private AnimationCurve m_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[AkEventReference]
	[SerializeField]
	private string m_PreparationStartSound;

	[AkEventReference]
	[SerializeField]
	private string m_PreparationEndSound;

	public PrefabLink OnCreatureCastPrefab => m_OnCreatureCastPrefab;

	public PrefabLink OnCreaturePrefab => m_OnCreaturePrefab;

	public float HeightOffset => m_HeightOffset;

	public float RotationLifetime => m_RotationLifetime;

	public AnimationCurve RotationCurve => m_RotationCurve;

	public string PreparationStartSound => m_PreparationStartSound;

	public string PreparationEndSound => m_PreparationEndSound;
}
