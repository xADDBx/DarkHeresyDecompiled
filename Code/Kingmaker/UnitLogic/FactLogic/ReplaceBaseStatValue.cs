using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete("Use OverrideSkillBaseStat instead")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("547decea8a90414eb6dedb6ac9053ec9")]
public class ReplaceBaseStatValue : ReplaceStat
{
	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}
}
