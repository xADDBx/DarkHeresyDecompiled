namespace Owlcat.BehaviourTrees;

public static class BehaviourTreeConfig
{
	public static class Features
	{
		public static bool LogicalBranchesEnabled { get; set; }

		public static bool ConditionAbortEnabled { get; set; }

		public static bool ProfilerEnabled { get; set; }
	}

	public static class LogicalBranches
	{
		public static float DisabledTransparency { get; set; } = 0.2f;


		public static bool HasResetCooldown { get; set; } = false;


		public static float ResetCooldown { get; set; } = 10f;

	}

	public static IBehaviourTreeTimeProvider TimeProvider
	{
		set
		{
			BehaviourTreeTimeProvider.SetProvider(value);
		}
	}

	public static IBehaviourTreeRandomProvider RandomProvider
	{
		set
		{
			BehaviourTreeRandomProvider.SetProvider(value);
		}
	}

	public static IBehaviourTreeLogger Logger
	{
		set
		{
			BTLog.SetLogger(value);
		}
	}

	public static IBehaviourTreeNameGenerator NameGenerator
	{
		set
		{
			BehaviourTreeNameGenerator.SetGenerator(value);
		}
	}

	public static BreakpointHandlingType BreakpointHandlingType { get; set; }
}
