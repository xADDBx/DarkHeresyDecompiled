using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeatureGroupPCView : View<CharInfoFeatureGroupVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_WidgetEntityView;

	[SerializeField]
	protected ExpandableCollapseMultiButtonPC m_ExpandableElement;

	[SerializeField]
	protected CharInfoFeatureGroupVM.FeatureGroupType m_GroupType;

	private AccessibilityTextHelper m_TextHelper;

	public bool IsEmpty => base.ViewModel?.IsEmpty ?? true;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			base.gameObject.SetActive(!IsEmpty);
		});
		if (!IsEmpty)
		{
			SetupLabel();
			DrawEntities();
			m_TextHelper.UpdateTextSize();
		}
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}

	private void SetupLabel()
	{
		if (m_Label != null)
		{
			m_Label.text = base.ViewModel.Label;
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.FeatureList.ToArray(), m_WidgetEntityView, unused: true);
	}

	public void Expand()
	{
		m_ExpandableElement.SetValue(isOn: true, isImmediately: true);
	}
}
