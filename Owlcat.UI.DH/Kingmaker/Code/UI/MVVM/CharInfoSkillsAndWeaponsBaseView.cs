using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharInfoSkillsAndWeaponsBaseView : CharInfoComponentView<CharInfoSkillsAndWeaponsVM>
{
	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_SkillsBlockPCView;

	[SerializeField]
	protected CharInfoWeaponsBlockPCView m_WeaponsBlockPCView;

	[Header("Localization")]
	[SerializeField]
	private TextMeshProUGUI m_WeaponStatsLabel;

	[SerializeField]
	private TextMeshProUGUI m_SkillsStatsLabel;

	protected Dictionary<CharInfoComponentType, ICharInfoComponentView> TypeToView;

	protected readonly ReactiveProperty<CharInfoComponentType> CurrentSection = new ReactiveProperty<CharInfoComponentType>(CharInfoComponentType.Skills);

	public override void Initialize()
	{
		base.Initialize();
		m_SkillsBlockPCView.Initialize();
		m_WeaponsBlockPCView.Initialize();
		TypeToView = new Dictionary<CharInfoComponentType, ICharInfoComponentView>
		{
			{
				CharInfoComponentType.Skills,
				m_SkillsBlockPCView
			},
			{
				CharInfoComponentType.Weapons,
				m_WeaponsBlockPCView
			}
		};
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetupLabels();
		CurrentSection.Subscribe(InternalBindSection).AddTo(this);
	}

	private void SetupLabels()
	{
		if ((bool)m_WeaponStatsLabel)
		{
			m_WeaponStatsLabel.text = UIStrings.Instance.CharacterSheet.Weapons;
		}
		if ((bool)m_SkillsStatsLabel)
		{
			m_SkillsStatsLabel.text = UIStrings.Instance.CharacterSheet.Skills;
		}
	}

	protected abstract void InternalBindSection(CharInfoComponentType section);
}
