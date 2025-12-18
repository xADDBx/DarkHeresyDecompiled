using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterConsoleView : ItemsFilterBaseView, ICullFocusHandler, ISubscriber
{
	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	[SerializeField]
	private ConsoleHint m_SortingHint;

	[SerializeField]
	private ConsoleHint m_ToggleShowUnavailableHint;

	private CompositeDisposable m_DropdownDisposables = new CompositeDisposable();

	private IConsoleEntity m_CulledEntity;

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

	public void AddInput(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enabledHints = null, ConsoleHintsWidget temp = null)
	{
		foreach (IDisposable item in AddInputDisposable(inputLayer, enabledHints))
		{
			item.AddTo(this);
		}
	}

	public IEnumerable<IDisposable> AddInputDisposable(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(base.OnPrevious, 14, enabledHints);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(base.OnNext, 15, enabledHints);
		List<IDisposable> list = new List<IDisposable>
		{
			inputBindStruct,
			m_PreviousFilterHint.Bind(inputBindStruct),
			inputBindStruct2,
			m_NextFilterHint.Bind(inputBindStruct2)
		};
		if ((bool)m_SortingHint)
		{
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(ShowSortingMenu, 17, enabledHints, InputActionEventType.ButtonJustReleased);
			list.Add(m_SortingHint.Bind(inputBindStruct3));
			list.Add(inputBindStruct3);
		}
		if ((bool)m_ToggleShowUnavailableHint && m_ShowToggle)
		{
			m_SorterDropdown.IsOn.Subscribe(delegate(bool value)
			{
				if (value)
				{
					ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(0, UnityFrameProvider.PostLateUpdate), delegate
					{
						m_DropdownDisposables.Clear();
						InputBindStruct inputBindStruct4 = m_SorterDropdown.InputLayer.AddButton(delegate
						{
							ToggleShowItems();
						}, 11);
						m_DropdownDisposables.Add(m_ToggleShowUnavailableHint.Bind(inputBindStruct4));
						m_DropdownDisposables.Add(inputBindStruct4);
					}).AddTo(this);
				}
			}).AddTo(this);
		}
		list.Add((m_SearchView as ItemsFilterSearchConsoleView)?.AddInputDisposable(inputLayer, enabledHints));
		return list;
	}

	public void GetNextFilter(InputActionEventData data)
	{
		OnNext(data);
	}

	public void GetPrevFilter(InputActionEventData data)
	{
		OnPrevious(data);
	}

	private void ShowSortingMenu(InputActionEventData data)
	{
		ItemsFilterSearchConsoleView obj = m_SearchView as ItemsFilterSearchConsoleView;
		if ((object)obj == null || !obj.IsActive)
		{
			TooltipHelper.HideTooltip();
			m_SorterDropdown.SetState(value: true);
		}
	}

	public void HandleRemoveFocus()
	{
		GridConsoleNavigationBehaviour navigationBehaviour = m_SorterDropdown.GetNavigationBehaviour();
		if (navigationBehaviour != null)
		{
			m_CulledEntity = navigationBehaviour.DeepestNestedFocus;
			navigationBehaviour.UnFocusCurrentEntity();
		}
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledEntity != null)
		{
			m_SorterDropdown.GetNavigationBehaviour()?.FocusOnEntityManual(m_CulledEntity);
		}
		m_CulledEntity = null;
	}

	private void ToggleShowItems()
	{
		m_ToggleUnequippable.Set(!m_ToggleUnequippable.IsOn.CurrentValue);
	}
}
