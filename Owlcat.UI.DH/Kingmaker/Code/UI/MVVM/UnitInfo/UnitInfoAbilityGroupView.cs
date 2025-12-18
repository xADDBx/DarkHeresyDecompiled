using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoAbilityGroupView : UnitInfoPartGroupView
{
	[SerializeField]
	private AbilityUIGroup m_Group;

	public AbilityUIGroup Group => m_Group;
}
