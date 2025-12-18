using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SmartSliders;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CombatCurrentUnitActionPointsSlider : MonoBehaviour, IDisposable
{
	[SerializeField]
	private ActionPointsSliderType m_Type;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private LampSliderPrediction m_Prediction;

	[SerializeField]
	private Image m_HintPlace;

	[SerializeField]
	private Image m_Icon;

	private Color m_OriginalCurrentLabelColor;

	[SerializeField]
	private Color m_DisableColor = Color.gray;

	private void Awake()
	{
		m_OriginalCurrentLabelColor = m_Icon.color;
	}

	public IDisposable Bind(ReadOnlyReactiveProperty<int> maxValue, ReadOnlyReactiveProperty<int> currentValue, ReadOnlyReactiveProperty<int> predictionValue)
	{
		if ((bool)m_HintPlace)
		{
			currentValue.Subscribe(delegate(int value)
			{
				SetPointsHint(value);
			}).AddTo(this);
		}
		return m_Prediction.Bind(maxValue, currentValue, predictionValue);
	}

	public void SetVisible(bool visible)
	{
		m_CanvasGroup.alpha = (visible ? 1f : 0f);
		m_CanvasGroup.blocksRaycasts = visible;
	}

	public void SetCurrentValue(int value)
	{
		m_Icon.color = ((value <= 0) ? m_DisableColor : m_OriginalCurrentLabelColor);
	}

	public void SetPointsHint(float value)
	{
		if (!(m_HintPlace == null))
		{
			switch (m_Type)
			{
			case ActionPointsSliderType.AP:
				m_HintPlace.SetHint($"{UIStrings.Instance.ActionBar.ActionPoints.Text}: {value}");
				break;
			case ActionPointsSliderType.MP:
				m_HintPlace.SetHint($"{UIStrings.Instance.ActionBar.MovementPoints.Text}: {value}");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void Dispose()
	{
		m_Prediction?.Dispose();
	}
}
