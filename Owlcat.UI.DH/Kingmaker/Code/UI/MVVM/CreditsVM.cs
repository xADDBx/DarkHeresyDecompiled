using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.Common;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsVM : ViewModel
{
	public readonly List<BlueprintCreditsGroup> Groups;

	private readonly ReactiveProperty<bool> m_Pause = new ReactiveProperty<bool>(value: false);

	public readonly SelectionGroupRadioVM<CreditsMenuEntityVM> SelectionGroup;

	public readonly LensSelectorVM Selector;

	private readonly ReactiveCommand<BlueprintCreditsGroup> m_OnSelectGroup = new ReactiveCommand<BlueprintCreditsGroup>();

	private readonly List<CreditsMenuEntityVM> m_MenuEntitiesList;

	private readonly ReactiveProperty<CreditsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<CreditsMenuEntityVM>();

	private readonly Action m_CloseAction;

	private readonly ReactiveProperty<bool> m_InputFieldHasAnySymbol = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> Pause => m_Pause;

	public Observable<BlueprintCreditsGroup> OnSelectGroup => m_OnSelectGroup;

	public ReadOnlyReactiveProperty<bool> InputFieldHasAnySymbol => m_InputFieldHasAnySymbol;

	public int SelectedMenuIndex => m_MenuEntitiesList.IndexOf(m_SelectedMenuEntity.Value);

	public CreditsVM(Action closeAction, bool onlyBakers = false)
	{
		m_CloseAction = closeAction;
		Groups = ConfigRoot.Instance.UIConfig.Credits.Groups.Select((BlueprintCreditsGroupReference g) => g.Get()).ToList();
		if (onlyBakers)
		{
			Groups = Groups.Where((BlueprintCreditsGroup g) => g.IsBakers && g.ShowInGameCredits).ToList();
		}
		else
		{
			Groups = Groups.Where((BlueprintCreditsGroup g) => !g.IsBakers || g.ShowInMainMenuCredits).ToList();
		}
		m_MenuEntitiesList = Groups.Select((BlueprintCreditsGroup g) => new CreditsMenuEntityVM(g.PageIcon, g.HeaderText, delegate
		{
			m_OnSelectGroup?.Execute(g);
		})).ToList();
		SelectionGroup = new SelectionGroupRadioVM<CreditsMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
	}

	protected override void OnDispose()
	{
		m_OnSelectGroup.Dispose();
		m_SelectedMenuEntity.Dispose();
		Groups.Clear();
		m_MenuEntitiesList.Clear();
	}

	public void SetSelectedGroup(BlueprintCreditsGroup group)
	{
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.ElementAtOrDefault(Groups.IndexOf(group));
	}

	public void SetPauseState(bool state)
	{
		m_Pause.Value = state;
	}

	public void TogglePause()
	{
		m_Pause.Value = !Pause.CurrentValue;
	}

	public void CloseCredits()
	{
		m_CloseAction?.Invoke();
	}

	public void CheckInputFieldAnySymbols(string str)
	{
		m_InputFieldHasAnySymbol.Value = !string.IsNullOrWhiteSpace(str);
	}

	public void SetPause(bool isPause)
	{
		m_Pause.Value = isPause;
	}
}
