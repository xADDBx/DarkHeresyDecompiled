namespace Kingmaker.Visual.Sound;

public class AskSchedulingEntry
{
	public AskWrapper Wrapper;

	public bool Is2D;

	public AskCallback Callback;

	public AsksContext AsksContext;

	public int Priority => Wrapper.AsksSet.Priority;

	public bool IsInterruptCurrent => Wrapper.AsksSet.InterruptCurrent;

	public bool IsClearsQueue
	{
		get
		{
			if (IsInterruptCurrent)
			{
				return Wrapper.AsksSet.ClearQueue;
			}
			return false;
		}
	}

	public bool IsForbidQueueing => Wrapper.AsksSet.IsForbidQueueing;

	public bool CannotBePlayedIfQueueNotEmpty => Wrapper.AsksSet.CannotBePlayedIfQueueNotEmpty;

	public AskSchedulingEntry(AskWrapper wrapper, bool is2D, AskCallback callback = null, AsksContext asksContext = null)
	{
		Wrapper = wrapper;
		Is2D = is2D;
		Callback = callback;
		AsksContext = asksContext;
	}
}
