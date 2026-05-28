namespace Owlcat.BehaviourTrees;

public class TreeProfilingData
{
	private int m_CurrentTickPassedNodes;

	public int TicksCount { get; private set; }

	public int TicksInRunningNodeCount { get; private set; }

	public double AccumulatedTicksTime { get; private set; }

	public double LastTickTime { get; private set; }

	public double AverageTickTime
	{
		get
		{
			if (TicksCount <= 0)
			{
				return 0.0;
			}
			return AccumulatedTicksTime / (double)TicksCount;
		}
	}

	public int AccumulatedPassedNodes { get; private set; }

	public int LastTickPassedNodes { get; private set; }

	public int AverageTickPassedNodes
	{
		get
		{
			if (TicksCount <= 0)
			{
				return 0;
			}
			return AccumulatedPassedNodes / TicksCount;
		}
	}

	public float InRunningStateProgress
	{
		get
		{
			if (TicksCount <= 0)
			{
				return 0f;
			}
			return (float)TicksInRunningNodeCount * 1f / (float)TicksCount;
		}
	}

	public void AddTickTime(double ms)
	{
		TicksCount++;
		AccumulatedTicksTime += ms;
		LastTickTime = ms;
		LastTickPassedNodes = m_CurrentTickPassedNodes;
		m_CurrentTickPassedNodes = 0;
	}

	public void AddPassedNode()
	{
		AccumulatedPassedNodes++;
		m_CurrentTickPassedNodes++;
	}

	public void AddRunningNodeTick()
	{
		TicksInRunningNodeCount++;
	}
}
