using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Blueprints.Root;

[Serializable]
[ComponentName("Root/BlueprintCoverRoot")]
[TypeId("3f5e955e42b44b37abb1b3d92c4a2356")]
public sealed class BlueprintCoverRoot : BlueprintScriptableObject
{
	[Header("Gizmos")]
	[ValidateNotNull]
	public Material ObstacleMaterial;

	[ValidateNotNull]
	public Material InactiveObstacleMaterial;

	[ValidateNotNull]
	public Material CoverMaterial;

	[ValidateNotNull]
	public Material InactiveCoverMaterial;

	[ValidateNotNull]
	public Material InvisibleCoverMaterial;

	[ValidateNotNull]
	public Material LosBlockerMaterial;

	[ValidateNotNull]
	public Material InactiveLosBlockerMaterial;

	[ValidateNotNull]
	public Material InvisibleLosBlockerMaterial;
}
