using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.Morale;

public class ActionBarMoraleView : View<ActionBarMoraleVM>
{
	[Header("Morale")]
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_ValueFill;

	[SerializeField]
	private Image m_MoraleIcon;

	[SerializeField]
	private Slider m_MoralePositiveSlider;

	[SerializeField]
	private Slider m_MoraleNegativeSlider;

	[SerializeField]
	private Image[] m_MoraleSliderFillImages;

	[Header("Slots")]
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private OwlcatSelectable m_AbilitiesHover;

	[SerializeField]
	private OwlcatSelectable m_BarHover;

	[SerializeField]
	private CanvasGroup m_HeroicAbilitiesContainer;

	[SerializeField]
	private CanvasGroup m_BrokenAbilitiesContainer;

	[SerializeField]
	private ActionBarMoraleAbilityContainerView m_HeroicAbilities;

	[SerializeField]
	private ActionBarMoraleAbilityContainerView m_BrokenAbilities;

	[Header("Visual")]
	[SerializeField]
	private CanvasGroup m_MainContainer;

	[SerializeField]
	private List<MoraleColorOption> m_MoralePair;

	[SerializeField]
	private MoraleColorOption m_DefaultMoralePair;

	[Header("Tooltip")]
	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	[SerializeField]
	private MonoBehaviour[] m_TooltipSources;

	[Header("Highlight")]
	[SerializeField]
	private Image m_Highlight;

	[SerializeField]
	private Color m_Heroic;

	[SerializeField]
	private Color m_Broken;

	private DisposableBag m_TooltipDisposable;

	private Dictionary<CanvasGroup, (Tween show, Tween hide)> m_Tweens = new Dictionary<CanvasGroup, (Tween, Tween)>();

	private Tween m_HeroicFadeTween;

	private Tween m_BrokenFadeTween;

	private ReactiveProperty<bool> m_ForceShowAbilities = new ReactiveProperty<bool>(value: false);

	private ReadOnlyReactiveProperty<bool> m_IsAbilitiesHover;

	protected override void OnBind()
	{
		m_MoralePositiveSlider.minValue = 0f;
		m_MoralePositiveSlider.maxValue = 1f;
		m_MoraleNegativeSlider.minValue = 0f;
		m_MoraleNegativeSlider.maxValue = 1f;
		base.ViewModel.HasMorale.Subscribe(delegate(bool hasMorale)
		{
			m_MainContainer.alpha = (hasMorale ? 1f : 0f);
			m_MainContainer.blocksRaycasts = hasMorale;
		}).AddTo(this);
		base.ViewModel.MoraleValue.CombineLatest(base.ViewModel.MoralePhase, base.ViewModel.WillBecomeBroken, base.ViewModel.WillBecomeHeroic, (int _, MoralePhaseType _, bool _, bool _) => new { }).Subscribe(_ =>
		{
			UpdateMorale();
		}).AddTo(this);
		m_HeroicAbilities.Initialize(broken: false);
		m_HeroicAbilities.Bind(base.ViewModel);
		m_BrokenAbilities.Initialize(broken: true);
		m_BrokenAbilities.Bind(base.ViewModel);
		m_IsAbilitiesHover = m_AbilitiesHover.OnHoverAsObservable().Or(m_BarHover.OnHoverAsObservable()).Or(m_ForceShowAbilities)
			.ToReadOnlyReactiveProperty(initialValue: false)
			.AddTo(this);
		m_BarHover.OnHoverAsObservable().Subscribe(ShowTooltip).AddTo(this);
		m_IsAbilitiesHover.Subscribe(delegate(bool v)
		{
			m_MoveAnimator.PlayAnimation(v);
		}).AddTo(this);
		ShowTooltip(hover: false);
	}

	protected override void OnUnbind()
	{
		m_TooltipDisposable.Clear();
		foreach (KeyValuePair<CanvasGroup, (Tween, Tween)> tween3 in m_Tweens)
		{
			tween3.Deconstruct(out var _, out var value);
			var (tween, tween2) = value;
			tween?.Kill();
			tween2?.Kill();
		}
		m_Tweens.Clear();
	}

	private void ShowTooltip(bool hover)
	{
		if (hover)
		{
			m_AbilitiesHover.ShowTooltip(base.ViewModel.GetMoraleTooltipTemplate(), m_TooltipConfig);
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void UpdateMorale()
	{
		int currentValue = base.ViewModel.MoraleValue.CurrentValue;
		MoralePhaseType currentValue2 = base.ViewModel.MoralePhase.CurrentValue;
		m_Value.text = Math.Abs(currentValue).ToString();
		MoraleColorOption moraleColorOption = GetColors(currentValue2, currentValue) ?? m_DefaultMoralePair;
		m_ValueFill.color = moraleColorOption.TextColor;
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
		UpdateSliders(moraleColorOption.Color, currentValue);
		UpdateHighlight();
		m_BarHover.SetTooltip(base.ViewModel.GetMoraleTooltipTemplate(), m_TooltipConfig).AddTo(this);
	}

	public void UpdateHighlight()
	{
		switch (base.ViewModel.MoralePhase.CurrentValue)
		{
		case MoralePhaseType.Regular:
			m_Highlight.color = Color.clear;
			m_ForceShowAbilities.Value = false;
			break;
		case MoralePhaseType.Broken:
			m_Highlight.color = m_Broken;
			m_ForceShowAbilities.Value = true;
			break;
		case MoralePhaseType.Heroic:
			m_Highlight.color = m_Heroic;
			m_ForceShowAbilities.Value = true;
			break;
		}
	}

	private void UpdateSliders(Color sliderColor, int currentValue)
	{
		bool flag = currentValue > 0;
		bool flag2 = currentValue < 0;
		m_MoralePositiveSlider.gameObject.SetActive(flag);
		m_MoraleNegativeSlider.gameObject.SetActive(flag2);
		Image[] moraleSliderFillImages = m_MoraleSliderFillImages;
		for (int i = 0; i < moraleSliderFillImages.Length; i++)
		{
			moraleSliderFillImages[i].color = sliderColor;
		}
		if (flag)
		{
			float value = (float)currentValue / (float)base.ViewModel.MaxMorale;
			m_MoralePositiveSlider.value = value;
		}
		if (flag2)
		{
			float value2 = (float)currentValue / (float)base.ViewModel.MinMorale;
			m_MoraleNegativeSlider.value = value2;
		}
	}

	private MoraleColorOption GetColors(MoralePhaseType phase, int current)
	{
		return phase switch
		{
			MoralePhaseType.Heroic => m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Heroic), 
			MoralePhaseType.Broken => m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Broken), 
			_ => m_MoralePair.FirstOrDefault((MoraleColorOption v) => v.Type == MoraleColorOptionType.Regular && v.MinValue <= current && current < v.MaxValue), 
		};
	}
}
