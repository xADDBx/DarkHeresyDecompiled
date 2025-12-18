using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class PaperReportEntityView : View<PaperReportEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private TextTypewriter m_ClueText;

	[SerializeField]
	private ClassifiedRect m_ClassifiedRectPrefab;

	[Header("Values")]
	[SerializeField]
	private Vector2 m_CorrectPosition = new Vector2(0f, -3f);

	[SerializeField]
	private float m_RectHeight = 40f;

	private readonly ReactiveProperty<bool> m_RectanglesInitialized = new ReactiveProperty<bool>();

	private readonly List<ClassifiedRect> m_Rects = new List<ClassifiedRect>();

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.GetDescription();
		base.ViewModel.SelectedAnswer.Skip(1).CombineLatest(m_RectanglesInitialized, (BlueprintCaseAnswer clue, bool initialized) => new { clue, initialized }).Subscribe(value =>
		{
			if (value.initialized)
			{
				UpdateClassifiedRects(value.clue);
			}
		})
			.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			InitializeRectangles();
		}).AddTo(this);
		UpdateClassifiedRects(base.ViewModel.SelectedAnswer.CurrentValue, force: true);
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

	private void UpdateClassifiedRects(BlueprintCaseAnswer answer, bool force = false)
	{
		m_Rects.ForEach(delegate(ClassifiedRect r)
		{
			r.SetHasClueState(answer != null);
		});
		m_ClueText.SetTextAnimation(base.ViewModel.GetSelectedClueDescription(), base.ViewModel.ClueStartId, force);
	}

	private void InitializeRectangles()
	{
		TMP_CharacterInfo tMP_CharacterInfo = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueStartId);
		TMP_CharacterInfo tMP_CharacterInfo2 = m_Text.textInfo.characterInfo.ElementAtOrDefault(base.ViewModel.ClueEndId);
		for (int i = tMP_CharacterInfo.lineNumber; i < tMP_CharacterInfo2.lineNumber + 1; i++)
		{
			ClassifiedRect classifiedRect = Object.Instantiate(m_ClassifiedRectPrefab, base.transform, worldPositionStays: false);
			classifiedRect.transform.SetSiblingIndex(1);
			int index = ((i == tMP_CharacterInfo.lineNumber) ? base.ViewModel.ClueStartId : GetFirstLetterIdInLine(i));
			int index2 = ((i == tMP_CharacterInfo2.lineNumber) ? base.ViewModel.ClueEndId : (GetFirstLetterIdInLine(i + 1) - 1));
			TMP_CharacterInfo firstLetter = m_Text.textInfo.characterInfo.ElementAtOrDefault(index);
			TMP_CharacterInfo lastLetter = m_Text.textInfo.characterInfo.ElementAtOrDefault(index2);
			DrawClassifiedRect(firstLetter, lastLetter, classifiedRect.RectTransform);
			m_Rects.Add(classifiedRect);
		}
		m_RectanglesInitialized.Value = true;
		UpdateClassifiedRects(base.ViewModel.SelectedAnswer.CurrentValue, force: true);
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
