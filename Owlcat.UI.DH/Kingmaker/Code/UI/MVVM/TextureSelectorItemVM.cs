using System;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextureSelectorItemVM : SelectionGroupEntityVM
{
	private Action m_OnSelect;

	private Action m_OnUnselect;

	public readonly int Number;

	private readonly ReactiveProperty<Texture2D> m_Texture = new ReactiveProperty<Texture2D>();

	public ReadOnlyReactiveProperty<Texture2D> Texture => m_Texture;

	public bool IsEmpty { get; private set; }

	public TextureSelectorItemVM(Texture2D value, Action onSelect, int number, bool allowSwitchOff = false, Action onUnselect = null, bool isEmpty = false)
		: base(allowSwitchOff)
	{
		m_Texture.Value = value;
		m_OnSelect = onSelect;
		Number = number;
		IsEmpty = isEmpty;
		if (onUnselect == null)
		{
			return;
		}
		AddDisposable(IsSelected.Subscribe(delegate(bool selected)
		{
			if (!selected)
			{
				onUnselect();
			}
		}));
	}

	public void UpdateTextureAndSetter(Texture2D value, Action setter, bool isEmpty = false)
	{
		m_Texture.Value = value;
		m_OnSelect = setter;
		IsEmpty = isEmpty;
	}

	protected override void DoSelectMe()
	{
		m_OnSelect();
	}

	public void DoFocusMe()
	{
		DoSelectMe();
	}
}
