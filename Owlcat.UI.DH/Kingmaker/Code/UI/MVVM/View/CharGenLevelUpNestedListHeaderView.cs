using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpNestedListHeaderView : SelectionGroupEntityView<CharGenLevelUpNestedListHeaderVM>
{
	private const int PADDING_FOR_NESTING_LEVEL = 20;

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup m_Layout;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_RecommendationIcon;

	[SerializeField]
	private RectTransform m_Arrow;

	private void OnEnable()
	{
		ObservableSubscribeExtensions.Subscribe(m_Button.OnConfirmClickAsObservable(), delegate
		{
			OnClick();
		}).AddTo(this);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_Layout.padding.left = base.ViewModel.NestingLevel * 20;
		base.ViewModel.IsShowed.Subscribe(delegate(bool e)
		{
			base.gameObject.SetActive(e);
		}).AddTo(this);
		base.ViewModel.IsRecommended.Subscribe(delegate(bool r)
		{
			m_RecommendationIcon.SetActive(r);
		}).AddTo(this);
		base.ViewModel.IsExpanded.Subscribe(delegate(bool r)
		{
			m_Arrow.rotation = Quaternion.Euler(0f, 0f, (!r) ? (-90) : 0);
		}).AddTo(this);
		if (m_Label != null)
		{
			AddDisposable(base.ViewModel.Label.Subscribe(delegate(string l)
			{
				m_Label.text = l;
			}));
		}
	}

	protected override void OnClick()
	{
		base.ViewModel.ToggleExpand();
	}
}
