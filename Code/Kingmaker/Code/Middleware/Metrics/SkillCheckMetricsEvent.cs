using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Middleware.Metrics;

public class SkillCheckMetricsEvent : MetricsEvent
{
	protected override string Name => "skill_check";

	public SkillCheckMetricsEvent Type(SkillCheckType type)
	{
		AddParam("type", type switch
		{
			SkillCheckType.Default => "default", 
			SkillCheckType.CritSave => "crit_save", 
			SkillCheckType.Inspect => "inspect", 
			SkillCheckType.DOT => "dot", 
			_ => MetricsUtils.EnumToSnakeCase(type), 
		});
		return this;
	}

	public SkillCheckMetricsEvent Initiator(string initiator)
	{
		AddParam("initiator", initiator);
		return this;
	}

	public SkillCheckMetricsEvent Target(string target)
	{
		AddParam("target", target);
		return this;
	}

	public SkillCheckMetricsEvent Result(bool result)
	{
		AddParam("result", result.ToString());
		return this;
	}
}
