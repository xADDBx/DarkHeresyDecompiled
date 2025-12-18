using System;
using Kingmaker.Visual.DayNightCycle;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("30897cdea22c475494ff0e14d456730e")]
public class EtudeBracketOverrideLightConfig : EtudeBracketTrigger
{
	[ValidateNotNull]
	public SceneLightConfig.Link LightConfig;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		LightController.Active.OverrideConfig(LightConfig.Load());
	}

	protected override void OnExit()
	{
		LightController.Active.OverrideConfig(null);
	}

	protected override void OnResume()
	{
		OnEnter();
	}
}
