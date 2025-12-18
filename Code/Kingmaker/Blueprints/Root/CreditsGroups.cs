using System;
using System.Collections.Generic;
using Kingmaker.ResourceLinks;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CreditsGroups
{
	public List<BlueprintCreditsGroupReference> Groups = new List<BlueprintCreditsGroupReference>();

	public List<BlueprintCreditsGroupReference> EndTitlesGroups = new List<BlueprintCreditsGroupReference>();

	public List<SpriteLink> BackgroundSprites = new List<SpriteLink>();
}
