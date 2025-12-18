using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Settings.UISettings;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintCampaign))]
[AllowMultipleComponents]
[TypeId("5b8c75aac166c2646944dd52e9b7cae9")]
public class BlueprintCampaignOverrideSettingBool : BlueprintCampaignOverrideSetting
{
	public UISettingsEntityBool Bool;

	public bool Value;

	public override void Activate()
	{
		Bool.Setting.OverrideStart(Value);
	}

	public override void Deactivate()
	{
		Bool.Setting.OverrideStop();
	}
}
