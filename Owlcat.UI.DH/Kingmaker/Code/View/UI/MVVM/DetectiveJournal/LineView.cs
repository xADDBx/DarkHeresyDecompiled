using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class LineView : MonoBehaviour
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_LinePrefab;

	[Header("Values")]
	[SerializeField]
	private float m_AnimationTime = 0.5f;

	[SerializeField]
	private Color m_DefaultColor;

	[SerializeField]
	private Color m_AnswerColor;

	[SerializeField]
	private Color m_AnswerSelectedColor;

	[SerializeField]
	private Color m_CommonGreenColor;

	private ClueLineData m_LineData;

	private LineType m_LineType;

	private RectTransform m_RectTransform;

	private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

	private readonly List<RectTransform> m_LineViews = new List<RectTransform>();

	public void Initialize(ClueLineData lineData, LineType lineType = LineType.Default)
	{
		m_LineData = lineData;
		m_LineType = lineType;
		if ((object)m_RectTransform == null)
		{
			m_RectTransform = GetComponent<RectTransform>();
		}
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			UpdateLines();
		}).AddTo(m_Disposables);
		UpdateLines();
	}

	public void Destroy()
	{
		m_Disposables?.Dispose();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Animate()
	{
		m_LineViews.ForEach(delegate(RectTransform v)
		{
			v.localScale = new Vector3(0f, 1f, 1f);
			v.DOScale(Vector3.one, m_AnimationTime).SetUpdate(isIndependentUpdate: true);
		});
	}

	private void UpdateLines()
	{
		if (!(m_LineData.DotFrom == null) && !(m_LineData.DotTo == null))
		{
			UpdateLineFromTo(m_LineData.DotFrom.position, m_LineData.DotTo.position, 0);
		}
	}

	private void UpdateLineFromTo(Vector3 posFrom, Vector3 posTo, int lineId)
	{
		float num = Vector3.Distance(posFrom, posTo);
		Vector3 vector = posTo - posFrom;
		float z = (MathF.PI + Mathf.Atan2(vector.y, vector.x)) * 57.29578f;
		RectTransform rectTransform;
		if (m_LineViews.Count > lineId)
		{
			rectTransform = m_LineViews[lineId];
		}
		else
		{
			rectTransform = UnityEngine.Object.Instantiate(m_LinePrefab, base.transform);
			rectTransform.EnsureComponent<Image>().color = GetColor();
			m_LineViews.Add(rectTransform);
		}
		rectTransform.transform.position = posFrom;
		rectTransform.eulerAngles = new Vector3(0f, 0f, z);
		rectTransform.sizeDelta = new Vector2(num / base.transform.lossyScale.x, rectTransform.sizeDelta.y);
	}

	private Color GetColor()
	{
		bool flag = UIConfig.Instance.DetectiveConfig.AnswerDebugValues.HasFlag(AnswerDebugValues.CommonLinesAreGreen);
		return m_LineType switch
		{
			LineType.Default => flag ? m_CommonGreenColor : m_DefaultColor, 
			LineType.Answer => m_AnswerColor, 
			LineType.AnswerSelected => m_AnswerSelectedColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
