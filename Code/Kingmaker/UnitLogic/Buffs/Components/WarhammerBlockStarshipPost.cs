using System;
using Kingmaker.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("7794392710080e741beb48abedb3deb2")]
public class WarhammerBlockStarshipPost : BlueprintComponent
{
	public bool BlockRandom;

	[HideIf("BlockRandom")]
	public PostType Post;
}
