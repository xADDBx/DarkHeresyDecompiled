using DG.Tweening;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class SimpleAnswerView : View<SimpleAnswerVM>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private TMP_Text m_AnswerName;

	[SerializeField]
	private TMP_Text m_AnswerDescription;

	[SerializeField]
	private OwlcatMultiButton m_AnswerButton;

	[SerializeField]
	private OwlcatMultiSelectable m_NewStateSelectable;

	[SerializeField]
	private GameObject m_NameContainer;

	[Header("Values")]
	[SerializeField]
	private float m_ExpandTime = 0.25f;

	[SerializeField]
	private float m_ExpandDelay = 0.1f;

	private Tweener m_ExpandTween;

	protected override void OnBind()
	{
		m_NameContainer.SetActive(base.ViewModel.Answer.RelatedItem != null);
		m_AnswerName.text = base.ViewModel.Answer.RelatedItem?.Blueprint.Name.Text;
		m_AnswerButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnAnswerClick).AddTo(this);
		base.ViewModel.IsNew.Subscribe(delegate(bool value)
		{
			m_NewStateSelectable.SetActiveLayer(value ? "New" : "Default");
		}).AddTo(this);
		base.ViewModel.AnswerDescription.Subscribe(delegate(string value)
		{
			m_AnswerDescription.text = value;
		}).AddTo(this);
		m_AnswerButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.MarkAsViewed();
		}).AddTo(this);
	}

	public void SetExpandState(bool state)
	{
		float startHeight = m_RectTransform.sizeDelta.y;
		float preferredHeight = (state ? 122f : 40f);
		LayoutElement descLayout = m_AnswerDescription.EnsureComponent<LayoutElement>();
		descLayout.minHeight = -1f;
		m_ExpandTween.Kill();
		m_ExpandTween = DOTween.To(() => 0f, delegate(float x)
		{
			m_LayoutElement.preferredHeight = Mathf.Lerp(startHeight, preferredHeight, x);
		}, 1f, m_ExpandTime).SetUpdate(isIndependentUpdate: true).SetDelay(state ? m_ExpandDelay : 0f)
			.OnComplete(delegate
			{
				descLayout.minHeight = (state ? 80f : (-1f));
				m_LayoutElement.preferredHeight = (state ? (-1f) : preferredHeight);
			});
	}
}
