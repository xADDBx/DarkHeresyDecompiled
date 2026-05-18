using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharacterInfoBuffGroupVM : VirtualListElementVMBase
{
	private readonly BuffListVM<CharInfoFeatureVM> m_BuffListVM;

	public readonly string GroupTitle;

	public readonly Sprite GroupIcon;

	public ReadOnlyReactiveProperty<IReadOnlyList<CharInfoFeatureVM>> Buffs => m_BuffListVM.Buffs;

	public CharacterInfoBuffGroupVM(MechanicEntity unit, string groupTitle, Sprite groupIcon, ReadOnlyReactiveProperty<IReadOnlyList<Buff>> buffs)
	{
		m_BuffListVM = new BuffListVM<CharInfoFeatureVM>(buffs, (Buff buff) => new CharInfoFeatureVM(buff, unit)).AddTo(this);
		GroupTitle = groupTitle;
		GroupIcon = groupIcon;
	}
}
