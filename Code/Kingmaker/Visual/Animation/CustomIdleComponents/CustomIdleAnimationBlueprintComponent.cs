using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.CustomIdleComponents;

[ComponentName("Animation/CustomIdleAnimationBlueprintComponent")]
[TypeId("6e5ad1895b5bedc47bcc0ebc1a615edd")]
public class CustomIdleAnimationBlueprintComponent : BlueprintComponent
{
	public List<AnimationClipWrapper> IdleClips;
}
