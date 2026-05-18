using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsBlockCommonView : CharInfoComponentWithLevelUpView<CharInfoSkillsBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterSkillsLabel;

	[Header("Containers")]
	[SerializeField]
	private Transform m_SkillContainer;

	protected List<CharInfoSkillPCView> SkillEntries;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnStatsUpdated, delegate
		{
			BindEntries();
		}).AddTo(this);
		if ((bool)m_CharacterSkillsLabel)
		{
			m_CharacterSkillsLabel.text = UIStrings.Instance.CharacterSheet.Skills;
		}
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		CreateEntries();
		BindEntries();
	}

	private void BindEntries()
	{
		List<CharInfoStatVM> sortedSkills = GetSortedSkills();
		int num = Math.Min(sortedSkills.Count, SkillEntries.Count);
		for (int i = 0; i < num; i++)
		{
			SkillEntries[i].Bind(sortedSkills[i]);
		}
	}

	private void CreateEntries()
	{
		if (SkillEntries == null || !SkillEntries.Any())
		{
			SkillEntries = new List<CharInfoSkillPCView>();
			CharInfoSkillPCView[] componentsInChildren = m_SkillContainer.GetComponentsInChildren<CharInfoSkillPCView>();
			foreach (CharInfoSkillPCView charInfoSkillPCView in componentsInChildren)
			{
				charInfoSkillPCView.Initialize();
				SkillEntries.Add(charInfoSkillPCView);
			}
		}
	}

	private List<CharInfoStatVM> GetSortedSkills()
	{
		return (base.ViewModel?.Stats.OrderBy((CharInfoStatVM s) => StatTypeHelper.DisplayOrder.IndexOf(s.StatType)))?.ToList() ?? new List<CharInfoStatVM>();
	}
}
