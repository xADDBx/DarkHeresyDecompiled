using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.UI.Tweenable;

[Serializable]
public class TweenBehavior
{
	[SerializeReference]
	[Tween]
	[UsedImplicitly]
	private List<ITweenable> m_Tweenables;

	public void Play()
	{
		m_Tweenables.ForEach(delegate(ITweenable tweenable)
		{
			tweenable?.Play();
		});
	}
}
