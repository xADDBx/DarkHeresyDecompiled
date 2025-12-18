using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionWindowsLines : MonoBehaviour
{
	[Header("Views")]
	[SerializeField]
	private NewStraightLine m_StraightLinePrefab;

	[Header("Values")]
	[SerializeField]
	private float m_LineFromStartLength = -75f;

	[SerializeField]
	private float m_LineToStartLength = 50f;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	public void DrawLinesFrom(LineDirectionData clueFrom, List<ConclusionSelectionEntityView> selections)
	{
		RectTransform component = GetComponent<RectTransform>();
		for (int j = 0; j < selections.Count; j++)
		{
			NewStraightLine line = Object.Instantiate(m_StraightLinePrefab, base.transform, worldPositionStays: false);
			LineDirectionData lineFrom = selections[j].LineFrom;
			lineFrom.Length = m_LineFromStartLength;
			line.Initialize(clueFrom, lineFrom, component);
			NewStraightLine newStraightLine = Object.Instantiate(m_StraightLinePrefab, base.transform, worldPositionStays: false);
			newStraightLine.Initialize(clueFrom, selections[j].LineFrom, component);
			newStraightLine.GetComponent<CanvasGroup>().alpha = 0.075f;
			int i2 = j;
			selections[i2].Button.OnHoverAsObservable().Subscribe(delegate(bool value)
			{
				if (value)
				{
					EventBus.RaiseEvent(delegate(IUIHighlighter h)
					{
						h.StartHighlight(string.Format("{0}{1}", "ClueScale_", i2));
					});
				}
				else
				{
					EventBus.RaiseEvent(delegate(IUIHighlighter h)
					{
						h.StopHighlight(string.Format("{0}{1}", "ClueScale_", i2));
					});
				}
				if (!selections[i2].ViewModel.IsSelected.Value)
				{
					line.GetComponent<CanvasGroup>().alpha = (value ? 0.25f : 0f);
					line.gameObject.SetActive(value);
				}
			}).AddTo(m_Disposable);
			selections[j].ViewModel.IsSelected.Subscribe(delegate(bool value)
			{
				line.gameObject.SetActive(value);
				line.GetComponent<CanvasGroup>().alpha = 1f;
			}).AddTo(m_Disposable);
		}
	}

	public void DrawLinesTo(List<ConclusionSelectionEntityView> selections, List<LineDirectionData> entitiesTo)
	{
		RectTransform component = GetComponent<RectTransform>();
		for (int j = 0; j < selections.Count; j++)
		{
			NewStraightLine line = Object.Instantiate(m_StraightLinePrefab, base.transform, worldPositionStays: false);
			LineDirectionData lineTo = selections[j].LineTo;
			lineTo.Length = m_LineToStartLength + (float)j * 10f;
			LineDirectionData toDirection = entitiesTo[j].Negate();
			line.Initialize(lineTo, toDirection, component);
			NewStraightLine newStraightLine = Object.Instantiate(m_StraightLinePrefab, base.transform, worldPositionStays: false);
			newStraightLine.Initialize(lineTo, toDirection, component);
			newStraightLine.GetComponent<CanvasGroup>().alpha = 0.075f;
			int i2 = j;
			selections[i2].Button.OnHoverAsObservable().Subscribe(delegate(bool value)
			{
				if (!selections[i2].ViewModel.IsSelected.Value)
				{
					line.GetComponent<CanvasGroup>().alpha = (value ? 0.25f : 0f);
					line.gameObject.SetActive(value);
				}
			}).AddTo(m_Disposable);
			selections[j].ViewModel.IsSelected.Subscribe(delegate(bool value)
			{
				line.gameObject.SetActive(value);
				line.GetComponent<CanvasGroup>().alpha = 1f;
			}).AddTo(m_Disposable);
		}
	}

	public void Clear()
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
		m_Disposable?.Clear();
	}
}
