using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Buffs;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffGroupVM : TooltipBrickVM
{
	private readonly BuffListVM<BrickBuffVM> m_BuffListVM;

	public readonly string GroupName;

	public readonly Sprite GroupIcon;

	public readonly string GroupHint;

	public readonly string NoEffectsText;

	public ReadOnlyReactiveProperty<IReadOnlyList<BrickBuffVM>> Buffs => m_BuffListVM.Buffs;

	public BrickBuffGroupVM(string groupName, Sprite groupIcon, ReadOnlyReactiveProperty<IReadOnlyList<Buff>> buffs, string groupHint = null)
	{
		m_BuffListVM = new BuffListVM<BrickBuffVM>(buffs, (Buff buff) => new BrickBuffVM(buff)).AddTo(this);
		GroupName = groupName;
		GroupIcon = groupIcon;
		GroupHint = groupHint;
		NoEffectsText = "[ " + UIStrings.Instance.Inspect.NoStatusEffects.Text + " ]";
	}
}
