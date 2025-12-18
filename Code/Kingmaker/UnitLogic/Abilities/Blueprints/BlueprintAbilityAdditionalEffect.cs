using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[Serializable]
[Obsolete]
[TypeId("6c250ac246c7485c93e5db9b58f244dc")]
public class BlueprintAbilityAdditionalEffect : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAbilityAdditionalEffect>
	{
	}

	public ActionList OnHitActions;

	public override bool AllowContextActionsOnly => true;
}
