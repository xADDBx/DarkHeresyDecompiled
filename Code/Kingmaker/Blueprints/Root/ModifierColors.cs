using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class ModifierColors
{
	[Serializable]
	public class ModifierBlueprintColorPair
	{
		public BpRef<BlueprintAbilityTag> AbilityTag;

		public Color Color;
	}

	[SerializeField]
	private List<ModifierBlueprintColorPair> m_TagColors = new List<ModifierBlueprintColorPair>();

	[field: SerializeField]
	public Sprite DefaultSprite { get; private set; }

	public Color GetModifierColor(BlueprintScriptableObject blueprint)
	{
		BlueprintAbilityModifier modifier = blueprint as BlueprintAbilityModifier;
		if (blueprint is BlueprintFeature blueprint2 && blueprint2.TryGetComponent<AddAvailableAbilityModifier>(out var component))
		{
			modifier = component.Modifier;
		}
		if (modifier == null || modifier.Tags.Empty())
		{
			return Color.white;
		}
		return Enumerable.FirstOrDefault(m_TagColors, (ModifierBlueprintColorPair p) => modifier.Match(p.AbilityTag))?.Color ?? Color.white;
	}
}
