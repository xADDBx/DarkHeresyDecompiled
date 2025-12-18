using UnityEngine.Playables;

namespace Owlcat.UI;

public class TimelineTransition : Transition
{
	private readonly PlayableDirector m_Director;

	public override bool Completed
	{
		get
		{
			if (!(m_Director == null))
			{
				return m_Director.state == PlayState.Paused;
			}
			return true;
		}
	}

	public TimelineTransition(PlayableDirector director)
	{
		m_Director = director;
		m_Director.time = m_Director.initialTime;
		m_Director.Play();
	}

	public override void Complete()
	{
		if (!Completed)
		{
			m_Director.time = m_Director.duration;
			m_Director.Evaluate();
		}
	}
}
