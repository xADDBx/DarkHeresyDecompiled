using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.Dialog;

public class TrueAnswerEntityView : View<TrueAnswerEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private ClassifiedRect m_ClassifiedRectPrefab;

	[Header("Values")]
	[SerializeField]
	private Vector2 m_CorrectPosition;

	[SerializeField]
	private float m_RectHeight = 40f;

	private readonly ReactiveProperty<bool> m_RectanglesInitialized = new ReactiveProperty<bool>();

	private readonly List<ClassifiedRect> m_Rects = new List<ClassifiedRect>();

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.Text;
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			InitializeRectangles();
		}).AddTo(this);
		base.ViewModel.AnswerShown.Subscribe(UpdateClassifiedRects).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Rects.ForEach(delegate(ClassifiedRect r)
		{
			Object.Destroy(r.gameObject);
		});
		m_Rects.Clear();
	}

	private void UpdateClassifiedRects(bool answerShown)
	{
		m_Rects.ForEach(delegate(ClassifiedRect r)
		{
			r.SetHasClueState(answerShown);
		});
		if (answerShown)
		{
			m_Text.transform.SetAsLastSibling();
		}
		else
		{
			m_Text.transform.SetAsFirstSibling();
		}
	}

	private void InitializeRectangles()
	{
		TMP_CharacterInfo tMP_CharacterInfo = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueStartId);
		TMP_CharacterInfo tMP_CharacterInfo2 = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueEndId);
		for (int i = tMP_CharacterInfo.lineNumber; i < tMP_CharacterInfo2.lineNumber + 1; i++)
		{
			ClassifiedRect classifiedRect = Object.Instantiate(m_ClassifiedRectPrefab, base.transform, worldPositionStays: false);
			classifiedRect.transform.SetSiblingIndex(1);
			m_Rects.Add(classifiedRect);
		}
		UpdatePositions();
		m_RectanglesInitialized.Value = true;
		m_Rects.ForEach(delegate(ClassifiedRect r)
		{
			r.OnPointerClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.ShowAnswer();
			}).AddTo(this);
		});
	}

	private void UpdatePositions()
	{
		TMP_CharacterInfo tMP_CharacterInfo = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueStartId);
		TMP_CharacterInfo tMP_CharacterInfo2 = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueEndId);
		for (int i = tMP_CharacterInfo.lineNumber; i < tMP_CharacterInfo2.lineNumber + 1; i++)
		{
			ClassifiedRect classifiedRect = m_Rects[i - tMP_CharacterInfo.lineNumber];
			int index = ((i == tMP_CharacterInfo.lineNumber) ? base.ViewModel.ClueStartId : GetFirstLetterIdInLine(i));
			int index2 = ((i == tMP_CharacterInfo2.lineNumber) ? base.ViewModel.ClueEndId : (GetFirstLetterIdInLine(i + 1) - 1));
			TMP_CharacterInfo firstLetter = m_Text.textInfo.characterInfo.ElementAtOrDefault(index);
			TMP_CharacterInfo lastLetter = m_Text.textInfo.characterInfo.ElementAtOrDefault(index2);
			DrawClassifiedRect(firstLetter, lastLetter, classifiedRect.RectTransform);
		}
	}

	private int GetFirstLetterIdInLine(int lineNumber)
	{
		for (int i = base.ViewModel.ClueStartId; i < m_Text.textInfo.characterCount; i++)
		{
			if (m_Text.textInfo.characterInfo[i].lineNumber >= lineNumber)
			{
				return i;
			}
		}
		return -1;
	}

	private void DrawClassifiedRect(TMP_CharacterInfo firstLetter, TMP_CharacterInfo lastLetter, RectTransform rect)
	{
		CalculateClassifiedCoordinates(firstLetter, lastLetter, out var width, out var midBotPoint);
		Extents lineExtents = m_Text.textInfo.lineInfo[firstLetter.lineNumber].lineExtents;
		midBotPoint.y = (lineExtents.min.y + lineExtents.max.y) * 0.5f;
		rect.anchoredPosition = m_CorrectPosition + midBotPoint;
		rect.sizeDelta = new Vector2(width, m_RectHeight);
		rect.gameObject.SetActive(value: true);
	}

	private static void CalculateClassifiedCoordinates(TMP_CharacterInfo firstLetter, TMP_CharacterInfo lastLetter, out float width, out Vector2 midBotPoint)
	{
		width = lastLetter.topRight.x - firstLetter.topLeft.x;
		midBotPoint = new Vector2(firstLetter.topLeft.x + width / 2f, firstLetter.bottomLeft.y);
	}
}
