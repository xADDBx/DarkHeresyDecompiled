using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatusEffectsVM : CharInfoComponentVM
{
	public CharInfoFeatureGroupVM BuffsGroup;

	public bool NoBuffs
	{
		get
		{
			CharInfoFeatureGroupVM buffsGroup = BuffsGroup;
			if (buffsGroup == null)
			{
				return false;
			}
			return buffsGroup.FeatureList.Count == 0;
		}
	}

	public CharInfoStatusEffectsVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		List<CharInfoFeatureVM> buffs = new List<CharInfoFeatureVM>();
		buffs = ExtractBuffs(Unit.CurrentValue, buffs);
		BuffsGroup = new CharInfoFeatureGroupVM(buffs).AddTo(this);
	}

	private List<CharInfoFeatureVM> ExtractBuffs(BaseUnitEntity unit, List<CharInfoFeatureVM> buffs)
	{
		List<Buff> unitBuffs = GetUnitBuffs(unit);
		buffs.AddRange(unitBuffs.Select((Buff bf) => new CharInfoFeatureVM(bf, unit)));
		return buffs;
	}

	private List<Buff> GetUnitBuffs(BaseUnitEntity unit)
	{
		return unit.Buffs.Enumerable.Where((Buff b) => !b.Blueprint.IsHiddenInUI).ToList();
	}
}
