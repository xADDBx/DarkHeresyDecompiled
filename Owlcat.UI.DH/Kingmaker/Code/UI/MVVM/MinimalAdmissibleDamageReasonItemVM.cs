using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MinimalAdmissibleDamageReasonItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite IconSprite;

	public readonly string Text;

	public MinimalAdmissibleDamageReasonItemVM(Sprite iconSprite, string text)
	{
		IconSprite = iconSprite;
		Text = text;
	}

	protected override void DisposeImplementation()
	{
	}
}
