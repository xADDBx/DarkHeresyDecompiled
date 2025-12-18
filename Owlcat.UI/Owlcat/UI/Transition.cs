using System;

namespace Owlcat.UI;

public abstract class Transition : IDisposable
{
	private class NoneTransition : Transition
	{
		public override bool Completed => true;

		public override void Complete()
		{
		}
	}

	private class CombineTransition : Transition
	{
		private readonly Transition[] m_Transitions;

		public override bool Completed => IsCompleted();

		public CombineTransition(params Transition[] transitions)
		{
			m_Transitions = transitions;
		}

		private bool IsCompleted()
		{
			Transition[] transitions = m_Transitions;
			for (int i = 0; i < transitions.Length; i++)
			{
				if (!transitions[i].Completed)
				{
					return false;
				}
			}
			return true;
		}

		public override void Complete()
		{
			Transition[] transitions = m_Transitions;
			foreach (Transition transition in transitions)
			{
				if (!transition.Completed)
				{
					transition.Complete();
				}
			}
		}
	}

	public static readonly Transition None = new NoneTransition();

	public abstract bool Completed { get; }

	public static Transition Combine(params Transition[] transitions)
	{
		return new CombineTransition(transitions);
	}

	public abstract void Complete();

	void IDisposable.Dispose()
	{
		Complete();
	}

	public static Transition operator &(Transition left, Transition right)
	{
		return Combine(left, right);
	}
}
