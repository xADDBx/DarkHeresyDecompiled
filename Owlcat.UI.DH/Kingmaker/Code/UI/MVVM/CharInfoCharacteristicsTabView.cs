using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoCharacteristicsTabView : CharInfoComponentView<CharInfoCharacteristicsTabVM>
{
	[SerializeField]
	private List<CharInfoSkillPCView> m_SkillEntries = new List<CharInfoSkillPCView>();

	[SerializeField]
	private List<CharInfoSkillPCView> m_StatsEntries = new List<CharInfoSkillPCView>();

	[SerializeField]
	protected CharInfoStatusEffectsView m_StatusEffectsView;

	[SerializeField]
	private TextMeshProUGUI m_SkillTitle;

	[SerializeField]
	private TextMeshProUGUI m_StatsTitle;

	private CharInfoSkillsBlockVM SkillsBlockVM => base.ViewModel.CharInfoSkillsAndWeaponsVM.SkillsBlockVM;

	private CharInfoAbilityScoresBlockVM StatsBlockVM => base.ViewModel.CharInfoSkillsAndWeaponsVM.AbilityScoresBlockVM;

	public override void Initialize()
	{
		base.Initialize();
		m_StatsTitle.text = UIStrings.Instance.CharacterSheet.Stats;
		m_SkillTitle.text = UIStrings.Instance.CharacterSheet.Skills;
		m_StatusEffectsView.Initialize();
		m_SkillEntries.ForEach(delegate(CharInfoSkillPCView e)
		{
			e.Initialize();
		});
		m_StatsEntries.ForEach(delegate(CharInfoSkillPCView e)
		{
			e.Initialize();
		});
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_StatusEffectsView.Or(null)?.Bind(base.ViewModel.StatusEffects.Value);
		ObservableSubscribeExtensions.Subscribe(SkillsBlockVM.OnStatsUpdated.Prepend(Unit.Default), delegate
		{
			BindEntries(m_SkillEntries, SkillsBlockVM);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(StatsBlockVM.OnStatsUpdated.Prepend(Unit.Default), delegate
		{
			BindEntries(m_StatsEntries, StatsBlockVM);
		}).AddTo(this);
	}

	private void BindEntries(List<CharInfoSkillPCView> entries, CharInfoBaseAbilityScoresBlockVM scoresVM)
	{
		List<CharInfoStatVM> sortedSkills = GetSortedSkills(scoresVM);
		int num = Math.Min(sortedSkills.Count, entries.Count);
		for (int i = 0; i < num; i++)
		{
			entries[i].Bind(sortedSkills[i]);
		}
	}

	private List<CharInfoStatVM> GetSortedSkills(CharInfoBaseAbilityScoresBlockVM scoresVM)
	{
		return (scoresVM?.Stats.OrderBy((CharInfoStatVM s) => StatTypeHelper.DisplayOrder.IndexOf(s.StatType)))?.ToList() ?? new List<CharInfoStatVM>();
	}
}
