using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsAndWeaponsConsoleView : CharInfoSkillsAndWeaponsBaseView
{
	[Header("Console")]
	[SerializeField]
	protected CharInfoAbilityScoresBlockConsoleView m_AbilityScoresBlockConsoleView;

	private Action<IConsoleEntity> m_OnFocusChangeAction;

	private readonly List<CharInfoComponentType> m_ViewsOrder = new List<CharInfoComponentType>
	{
		CharInfoComponentType.Abilities,
		CharInfoComponentType.Skills,
		CharInfoComponentType.Weapons
	};

	public override void Initialize()
	{
		base.Initialize();
		m_AbilityScoresBlockConsoleView.Initialize();
		TypeToView.Add(CharInfoComponentType.Abilities, m_AbilityScoresBlockConsoleView);
	}

	protected override void OnBind()
	{
		base.OnBind();
		CurrentSection.Value = m_ViewsOrder.First();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_OnFocusChangeAction = null;
	}

	private void OnNext()
	{
		int index = (m_ViewsOrder.IndexOf(CurrentSection.Value) + 1) % m_ViewsOrder.Count;
		CurrentSection.Value = m_ViewsOrder[index];
	}

	protected override void InternalBindSection(CharInfoComponentType section)
	{
		switch (section)
		{
		case CharInfoComponentType.Abilities:
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.UnbindSection();
			m_AbilityScoresBlockConsoleView.BindSection(base.ViewModel.AbilityScoresBlockVM);
			break;
		case CharInfoComponentType.Skills:
			m_AbilityScoresBlockConsoleView.UnbindSection();
			m_WeaponsBlockPCView.UnbindSection();
			m_SkillsBlockPCView.BindSection(base.ViewModel.SkillsBlockVM);
			break;
		case CharInfoComponentType.Weapons:
			m_AbilityScoresBlockConsoleView.UnbindSection();
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.BindSection(base.ViewModel.WeaponsBlockVM);
			break;
		default:
			throw new ArgumentOutOfRangeException("section", section, null);
		}
	}

	public CompositeDisposable AddInput()
	{
		return new CompositeDisposable();
	}

	public void SetFocusChangeAction(Action<IConsoleEntity> onFocusChange)
	{
		m_OnFocusChangeAction = onFocusChange;
	}
}
