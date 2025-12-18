using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

[TypeId("d1d0e6baf89343708cb07802cf9942c6")]
public class DetectiveAsksSettings : BlueprintScriptableObject
{
	public class DetectiveAsksSettingsReference : BlueprintReference<DetectiveAsksSettings>
	{
		public DetectiveAsksSettingsReference()
		{
			guid = "c32b03b3cfd54d08bdbd7dcedfd582d0";
		}
	}

	private static readonly DetectiveAsksSettingsReference s_Instance = new DetectiveAsksSettingsReference();

	public List<BpRef<DetectiveAskTriggerCondition>> ReconstructionReadyAskConditions = new List<BpRef<DetectiveAskTriggerCondition>>();

	public static DetectiveAsksSettings Instance => s_Instance;
}
