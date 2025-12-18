using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Movement/AiPositioningCheckThreatsGetter")]
[TypeId("3cfe67de059048659f865510996378a4")]
public class AiPositioningCheckThreatsGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	public enum ThreatType
	{
		AttackOfOpportunity = 1,
		AreaEffect = 2
	}

	public PropertyTargetType Target;

	[EnumFlagsAsDropdown]
	public ThreatType Threats;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"<color='purple'>graph node</color> has {Threats}";
	}

	protected override int GetBaseValue()
	{
		AiBrainHelper.IThreatsInfo threatsData = AiBrainHelper.GetThreatsData((BaseUnitEntity)this.GetTargetByType(Target), AiPositioningData.CurrentNode);
		int num = 0;
		if ((Threats & ThreatType.AttackOfOpportunity) != 0)
		{
			num += threatsData.AreaEffects.Count;
		}
		if ((Threats & ThreatType.AreaEffect) != 0)
		{
			num += threatsData.AreaEffects.Count;
		}
		return num;
	}
}
