using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffGroupsVM : TooltipBrickVM
{
	private readonly BuffGroupsVM m_BuffGroupsVM;

	private readonly IDisposable m_Disposable;

	private readonly BuffGroupFlags m_ShowFlags;

	private readonly List<BrickBuffGroupVM> m_Groups = new List<BrickBuffGroupVM>();

	public readonly string TitleText;

	public IReadOnlyList<BrickBuffGroupVM> Groups => m_Groups;

	public BrickBuffGroupsVM(MechanicEntity unit, BuffGroupFlags flags = BuffGroupFlags.All)
	{
		m_Disposable = (m_BuffGroupsVM = new BuffGroupsVM(unit));
		TitleText = UIStrings.Instance.Inspect.StatusEffectsTitle.Text;
		m_ShowFlags = flags;
		BuildBricks();
	}

	public BrickBuffGroupsVM(BuffGroupsVM buffGroupsVM, BuffGroupFlags flags = BuffGroupFlags.All)
	{
		m_BuffGroupsVM = buffGroupsVM;
		TitleText = UIStrings.Instance.Inspect.StatusEffectsTitle.Text;
		m_ShowFlags = flags;
		BuildBricks();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		m_Disposable?.Dispose();
	}

	private void BuildBricks()
	{
		if (m_ShowFlags.HasFlag(BuffGroupFlags.CriticalEffects))
		{
			AddGroup(UIStrings.Instance.Inspect.EffectsCritical.Text, UIConfig.Instance.UIIcons.CriticalEffects, m_BuffGroupsVM.CriticalEffects, UIStrings.Instance.Tooltips.CriticalEffectHint);
		}
		if (m_ShowFlags.HasFlag(BuffGroupFlags.StatusEffects))
		{
			AddGroup(UIStrings.Instance.Inspect.EffectsStatus.Text, UIConfig.Instance.UIIcons.StatusEffects, m_BuffGroupsVM.StatusEffects);
		}
		if (m_ShowFlags.HasFlag(BuffGroupFlags.DotEffects))
		{
			AddGroup(UIStrings.Instance.Inspect.EffectsDOT.Text, UIConfig.Instance.UIIcons.DotEffects, m_BuffGroupsVM.DotEffects);
		}
		if (m_ShowFlags.HasFlag(BuffGroupFlags.NegativeEffects))
		{
			AddGroup(UIStrings.Instance.Inspect.EffectsNegative.Text, UIConfig.Instance.UIIcons.NegativeEffects, m_BuffGroupsVM.NegativeEffects);
		}
		if (m_ShowFlags.HasFlag(BuffGroupFlags.PositiveEffects))
		{
			AddGroup(UIStrings.Instance.Inspect.EffectsPositive.Text, UIConfig.Instance.UIIcons.PositiveEffects, m_BuffGroupsVM.PositiveEffects);
		}
	}

	private void AddGroup(string groupName, Sprite groupIcon, ReadOnlyReactiveProperty<IReadOnlyList<Buff>> buffs, string hint = null)
	{
		if (!m_ShowFlags.HasFlag(BuffGroupFlags.HideEmptyGroup) || buffs.CurrentValue.Count >= 1)
		{
			m_Groups.Add(new BrickBuffGroupVM(groupName, groupIcon, buffs, hint));
		}
	}
}
