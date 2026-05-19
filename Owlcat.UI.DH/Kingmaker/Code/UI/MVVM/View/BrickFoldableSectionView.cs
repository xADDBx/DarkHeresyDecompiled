using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickFoldableSectionView : BrickGroupView<BrickFoldableSectionVM>
{
	[SerializeField]
	private OwlcatMultiButton m_HeaderButton;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private RectTransform m_Arrow;

	[SerializeField]
	private Transform m_Content;

	protected override void OnBind()
	{
		if (m_HeaderLabel != null)
		{
			m_HeaderLabel.text = base.ViewModel.Header;
		}
		ObservableSubscribeExtensions.Subscribe(m_HeaderButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Toggle();
		}).AddTo(this);
		base.ViewModel.IsExpanded.Subscribe(delegate(bool expanded)
		{
			m_Content.gameObject.SetActive(expanded);
			m_HeaderButton.SetActiveLayer(expanded ? "Expanded" : "Collapsed");
			if (m_Arrow != null)
			{
				m_Arrow.rotation = Quaternion.Euler(0f, 0f, expanded ? (-90) : 0);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		}).AddTo(this);
	}

	public override void AddChild(RectTransform childTransform)
	{
		childTransform.SetParent(m_Content, worldPositionStays: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
