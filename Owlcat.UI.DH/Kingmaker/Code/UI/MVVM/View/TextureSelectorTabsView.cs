using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorTabsView : BaseCharGenAppearancePageComponentView<TextureSelectorTabsVM>
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

	public void AddInput()
	{
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
