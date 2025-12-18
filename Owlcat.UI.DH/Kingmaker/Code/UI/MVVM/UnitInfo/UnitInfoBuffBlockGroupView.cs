using Kingmaker.UnitLogic.Buffs.Components;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBuffBlockGroupView : UnitInfoPartGroupView
{
	[SerializeField]
	private BuffGroupType m_Group;

	public BuffGroupType Group => m_Group;
}
