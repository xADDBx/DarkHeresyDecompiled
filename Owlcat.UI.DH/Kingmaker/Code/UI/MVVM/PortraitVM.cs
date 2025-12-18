using Kingmaker.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PortraitVM : ViewModel
{
	public enum PortraitSize
	{
		Small,
		Middle,
		Full
	}

	public readonly PortraitData PortraitData;

	public Sprite PortraitSmall => PortraitData?.SmallPortrait;

	public Sprite PortraitHalf => PortraitData?.HalfLengthPortrait;

	public Sprite PortraitFull => PortraitData?.FullLengthPortrait;

	public PortraitVM(PortraitData portraitData)
	{
		PortraitData = portraitData;
		portraitData?.EnsureImages();
	}
}
