using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoNameAndPortraitBaseView : CharInfoComponentView<CharInfoNameAndPortraitVM>
{
	private enum PortraitSize
	{
		Small,
		Middle,
		Full
	}

	private enum TabButtonName
	{
		Stats,
		Skills,
		Offence
	}

	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private CharInfoHitPointsPCView m_HitPointsView;

	[SerializeField]
	private PortraitSize m_Size;

	[Header("Animators")]
	[SerializeField]
	protected MoveAnimator m_PaperMoveAnimator;

	[SerializeField]
	protected FadeAnimator m_PaperFadeAnimator;

	[Header("Tab views")]
	[SerializeField]
	protected CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_SkillsBlockPCView;

	[SerializeField]
	protected CharInfoWeaponsBlockPCView m_WeaponsBlockPCView;

	[Header("Skills blocks characteristics")]
	[SerializeField]
	private CharInfoPredefinedAbilityScoresBaseView m_AbilityScores;

	[Header("Char Info Tab Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_StatsButton;

	[SerializeField]
	private OwlcatMultiButton m_SkillsButton;

	[SerializeField]
	private OwlcatMultiButton m_OffenceButton;

	[Header("Tabs")]
	[SerializeField]
	private Transform m_StatsGroup;

	[SerializeField]
	private Transform m_SkillsGroup;

	[SerializeField]
	private Transform m_OffenceGroup;

	[Header("Tabs Labels")]
	[SerializeField]
	private TextMeshProUGUI m_StatsLabel;

	[SerializeField]
	private TextMeshProUGUI m_SkillsLabel;

	[SerializeField]
	private TextMeshProUGUI m_OffenceLabel;

	[SerializeField]
	private TextMeshProUGUI m_CharacteristicsLabel;

	[Header("Bottoms block labels")]
	[SerializeField]
	private TextMeshProUGUI m_DefenceLabel;

	[SerializeField]
	private TextMeshProUGUI m_MeleeLabel;

	[SerializeField]
	private TextMeshProUGUI m_MeleeValueLabel;

	[SerializeField]
	private TextMeshProUGUI m_RangedLabel;

	[SerializeField]
	private TextMeshProUGUI m_RangedValueLabel;

	[SerializeField]
	private TextMeshProUGUI m_DamageReductionLabel;

	[SerializeField]
	private TextMeshProUGUI m_DamageReductionValueLabel;

	[SerializeField]
	private TextMeshProUGUI m_HealthLabel;

	[SerializeField]
	private TextMeshProUGUI m_ArmourLabel;

	[Header("State Blocks")]
	[SerializeField]
	private CharInfoBuffGroupsView m_BuffGroupsView;

	[Header("Career")]
	[SerializeField]
	private TextMeshProUGUI m_FirstCareerLabel;

	[SerializeField]
	private TextMeshProUGUI m_FirstCareerArchetypeLabel;

	[SerializeField]
	private TextMeshProUGUI m_SecondCareerLabel;

	[SerializeField]
	private TextMeshProUGUI m_SecondCareerArchetypeLabel;

	[SerializeField]
	private TextMeshProUGUI m_WeaponSet;

	[SerializeField]
	private TextMeshProUGUI m_WeaponSet2;

	[SerializeField]
	private Image m_FirstCareerImage;

	[SerializeField]
	private Image m_SecondCareerImage;

	[SerializeField]
	private OwlcatMultiSelectable m_FirstCareerTooltip;

	[SerializeField]
	private OwlcatMultiSelectable m_SecondCareerTooltip;

	[SerializeField]
	private CharInfoSummaryBaseView m_SummaryView;

	private bool m_IsShown;

	public void Awake()
	{
		m_PaperMoveAnimator.Initialize();
		m_PaperFadeAnimator.Initialize();
		m_LevelClassScoresView.Initialize();
		m_SkillsBlockPCView.Initialize();
		m_WeaponsBlockPCView.Initialize();
		m_AbilityScores.Or(null)?.Initialize();
		m_SummaryView.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_BuffGroupsView.Bind(base.ViewModel.CharInfoBuffGroupsVM);
		base.ViewModel.UnitName.Subscribe(delegate
		{
			SetName();
		}).AddTo(this);
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevCharacter.name, base.ViewModel.SelectPrevCharacter).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextCharacter.name, base.ViewModel.SelectNextCharacter).AddTo(this);
		base.ViewModel.SummaryVM.Subscribe(m_SummaryView.Bind).AddTo(this);
		m_StatsLabel.text = UIStrings.Instance.CharacterSheet.StatsLabel;
		m_SkillsLabel.text = UIStrings.Instance.CharacterSheet.Skills;
		m_OffenceLabel.text = UIStrings.Instance.CharacterSheet.OffenceLabel;
		m_DefenceLabel.text = UIStrings.Instance.Tooltips.Defence;
		m_CharacteristicsLabel.text = UIStrings.Instance.CharacterSheet.Stats;
		m_MeleeLabel.text = UIStrings.Instance.CharacterSheet.MeleeLabel;
		m_RangedLabel.text = UIStrings.Instance.CharacterSheet.RangedLabel;
		m_DamageReductionLabel.text = UIStrings.Instance.CharacterSheet.DamageReductionLabel;
		m_FirstCareerArchetypeLabel.text = UIStrings.Instance.CharacterSheet.CareerPathHeader;
		m_SecondCareerArchetypeLabel.text = UIStrings.Instance.CharacterSheet.CareerPathHeader;
		m_HealthLabel.text = UIStrings.Instance.Inspect.Wounds;
		m_ArmourLabel.text = UIStrings.Instance.CharacterSheet.ArmorAbsorption;
		m_WeaponSet.text = UIStrings.Instance.CharacterSheet.WeaponSet;
		m_WeaponSet2.text = UIStrings.Instance.CharacterSheet.WeaponSet;
		base.ViewModel.MeleeValue.Subscribe(delegate(int value)
		{
			m_MeleeValueLabel.text = value.ToString();
		}).AddTo(this);
		base.ViewModel.RangedValue.Subscribe(delegate(int value)
		{
			m_RangedValueLabel.text = value.ToString();
		}).AddTo(this);
		base.ViewModel.DamageReductionValue.Subscribe(delegate(int value)
		{
			m_DamageReductionValueLabel.text = value.ToString();
		}).AddTo(this);
		base.ViewModel.FirstCareer.Subscribe(delegate(CareerPathVM value)
		{
			if (value != null)
			{
				m_FirstCareerLabel.text = value.Name;
				m_FirstCareerImage.sprite = value.Icon?.CurrentValue;
				m_FirstCareerImage.gameObject.SetActive(value: true);
			}
			else
			{
				m_FirstCareerLabel.text = "---/ ? /---";
				m_FirstCareerImage.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		base.ViewModel.SecondCareer.Subscribe(delegate(CareerPathVM value)
		{
			if (value != null)
			{
				m_SecondCareerLabel.text = value.Name;
				m_SecondCareerImage.sprite = value.Icon?.CurrentValue;
				m_SecondCareerImage.gameObject.SetActive(value: true);
			}
			else
			{
				m_SecondCareerLabel.text = "---/ ? /---";
				m_SecondCareerImage.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		m_LevelClassScoresView.Bind(base.ViewModel.LevelClassScoresVM);
		m_AbilityScores.Or(null)?.Bind(base.ViewModel.AbilityScores);
		ObservableSubscribeExtensions.Subscribe(m_StatsButton.OnLeftClickAsObservable(), delegate
		{
			ChangeTab(TabButtonName.Stats);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SkillsButton.OnLeftClickAsObservable(), delegate
		{
			ChangeTab(TabButtonName.Skills);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_OffenceButton.OnLeftClickAsObservable(), delegate
		{
			ChangeTab(TabButtonName.Offence);
		}).AddTo(this);
		SetupSoundTypes();
		ChangeTab(TabButtonName.Stats);
	}

	protected override void OnUnbind()
	{
		m_BuffGroupsView.Unbind();
		base.OnUnbind();
	}

	private void SetupSoundTypes()
	{
		foreach (OwlcatMultiButton item in new List<OwlcatMultiButton> { m_StatsButton, m_SkillsButton, m_OffenceButton })
		{
			UISounds.Instance.SetHoverSound(item, ButtonSoundsEnum.PaperSound);
			UISounds.Instance.SetClickSound(item, ButtonSoundsEnum.NoSound);
		}
	}

	private void ChangeTab(TabButtonName tabButton)
	{
		m_StatsGroup.gameObject.SetActive(tabButton == TabButtonName.Stats);
		m_SkillsGroup.gameObject.SetActive(tabButton == TabButtonName.Skills);
		m_OffenceGroup.gameObject.SetActive(tabButton == TabButtonName.Offence);
		switch (tabButton)
		{
		case TabButtonName.Stats:
			m_StatsButton.SetActiveLayer("Focused");
			m_SkillsButton.SetActiveLayer("Normal");
			m_OffenceButton.SetActiveLayer("Normal");
			break;
		case TabButtonName.Skills:
			m_WeaponsBlockPCView.UnbindSection();
			m_SkillsBlockPCView.BindSection(base.ViewModel.CharInfoSkillsAndWeaponsVM.SkillsBlockVM);
			m_StatsButton.SetActiveLayer("Normal");
			m_SkillsButton.SetActiveLayer("Focused");
			m_OffenceButton.SetActiveLayer("Normal");
			break;
		case TabButtonName.Offence:
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.BindSection(base.ViewModel.CharInfoSkillsAndWeaponsVM.WeaponsBlockVM);
			m_StatsButton.SetActiveLayer("Normal");
			m_SkillsButton.SetActiveLayer("Normal");
			m_OffenceButton.SetActiveLayer("Focused");
			break;
		}
		RefreshView();
		ServiceWindowsSounds.Instance.Character.PaperTabChanged.Play();
	}

	protected override void RefreshView()
	{
		SetName();
		SetPortrait();
		SetHP();
		SetTooltips();
	}

	private void SetName()
	{
		string currentValue = base.ViewModel.UnitName.CurrentValue;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != currentValue)
		{
			m_NameFieldScrambled.SetText(string.Empty, currentValue);
		}
	}

	private void SetPortrait()
	{
		switch (m_Size)
		{
		case PortraitSize.Small:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitSmall);
			break;
		case PortraitSize.Middle:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitHalf);
			break;
		case PortraitSize.Full:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitFull);
			break;
		}
	}

	private void SetHP()
	{
		m_HitPointsView.Or(null)?.Bind(base.ViewModel.HitPoints);
	}

	private void SetTooltips()
	{
		if (base.ViewModel.FirstCareer.CurrentValue != null)
		{
			m_FirstCareerTooltip.SetTooltip(base.ViewModel.FirstCareer.CurrentValue.CareerTooltip).AddTo(this);
		}
		if (base.ViewModel.SecondCareer.CurrentValue != null)
		{
			m_SecondCareerTooltip.SetTooltip(base.ViewModel.SecondCareer.CurrentValue.CareerTooltip).AddTo(this);
		}
	}

	protected override void OnShow()
	{
		if (!m_IsShown)
		{
			m_IsShown = true;
			base.gameObject.SetActive(value: true);
			m_PaperMoveAnimator.AppearAnimation();
			m_PaperFadeAnimator.AppearAnimation();
		}
	}

	protected override void OnHide(UnityAction onHideCallback = null)
	{
		if (m_IsShown)
		{
			m_IsShown = false;
			ServiceWindowsSounds.Instance.Character.StatsHide.Play();
			m_PaperMoveAnimator.DisappearAnimation();
			m_PaperFadeAnimator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
			});
			m_Portrait.Dispose();
		}
	}
}
