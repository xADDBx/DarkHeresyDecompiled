using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsConsoleView : CreditsBaseView
{
	[Header("Console Input")]
	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[SerializeField]
	private HintView m_PlayRolesHint;

	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

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
	}

	protected override void OnUnbind()
	{
		m_InputFieldIsFocused.Value = false;
		m_HasSearchResults.Value = false;
		base.OnUnbind();
	}

	private void CreateInput()
	{
	}
}
