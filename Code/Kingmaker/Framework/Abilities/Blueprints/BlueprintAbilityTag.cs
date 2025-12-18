using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Framework.Abilities.Blueprints;

[Serializable]
[TypeId("1748564ff6a84d11a6a2384acc01a889")]
[OwlPackable(OwlPackableMode.NoGenerate)]
[ComponentName("Ability/BlueprintAbilityTag")]
public sealed class BlueprintAbilityTag : BlueprintScriptableObject
{
	public LocalizedString Name;

	public LocalizedString Description;

	public Sprite Icon;

	public Sprite AbilityIcon;
}
