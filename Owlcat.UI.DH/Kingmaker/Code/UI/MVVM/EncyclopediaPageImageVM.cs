using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageImageVM : ViewModel
{
	public readonly Sprite Image;

	private readonly Action<EncyclopediaPageImageVM> m_ZoomAction;

	public bool IsZoomAllowed => m_ZoomAction != null;

	public EncyclopediaPageImageVM(Sprite image, Action<EncyclopediaPageImageVM> zoomAction = null)
	{
		Image = image;
		m_ZoomAction = zoomAction;
	}

	public void HandleZoomClick()
	{
		m_ZoomAction?.Invoke(this);
	}
}
