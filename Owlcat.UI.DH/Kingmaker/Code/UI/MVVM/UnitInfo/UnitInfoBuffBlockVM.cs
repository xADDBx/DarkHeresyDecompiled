using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBuffBlockVM : ViewModel
{
	public readonly UnitBuffBlockVM BuffBlockVM;

	public readonly BuffListVM<BuffVM> CriticalEffects;

	public readonly BuffListVM<BuffVM> StatusEffects;

	public readonly BuffListVM<BuffVM> DOTEffects;

	public readonly BuffListVM<BuffVM> NegativeEffects;

	public readonly BuffListVM<BuffVM> PositiveEffects;

	public readonly string CriticalGroupHeader;

	public readonly string StatusGroupHeader;

	public readonly string DOTGroupHeader;

	public readonly string NegativeGroupHeader;

	public readonly string PositiveGroupHeader;

	public bool HasBuffs => GetBuffsCount() > 0;

	public UnitInfoBuffBlockVM(MechanicEntity unit)
	{
		UIInspect inspect = UIStrings.Instance.Inspect;
		CriticalGroupHeader = inspect.EffectsCritical;
		StatusGroupHeader = inspect.EffectsStatus;
		DOTGroupHeader = inspect.EffectsDOT;
		NegativeGroupHeader = inspect.EffectsNegative;
		PositiveGroupHeader = inspect.EffectsPositive;
		BuffBlockVM = new UnitBuffBlockVM(unit).AddTo(this);
		BuffGroupsVM buffGroupsVM = new BuffGroupsVM(BuffBlockVM.Buffs).AddTo(this);
		CriticalEffects = new BuffListVM<BuffVM>(buffGroupsVM.CriticalEffects, (Buff buff) => new BuffVM(buff)).AddTo(this);
		StatusEffects = new BuffListVM<BuffVM>(buffGroupsVM.StatusEffects, (Buff buff) => new BuffVM(buff)).AddTo(this);
		DOTEffects = new BuffListVM<BuffVM>(buffGroupsVM.DotEffects, (Buff buff) => new BuffVM(buff)).AddTo(this);
		NegativeEffects = new BuffListVM<BuffVM>(buffGroupsVM.NegativeEffects, (Buff buff) => new BuffVM(buff)).AddTo(this);
		PositiveEffects = new BuffListVM<BuffVM>(buffGroupsVM.PositiveEffects, (Buff buff) => new BuffVM(buff)).AddTo(this);
	}

	public void SetUnitData(MechanicEntity unit)
	{
		BuffBlockVM.SetUnitData(unit);
	}

	private int GetBuffsCount()
	{
		return CriticalEffects.Count + StatusEffects.Count + DOTEffects.Count + NegativeEffects.Count + PositiveEffects.Count;
	}
}
