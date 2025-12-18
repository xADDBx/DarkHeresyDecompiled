using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionActionSettings : InteractionSettings
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset DisplayName;

	[ShowCreator]
	public ConditionsReference Condition;

	[ShowCreator]
	public ActionsReference Actions;
}
