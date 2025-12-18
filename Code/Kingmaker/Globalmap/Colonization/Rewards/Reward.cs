using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("e9b07d76cb804c09b369deacfa0d1f40")]
[AllowedOn(typeof(BlueprintColonyChronicle))]
[AllowedOn(typeof(BlueprintColonyEventResult))]
public abstract class Reward : BlueprintComponent
{
	public bool HideInUI;
}
