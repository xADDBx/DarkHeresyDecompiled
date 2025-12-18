using System;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.Gameplay.Parts;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class UnitMoraleView : View<UnitMoraleVM>
{
	[SerializeField]
	private Image m_MoraleIcon;

	[SerializeField]
	private TMP_Text m_ValueLabel;

	[Space]
	[SerializeField]
	private MoraleColorOption[] m_MoralePair;

	[SerializeField]
	private MoraleColorOption m_DefaultMoralePair;

	[SerializeField]
	private GameObject[] m_HeroicMarkers;

	[SerializeField]
	private GameObject[] m_BrokenMarkers;

	[Space]
	[SerializeField]
	private Graphic m_AttentionObject;

	[SerializeField]
	private RectTransform m_AttentionObjectTransform;

	[SerializeField]
	private float m_PositiveAttentionYPos;

	[SerializeField]
	private float m_NegativeAttentionYPos;

	[SerializeField]
	private Vector3 m_PositiveAttentionScale = Vector3.one;

	[SerializeField]
	private Vector3 m_NegativeAttentionScale = Vector3.one;

	[SerializeField]
	private float m_BlinkSpeed = 1f;

	private IDisposable m_TooltipDisposable;

	private Tweener m_BlinkAnimation;

	protected override void OnBind()
	{
		UpdateMorale();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateMoraleValue, delegate
		{
			UpdateMorale();
		}).AddTo(this);
		base.ViewModel.WillBecomeBroken.CombineLatest(base.ViewModel.WillBecomeHeroic, (bool willBeBroken, bool willBeHeroic) => new { willBeBroken, willBeHeroic }).Subscribe(data =>
		{
			UpdateAttentionMark(data.willBeBroken, data.willBeHeroic);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_BlinkAnimation.Pause();
		m_TooltipDisposable?.Dispose();
		m_TooltipDisposable = null;
	}

	private void UpdateMorale()
	{
		m_ValueLabel.text = Math.Abs(base.ViewModel.MoraleValue.CurrentValue).ToString();
		MoralePhaseType currentValue = base.ViewModel.MoralePhase.CurrentValue;
		MoraleColorOption moraleColorOption = GetColorsByPhaseType(currentValue);
		if (moraleColorOption == null)
		{
			moraleColorOption = m_DefaultMoralePair;
		}
		m_ValueLabel.color = moraleColorOption.TextColor;
		m_MoraleIcon.color = moraleColorOption.Color;
		m_MoraleIcon.sprite = moraleColorOption.BackgroundSprite;
		CanvasGroup[] activeGroups = moraleColorOption.ActiveGroups;
		for (int i = 0; i < activeGroups.Length; i++)
		{
			activeGroups[i].alpha = 1f;
		}
		activeGroups = moraleColorOption.DisabledGroups;
		for (int i = 0; i < activeGroups.Length; i++)
		{
			activeGroups[i].alpha = 0f;
		}
		bool active = currentValue == MoralePhaseType.Heroic;
		bool active2 = currentValue == MoralePhaseType.Broken;
		GameObject[] heroicMarkers = m_HeroicMarkers;
		for (int i = 0; i < heroicMarkers.Length; i++)
		{
			heroicMarkers[i].SetActive(active);
		}
		heroicMarkers = m_BrokenMarkers;
		for (int i = 0; i < heroicMarkers.Length; i++)
		{
			heroicMarkers[i].SetActive(active2);
		}
	}

	private void UpdateAttentionMark(bool willBecomeBroken, bool willBecomeHeroic)
	{
		m_BlinkAnimation?.Pause();
		bool flag = willBecomeHeroic || willBecomeBroken;
		m_AttentionObject.gameObject.SetActive(flag);
		if (flag)
		{
			Vector2 anchoredPosition = m_AttentionObjectTransform.anchoredPosition;
			anchoredPosition.y = (willBecomeHeroic ? m_PositiveAttentionYPos : m_NegativeAttentionYPos);
			m_AttentionObjectTransform.anchoredPosition = anchoredPosition;
			m_AttentionObjectTransform.localScale = (willBecomeHeroic ? m_PositiveAttentionScale : m_NegativeAttentionScale);
			Color color = m_AttentionObject.color;
			color.a = 1f;
			m_AttentionObject.color = color;
			if (m_BlinkAnimation == null)
			{
				m_BlinkAnimation = m_AttentionObject.DOFade(0f, m_BlinkSpeed).SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Yoyo)
					.SetAutoKill(autoKillOnCompletion: false);
			}
			m_BlinkAnimation.Restart();
		}
	}

	private MoraleColorOption GetColorsByPhaseType(MoralePhaseType phaseType)
	{
		return phaseType switch
		{
			MoralePhaseType.Heroic => m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Heroic), 
			MoralePhaseType.Broken => m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Broken), 
			_ => GetRegularMoralePhaseColors(), 
		};
	}

	private MoraleColorOption GetRegularMoralePhaseColors()
	{
		if (base.ViewModel.WillBecomeHeroic.CurrentValue)
		{
			return m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.PreHeroic);
		}
		if (base.ViewModel.WillBecomeBroken.CurrentValue)
		{
			return m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.PreBroken);
		}
		return m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Regular && v.MinValue <= base.ViewModel.MoraleValue.CurrentValue && base.ViewModel.MoraleValue.CurrentValue <= v.MaxValue);
	}

	protected override void OnDestroy()
	{
		m_BlinkAnimation?.Kill();
		base.OnDestroy();
	}
}
