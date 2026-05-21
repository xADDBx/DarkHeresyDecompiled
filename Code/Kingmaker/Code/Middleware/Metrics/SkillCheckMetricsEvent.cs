namespace Kingmaker.Code.Middleware.Metrics;

public class SkillCheckMetricsEvent : MetricsEvent
{
	public enum Types
	{
		Trap,
		Hidden,
		Interact,
		Inspect,
		ShowAnswer,
		DialogCue,
		Identify,
		Awareness,
		RollAction,
		PickLock
	}

	protected override string Name => "skill_check";

	public SkillCheckMetricsEvent Type(Types type)
	{
		AddParam("type", type switch
		{
			Types.Trap => "trap", 
			Types.Hidden => "hidden", 
			Types.Interact => "inspect", 
			Types.Inspect => "inspect", 
			Types.ShowAnswer => "show_answer", 
			Types.DialogCue => "dialog_cue", 
			Types.Identify => "identify", 
			Types.Awareness => "awareness", 
			Types.RollAction => "roll_action", 
			Types.PickLock => "pick_lock", 
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
