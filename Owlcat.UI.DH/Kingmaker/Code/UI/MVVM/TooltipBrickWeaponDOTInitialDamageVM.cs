using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponDOTInitialDamageVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Name;

	public readonly string Damage;

	private readonly BlueprintBuff m_Buff;

	public TooltipBrickWeaponDOTInitialDamageVM(BlueprintBuff buff, int initialDamage)
	{
		m_Buff = buff;
		Icon = m_Buff?.Icon;
		Name = ((m_Buff != null) ? ((string)m_Buff.LocalizedName) : string.Empty);
		Damage = initialDamage.ToString();
	}
}
