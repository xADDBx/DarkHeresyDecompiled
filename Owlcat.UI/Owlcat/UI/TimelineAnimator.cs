using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Owlcat.UI;

[Serializable]
public struct TimelineAnimator
{
	[SerializeField]
	private PlayableDirector m_Director;

	public readonly Transition Play()
	{
		return new TimelineTransition(m_Director);
	}
}
