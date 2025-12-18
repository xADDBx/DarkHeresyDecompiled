using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class PantographConfig
{
	public readonly Transform Transform;

	public readonly string Text;

	public readonly List<Sprite> Icons;

	public readonly bool UseLargeView;

	public readonly string TextIcon;

	public readonly MonoBehaviour View;

	public readonly ViewModel ViewModel;

	public PantographConfig(Transform transform, string text, List<Sprite> icons = null, bool useLargeView = false, string textIcon = null)
	{
		Transform = transform;
		Text = text;
		Icons = icons;
		UseLargeView = useLargeView;
		TextIcon = textIcon;
	}

	public PantographConfig(Transform transform, MonoBehaviour itemView, ViewModel itemVM, bool useLargeView = false)
		: this(transform, string.Empty, null, useLargeView)
	{
		View = itemView;
		ViewModel = itemVM;
	}
}
