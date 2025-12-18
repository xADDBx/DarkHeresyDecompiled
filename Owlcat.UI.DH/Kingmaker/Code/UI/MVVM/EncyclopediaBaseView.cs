using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaBaseView : View<EncyclopediaVM>, IEncyclopediaGlossaryModeHandler, ISubscriber
{
	[SerializeField]
	[UsedImplicitly]
	protected EncyclopediaNavigationBaseView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	protected EncyclopediaPageBaseView m_Page;

	[SerializeField]
	protected List<RandomPickerBase> m_RandomPickers;

	private bool m_IsShowed;

	protected override void OnBind()
	{
		m_Page.Initialize();
		base.OnBind();
		Show();
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		base.ViewModel.Page.Subscribe(BindPage).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			base.gameObject.SetActive(value: true);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Encyclopedia);
			});
		}
	}

	private void Hide()
	{
		if (m_IsShowed)
		{
			m_IsShowed = false;
			base.gameObject.SetActive(value: false);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Encyclopedia);
			});
		}
	}

	private void BindPage(EncyclopediaPageVM pageVM)
	{
		m_Page.Bind(pageVM);
		if (pageVM == null || pageVM.BlockVMs.Empty())
		{
			foreach (RandomPickerBase randomPicker in m_RandomPickers)
			{
				randomPicker.Reset();
			}
			return;
		}
		foreach (RandomPickerBase randomPicker2 in m_RandomPickers)
		{
			randomPicker2.Randomize(pageVM.Title);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Hide();
	}

	public void HandleGlossaryMode(bool state)
	{
		if (!state)
		{
			OnCloseGlossaryMode();
		}
	}

	protected virtual void OnCloseGlossaryMode()
	{
	}
}
