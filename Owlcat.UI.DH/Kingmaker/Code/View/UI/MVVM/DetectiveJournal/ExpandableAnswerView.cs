using DG.Tweening;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ExpandableAnswerView : View<BlueprintCaseAnswer>, IAnswerTierChanged, ISubscriber, IAnswerTierViewed
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ClueName;

	[SerializeField]
	private ScrambledTMP m_AnswerDescription;

	[SerializeField]
	private Image m_ClueIcon;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private OwlcatMultiSelectable m_HasClueSelectable;

	[SerializeField]
	private GameObject m_NewIcon;

	[Header("Values")]
	[SerializeField]
	private Vector2 m_ExpandSize;

	[FormerlySerializedAs("m_AnimationTime")]
	[SerializeField]
	private float m_AnimationTimeX = 0.5f;

	[SerializeField]
	private float m_AnimationTimeY = 0.2f;

	[SerializeField]
	private float m_HideTime = 0.3f;

	private Tweener m_TweenerX;

	private Tweener m_TweenerY;

	private Tweener m_TweenerHide;

	protected override void OnBind()
	{
		if (base.ViewModel.RelatedItem?.Blueprint is BlueprintClue clue)
		{
			m_ClueIcon.sprite = clue.GetUIData().Icon;
			m_ClueName.text = clue.GetUIData().Name.Text;
			m_HasClueSelectable.SetActiveLayer(0);
		}
		else
		{
			m_HasClueSelectable.SetActiveLayer(1);
		}
		m_Background.OnPointerEnterAsObservable().Subscribe(OnPointerEnter).AddTo(this);
		m_Background.OnPointerExitAsObservable().Subscribe(OnPointerExit).AddTo(this);
		m_LayoutElement.minWidth = 0f;
		m_LayoutElement.minHeight = 0f;
		UpdateNewAnswerIcon();
		EventBus.Subscribe(this).AddTo(this);
	}

	private void OnPointerEnter(PointerEventData eventData)
	{
		m_TweenerX.Kill();
		m_TweenerY.Kill();
		float widthFrom = m_LayoutElement.minWidth;
		float heightFrom = m_LayoutElement.minHeight;
		m_TweenerX = DOTween.To(() => 0f, delegate(float x)
		{
			m_LayoutElement.minWidth = Mathf.Lerp(widthFrom, m_ExpandSize.x, x);
		}, 1f, m_AnimationTimeX * (1f - widthFrom / m_ExpandSize.x)).OnComplete(delegate
		{
			m_NewIcon.SetActive(value: false);
			UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.AddExaminedAnswerIfNeeded(base.ViewModel);
			EventBus.RaiseEvent(delegate(IAnswerTierViewed h)
			{
				h.HandleAnswerTierViewed(base.ViewModel);
			});
		}).SetUpdate(isIndependentUpdate: true);
		m_TweenerY = DOTween.To(() => 0f, delegate(float x)
		{
			m_LayoutElement.minHeight = Mathf.Lerp(heightFrom, m_ExpandSize.y, x);
		}, 1f, m_AnimationTimeY * (1f - heightFrom / m_ExpandSize.y)).SetUpdate(isIndependentUpdate: true).SetEase(Ease.Linear);
		LocalizedString answerDegreeDescription = UIUtilityDetective.GetAnswerDegreeDescription(base.ViewModel);
		m_AnswerDescription.SetText("##&$^###&#@##", answerDegreeDescription.Text);
	}

	private void OnPointerExit(PointerEventData eventData)
	{
		m_TweenerX.Kill();
		m_TweenerY.Kill();
		float widthFrom = m_LayoutElement.minWidth;
		float heightFrom = m_LayoutElement.minHeight;
		m_TweenerX = DOTween.To(() => 0f, delegate(float x)
		{
			m_LayoutElement.minWidth = Mathf.Lerp(widthFrom, 0f, x);
		}, 1f, m_HideTime * widthFrom / m_ExpandSize.x).SetUpdate(isIndependentUpdate: true).SetEase(Ease.Linear);
		m_TweenerY = DOTween.To(() => 0f, delegate(float x)
		{
			m_LayoutElement.minHeight = Mathf.Lerp(heightFrom, 0f, x);
		}, 1f, m_HideTime * heightFrom / m_ExpandSize.y).SetUpdate(isIndependentUpdate: true).SetEase(Ease.Linear);
	}

	public void HandleAnswerTierChanged(BlueprintCaseAnswer answer, int oldTier, int newTier)
	{
		if (answer == base.ViewModel)
		{
			m_NewIcon.SetActive(value: true);
		}
	}

	public void HandleAnswerTierViewed(BlueprintCaseAnswer answer)
	{
		if (base.ViewModel == answer)
		{
			UpdateNewAnswerIcon();
		}
	}

	private void UpdateNewAnswerIcon()
	{
		m_NewIcon.SetActive(UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.IsEntityNew(base.ViewModel));
	}
}
