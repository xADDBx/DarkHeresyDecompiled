using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsConsoleView : CreditsBaseView
{
	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_PlayRolesHint;

	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private readonly ReactiveProperty<bool> m_HasSearchResults = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_InputFieldIsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_ConsoleInputField.Bind(UIStrings.Instance.Credits.EnterSearchNameHere, delegate
		{
			OnFind();
			ReactiveProperty<bool> hasSearchResults = m_HasSearchResults;
			int value;
			if (!string.IsNullOrEmpty(m_SearchField.text))
			{
				LinkedList<PageGenerator.SearchResult> resultSearch = ResultSearch;
				value = ((resultSearch != null && resultSearch.Count > 0) ? 1 : 0);
			}
			else
			{
				value = 0;
			}
			hasSearchResults.Value = (byte)value != 0;
		});
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
	}

	protected override void OnUnbind()
	{
		m_InputFieldIsFocused.Value = false;
		m_HasSearchResults.Value = false;
		base.OnUnbind();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CreditsView"
		});
		m_PrevHint.Bind(m_InputLayer.AddButton(delegate
		{
			ChangeTab(direction: false);
		}, 14)).AddTo(this);
		m_NextHint.Bind(m_InputLayer.AddButton(delegate
		{
			ChangeTab(direction: true);
		}, 15)).AddTo(this);
		m_PlayRolesHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.TogglePause();
		}, 11)).AddTo(this);
		base.ViewModel.Pause.Subscribe(RefreshPlayRolesLabel).AddTo(this);
		RefreshPlayRolesLabel(base.ViewModel.Pause.CurrentValue);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.CloseCredits();
		}, 9, m_InputFieldIsFocused.Not().ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(!m_InputFieldIsFocused.Value);
		}, 9, m_InputFieldIsFocused), UIStrings.Instance.SettingsUI.Cancel).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnPrevPage(delegate
			{
				ChangeTab(direction: false);
			});
		}, 4, InputActionEventType.ButtonRepeating), UIStrings.Instance.Credits.PreviousPage, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 14), UIStrings.Instance.Credits.PreviousGroup, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 15), UIStrings.Instance.Credits.NextGroup, ConsoleHintsWidget.HintPosition.Right).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnNextPage(delegate
			{
				ChangeTab(direction: true);
			});
		}, 5, InputActionEventType.ButtonRepeating), UIStrings.Instance.Credits.NextPage, ConsoleHintsWidget.HintPosition.Right).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnFind();
			ActivateDeactivateInputField(state: false);
		}, 8, base.ViewModel.InputFieldHasAnySymbol), UIStrings.Instance.CommonTexts.Search).AddTo(this);
		string label = UIStrings.Instance.Credits.EnterSearchNameHere.Text.TrimEnd('.');
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(!m_InputFieldIsFocused.Value);
		}, 10, InputActionEventType.ButtonJustReleased), label).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_PlayRolesHint.transform.SetAsFirstSibling();
		}).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void ActivateDeactivateInputField(bool state)
	{
		if (state)
		{
			m_ConsoleInputField.Select();
		}
		else
		{
			m_ConsoleInputField.Abort();
		}
		m_InputFieldIsFocused.Value = state;
	}

	private void RefreshPlayRolesLabel(bool state)
	{
		m_PlayRolesHint.SetLabel(state ? UIStrings.Instance.Credits.PlayRoles : UIStrings.Instance.CommonTexts.Pause);
	}
}
