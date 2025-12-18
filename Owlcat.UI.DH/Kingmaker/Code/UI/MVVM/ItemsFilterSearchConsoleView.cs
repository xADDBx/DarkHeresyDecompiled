using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterSearchConsoleView : ItemsFilterSearchBaseView
{
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHint m_SearchHint;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	public bool IsActive => m_IsActive.Value;

	protected override void OnBind()
	{
		base.OnBind();
		m_ConsoleInputField.SetPlaceholderText(UIStrings.Instance.CharGen.EnterSearchTextHere);
		m_ConsoleInputField.Bind(null, base.OnSearchStringEdit);
	}

	public void AddInput(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		m_SearchHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Select();
		}, 16, enabledHints)).AddTo(this);
	}

	public IDisposable AddInputDisposable(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		return m_SearchHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Select();
		}, 16, enabledHints));
	}

	public override void SetActive(bool value)
	{
		m_IsActive.Value = value;
		base.gameObject.SetActive(value);
		m_ConsoleInputField.Text = string.Empty;
		ContextMenuHelper.HideContextMenu();
		if (value)
		{
			m_ConsoleInputField.OnConfirmClick();
			return;
		}
		m_ConsoleInputField.Abort();
		OnSearchStringEdit(null);
	}

	private void ShowDropdownMenu(InputActionEventData data)
	{
		TooltipHelper.HideTooltip();
	}
}
