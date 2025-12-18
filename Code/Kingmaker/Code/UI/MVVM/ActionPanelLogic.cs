using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.UI.MVVM;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[TypeId("fd79ed0bbb4760145b3b2839502cd2c3")]
public class ActionPanelLogic : BlueprintComponent
{
	public int Priority;
}
