using System;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class UISound
{
	[AkEventReference]
	public string Id;

	public void Play(GameObject gameObject = null)
	{
		if (gameObject == null)
		{
			UISounds.Instance.Play(this);
		}
		else
		{
			UISounds.Instance.Play(this, gameObject);
		}
	}
}
