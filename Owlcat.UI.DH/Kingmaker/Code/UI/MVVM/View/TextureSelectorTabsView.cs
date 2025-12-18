using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorTabsView : BaseCharGenAppearancePageComponentView<TextureSelectorTabsVM>, IFunc01ClickHandler, IConsoleEntity, IConsoleEntityProxy
{
	[SerializeField]
	private TextureSelectorGroupView m_GroupView;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private ClickablePageNavigation m_Paginator;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private bool m_IsInputAdded;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public IConsoleEntity ConsoleEntityProxy => m_GroupView;

	public bool CanFunc01Click()
	{
		return true;
	}

	public void OnFunc01Click()
	{
		m_Paginator.NextPage();
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.CharGen.SwitchPageSet;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentTabSelector.Subscribe(delegate(TextureSelectorVM value)
		{
			m_GroupView.Bind(value.SelectionGroup);
		}));
		AddDisposable(base.ViewModel.Title.Subscribe(SetTitleText));
		AddDisposable(base.ViewModel.TotalItems.CombineLatest(base.ViewModel.CurrentIndex, (int total, int current) => new { total, current }).Subscribe(value =>
		{
			m_Counter.text = $"{value.current + 1}/{value.total}";
		}));
		m_Paginator.Initialize(base.ViewModel.TotalItems.CurrentValue, delegate(int idx)
		{
			base.ViewModel.SetIndex(idx);
		});
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(delegate(int value)
		{
			if (!UtilityNet.IsControlMainCharacter())
			{
				m_Paginator.OnClickPage(value);
			}
		}));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_IsInputAdded = false;
		m_GroupView.Unbind();
		Hide();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_GroupView.SetFocus(value);
	}

	public override void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (!m_IsInputAdded)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				OnFunc01Click();
			}, 10, IsFocused);
			AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharGen.SwitchPageSet));
			AddDisposable(inputBindStruct);
			m_IsInputAdded = true;
			AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnSetValues, delegate
			{
				UpdateInternalFocus();
			}));
			AddDisposable(base.ViewModel.CurrentIndex.Subscribe(delegate
			{
				UpdateInternalFocus();
			}));
		}
	}

	private void UpdateInternalFocus()
	{
		m_GroupView.SetFocus(IsFocused.Value);
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}
}
