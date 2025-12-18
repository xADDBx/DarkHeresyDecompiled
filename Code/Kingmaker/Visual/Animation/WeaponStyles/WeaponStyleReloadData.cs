using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleReloadData
{
	public AnimationClipWrapper Main;

	[Header("Fake")]
	public AnimationClipWrapper Off;

	public AnimationClipWrapper All;
}
