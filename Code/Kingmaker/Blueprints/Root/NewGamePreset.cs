using System;
using Kingmaker.Localization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class NewGamePreset
{
	public BlueprintAreaPresetReference GamePreset;

	public LocalizedString DisplayName;

	public LocalizedString Description;

	[FormerlySerializedAs("PreviewSprite")]
	public Sprite Picture;
}
