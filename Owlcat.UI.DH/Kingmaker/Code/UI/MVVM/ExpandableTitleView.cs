using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExpandableTitleView : VirtualListElementViewBase<ExpandableTitleVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IVirtualListElementIdentifier
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatMultiButton m_ExpandButton;

	[SerializeField]
	private Transform m_ExpandArrow;

	[SerializeField]
	private float m_CollapsedAngle = 90f;

	[SerializeField]
	private bool m_HasNavigation = true;

	[ShowIf("m_HasNavigation")]
	[SerializeField]
	private OwlcatMultiButton m_ConsoleFocusButton;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private bool m_IsExpanded;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public bool IsExpanded => base.ViewModel.IsExpanded.CurrentValue;

	public int VirtualListTypeId => 0;

	protected override void BindViewImplementation()
	{
		m_Title.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsExpanded.Subscribe(ExpandStatedChanged));
		if (base.ViewModel.IsSwitchable)
		{
			AddDisposable(ObservableSubscribeExtensions.Subscribe(m_ExpandButton.OnLeftClickAsObservable(), delegate
			{
				Switch();
			}));
		}
		m_ConsoleFocusButton.SetFocus(value: false);
		m_ExpandButton.SetFocus(value: false);
		m_ExpandArrow.Or(null)?.gameObject.SetActive(base.ViewModel.IsSwitchable);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void ExpandStatedChanged(bool expanded)
	{
		m_ExpandButton.SetActiveLayer(expanded ? "Expanded" : "Collapsed");
		m_ConsoleFocusButton.ConfirmClickHint = (expanded ? UIStrings.Instance.CommonTexts.Collapse : UIStrings.Instance.CommonTexts.Expand);
		m_ExpandArrow.Or(null)?.DOLocalRotate(new Vector3(0f, 0f, expanded ? 0f : m_CollapsedAngle), 0.2f).SetUpdate(isIndependentUpdate: true);
	}

	public void Expand()
	{
		base.ViewModel.Expand();
	}

	public void Collapse()
	{
		base.ViewModel.Collapse();
	}

	public void Switch()
	{
		base.ViewModel.Switch();
	}

	public void SetFocus(bool value)
	{
		if (m_HasNavigation)
		{
			m_ConsoleFocusButton.SetFocus(value);
		}
		m_ExpandButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ExpandButton.Interactable;
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.IsSwitchable;
	}

	public void OnConfirmClick()
	{
		Switch();
	}

	public string GetConfirmClickHint()
	{
		return IsExpanded ? UIStrings.Instance.CommonTexts.Collapse : UIStrings.Instance.CommonTexts.Expand;
	}
}
