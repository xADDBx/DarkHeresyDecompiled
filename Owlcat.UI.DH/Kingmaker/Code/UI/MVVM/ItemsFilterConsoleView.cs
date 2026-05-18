using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterConsoleView : ItemsFilterBaseView
{
	[Header("Console Input")]
	[SerializeField]
	private HintView m_PreviousFilterHint;

	[SerializeField]
	private HintView m_NextFilterHint;

	[SerializeField]
	private HintView m_SortingHint;

	[SerializeField]
	private HintView m_ToggleShowUnavailableHint;

	private CompositeDisposable m_DropdownDisposables = new CompositeDisposable();

	public override void Initialize()
	{
		base.Initialize();
		if (!BuildModeUtility.Data.CloudSwitchSettings)
		{
			m_SearchView.Or(null)?.Initialize();
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_SorterDropdown.IsOn.Skip(1).Subscribe(UpdateFocus).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_DropdownDisposables.Clear();
	}

	private void UpdateFocus(bool value)
	{
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			if (value)
			{
				h.HandleRemoveFocus();
			}
			else
			{
				h.HandleRestoreFocus();
			}
		});
	}

	public void AddInput()
	{
	}

	public IEnumerable<IDisposable> AddInputDisposable()
	{
		return new CompositeDisposable();
	}

	public void GetNextFilter()
	{
		OnNext();
	}

	public void GetPrevFilter()
	{
		OnPrevious();
	}

	private void ShowSortingMenu()
	{
		ItemsFilterSearchConsoleView obj = m_SearchView as ItemsFilterSearchConsoleView;
		if ((object)obj == null || !obj.IsActive)
		{
			TooltipHelper.HideTooltip();
			m_SorterDropdown.SetState(value: true);
		}
	}
}
