using System.Collections.Generic;
using Code.View.UI.UIUtils;
using DG.Tweening;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationWidgetView : View<FactionReputationWidgetVM>
{
	[Header("Elements")]
	[FormerlySerializedAs("factionLogo")]
	[SerializeField]
	private Image m_FactionLogo;

	[FormerlySerializedAs("factionName")]
	[SerializeField]
	private TextMeshProUGUI m_FactionName;

	[FormerlySerializedAs("fearValue")]
	[SerializeField]
	private TextMeshProUGUI m_FearValue;

	[FormerlySerializedAs("respectValue")]
	[SerializeField]
	private TextMeshProUGUI m_RespectValue;

	[FormerlySerializedAs("fearBarFill")]
	[SerializeField]
	private Image m_FearBarFill;

	[SerializeField]
	private OwlcatMultiButton m_FearButton;

	[FormerlySerializedAs("respectBarFill")]
	[SerializeField]
	private Image m_RespectBarFill;

	[SerializeField]
	private RectTransform m_RespectTopLine;

	[SerializeField]
	private RectTransform m_FearTopLine;

	[SerializeField]
	private OwlcatMultiButton m_RespectButton;

	[Header("Values")]
	[SerializeField]
	private float m_ValuesAnimationDuration = 0.5f;

	protected override void OnBind()
	{
		m_FactionLogo.sprite = base.ViewModel.VendorFaction.Icon;
		m_FactionName.text = base.ViewModel.VendorFaction.DisplayName;
		m_FearValue.text = base.ViewModel.FearLevel.CurrentValue.ToString();
		m_RespectValue.text = base.ViewModel.RespectLevel.CurrentValue.ToString();
		base.ViewModel.FearLevel.Subscribe(MoveFearTo).AddTo(this);
		base.ViewModel.RespectLevel.Subscribe(MoveRespectTo).AddTo(this);
		m_FactionLogo.SetTooltip(new TooltipTemplateGlossary(UIUtilityEncyclopedy.GetFactionEncyclopediaKey(base.ViewModel.FactionType))).AddTo(this);
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(1f, 0.5f),
			new Vector2(1f, 0.75f),
			new Vector2(1f, 0.25f),
			new Vector2(0f, 0.5f)
		};
		TooltipConfig config = tooltipConfig;
		tooltipConfig = default(TooltipConfig);
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0f, 0.5f),
			new Vector2(0f, 0.75f),
			new Vector2(0f, 0.25f),
			new Vector2(1f, 0.5f)
		};
		TooltipConfig config2 = tooltipConfig;
		m_FearButton.SetTooltip(new TooltipTemplateGlossary("Presence"), config).AddTo(this);
		m_RespectButton.SetTooltip(new TooltipTemplateGlossary("Rapport"), config2).AddTo(this);
	}

	private void MoveFearTo(int value)
	{
		DOTween.To(() => 0, delegate(int fear)
		{
			SetProgress(fear, m_FearBarFill, m_FearValue, m_FearTopLine);
		}, value, m_ValuesAnimationDuration).OnComplete(delegate
		{
			SetProgress(value, m_FearBarFill, m_FearValue, m_FearTopLine);
		}).SetUpdate(isIndependentUpdate: true);
	}

	private void MoveRespectTo(int value)
	{
		DOTween.To(() => 0, delegate(int respect)
		{
			SetProgress(respect, m_RespectBarFill, m_RespectValue, m_RespectTopLine);
		}, value, m_ValuesAnimationDuration).OnComplete(delegate
		{
			SetProgress(value, m_RespectBarFill, m_RespectValue, m_RespectTopLine);
		}).SetUpdate(isIndependentUpdate: true);
	}

	private void SetProgress(int value, Image barFill, TextMeshProUGUI valueText, RectTransform topLine)
	{
		float percent = (barFill.fillAmount = Mathf.Clamp01((float)value / 100f));
		valueText.text = value.ToString();
		SetVerticalPosition(topLine, percent);
	}

	public void SetVerticalPosition(RectTransform target, float percent)
	{
		if (!(target == null) && target.parent is RectTransform { rect: { height: var height } } rectTransform)
		{
			float num = (0f - height) * rectTransform.pivot.y;
			target.SetLocalPositionAndRotation(new Vector3(target.localPosition.x, num + percent * height, target.localPosition.z), target.localRotation);
		}
	}
}
