namespace Owlcat.BehaviourTrees;

public class NodeProfilingData
{
	public int ExecutionCount { get; set; }

	public double AccumulatedTime { get; set; }

	public double LastTime { get; set; }

	public double MaxTime { get; set; }

	public int MaxTimeExecutionTick { get; set; }

	public double AverageTime
	{
		get
		{
			if (ExecutionCount <= 0)
			{
				return 0.0;
			}
			return AccumulatedTime / (double)ExecutionCount;
		}
	}

	public void AddTime(double ms)
	{
		ExecutionCount++;
		AccumulatedTime += ms;
		LastTime = ms;
		if (ms > MaxTime)
		{
			MaxTime = ms;
			MaxTimeExecutionTick = ExecutionCount;
		}
	}
}
