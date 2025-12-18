using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswerTierChangeView : View<AnswerTierChangeVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_ToastButton;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TMP_Text m_ClueName;

	[SerializeField]
	private GameObject m_ClueNameParent;

	[SerializeField]
	private TMP_Text m_AnswerTierDescription;

	[Header("Values")]
	[SerializeField]
	private TextStyle m_TextStyle;

	private Sequence m_BlinkSequence;

	private BlueprintCaseAnswer m_ToastAnswer;

	protected override void OnBind()
	{
		m_ToastButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.PopToast).AddTo(this);
		base.ViewModel.UpgradedAnswers.ObserveCountChanged().Subscribe(delegate
		{
			UpdateCollection();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_BlinkSequence?.Kill();
		m_ToastButton.gameObject.SetActive(value: false);
		m_ToastAnswer = null;
	}

	private void UpdateCollection()
	{
		if (base.ViewModel.UpgradedAnswers.Count <= 0)
		{
			m_BlinkSequence?.Kill();
			m_ToastButton.gameObject.SetActive(value: false);
			m_ToastAnswer = null;
			return;
		}
		BlueprintCaseAnswer blueprintCaseAnswer = base.ViewModel.UpgradedAnswers.ElementAt(0);
		if (m_ToastAnswer != blueprintCaseAnswer)
		{
			ShowToast(blueprintCaseAnswer);
		}
	}

	private void ShowToast(BlueprintCaseAnswer answer)
	{
		m_ToastAnswer = answer;
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = m_TextStyle;
			GameLogContext.CaseItem = answer.RelatedItem?.Blueprint;
			m_ClueName.text = UIStrings.Instance.DetectiveJournal.AnswerChangedTitle.Text;
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
		}
		m_ClueNameParent.SetActive(answer.RelatedItem?.Blueprint != null);
		m_AnswerTierDescription.text = UIUtilityDetective.GetAnswerDegreeDescription(answer).Text;
		m_ToastButton.gameObject.SetActive(value: true);
		m_BlinkSequence?.Kill();
		m_CanvasGroup.alpha = 0f;
		(float, float)[] obj = new(float, float)[3]
		{
			(0.75f, 0.1f),
			(0.5f, 0.05f),
			(1f, 0.1f)
		};
		m_BlinkSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		m_BlinkSequence.Append(m_CanvasGroup.DOFade(0f, 0.3f).SetUpdate(isIndependentUpdate: true));
		(float, float)[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			(float, float) tuple = array[i];
			m_BlinkSequence.Append(m_CanvasGroup.DOFade(tuple.Item1, tuple.Item2).SetUpdate(isIndependentUpdate: true));
		}
		m_BlinkSequence.Play().SetUpdate(isIndependentUpdate: true);
	}
}
