using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuEntityVM : ViewModel
{
	private readonly IContextMenuCollectionEntity m_Entity;

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SubTitle = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>(value: true);

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<string> SubTitle => m_SubTitle;

	public bool IsHeader => m_Entity?.IsHeader() ?? false;

	public Sprite Sprite { get; }

	public ReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	public bool IsInteractable => m_Entity?.IsInteractable() ?? false;

	public bool IsSeparator => m_Entity?.IsEmpty() ?? false;

	public ContextMenuEntityVM(IContextMenuCollectionEntity entity)
	{
		m_Entity = entity;
		UpdateTitle();
		RefreshEnabling();
		Sprite = entity?.GetIcon();
	}

	public void UpdateTitle()
	{
		m_SubTitle.Value = m_Entity?.GetSubTitle();
		LocalizedString localizedString = m_Entity?.GetTitleLocalized();
		string text = m_Entity?.GetTitle();
		if (localizedString != null)
		{
			m_Title.Value = localizedString.Text;
		}
		else if (text != null)
		{
			m_Title.Value = text;
		}
	}

	public void RefreshEnabling()
	{
		m_IsEnabled.Value = m_Entity.IsEnabled();
	}

	public void Execute()
	{
		m_Entity?.Execute();
	}

	public ButtonSoundsEnum GetClickSoundType()
	{
		return m_Entity.GetClickSoundType();
	}

	public ButtonSoundsEnum GetHoverSoundType()
	{
		return m_Entity.GetHoverSoundType();
	}
}
