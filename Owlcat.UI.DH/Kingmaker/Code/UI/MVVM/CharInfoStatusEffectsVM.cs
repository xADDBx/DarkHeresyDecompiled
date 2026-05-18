using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatusEffectsVM : CharInfoComponentVM
{
	private readonly BuffGroupsVM m_BuffGroups;

	private readonly List<CharacterInfoBuffGroupVM> m_BuffGroupsList = new List<CharacterInfoBuffGroupVM>();

	public readonly string StatusEffectsTitleText;

	public readonly string NoEffectsText;

	public bool NoBuffs => m_BuffGroupsList.Count < 1;

	public IEnumerable<CharacterInfoBuffGroupVM> BuffGroups => m_BuffGroupsList;

	public CharInfoStatusEffectsVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, BuffGroupsVM buffGroupsVM)
		: base(unit)
	{
		m_BuffGroups = buffGroupsVM;
		StatusEffectsTitleText = UIStrings.Instance.CharacterSheet.StatusEffects;
		NoEffectsText = UIStrings.Instance.CharacterSheet.NoBuffText;
		CollectBuffGroups();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		CollectBuffGroups();
	}

	private void CollectBuffGroups()
	{
		m_BuffGroupsList.Clear();
		if (m_BuffGroups != null)
		{
			if (m_BuffGroups.CriticalEffects.CurrentValue.Count > 0)
			{
				m_BuffGroupsList.Add(new CharacterInfoBuffGroupVM(Unit.CurrentValue, UIStrings.Instance.Inspect.EffectsCritical, UIConfig.Instance.UIIcons.CriticalEffects, m_BuffGroups.CriticalEffects));
			}
			if (m_BuffGroups.StatusEffects.CurrentValue.Count > 0)
			{
				m_BuffGroupsList.Add(new CharacterInfoBuffGroupVM(Unit.CurrentValue, UIStrings.Instance.Inspect.EffectsStatus, UIConfig.Instance.UIIcons.StatusEffects, m_BuffGroups.StatusEffects));
			}
			if (m_BuffGroups.DotEffects.CurrentValue.Count > 0)
			{
				m_BuffGroupsList.Add(new CharacterInfoBuffGroupVM(Unit.CurrentValue, UIStrings.Instance.Inspect.EffectsDOT, UIConfig.Instance.UIIcons.DotEffects, m_BuffGroups.DotEffects));
			}
			if (m_BuffGroups.NegativeEffects.CurrentValue.Count > 0)
			{
				m_BuffGroupsList.Add(new CharacterInfoBuffGroupVM(Unit.CurrentValue, UIStrings.Instance.Inspect.EffectsNegative, UIConfig.Instance.UIIcons.NegativeEffects, m_BuffGroups.NegativeEffects));
			}
			if (m_BuffGroups.PositiveEffects.CurrentValue.Count > 0)
			{
				m_BuffGroupsList.Add(new CharacterInfoBuffGroupVM(Unit.CurrentValue, UIStrings.Instance.Inspect.EffectsPositive, UIConfig.Instance.UIIcons.PositiveEffects, m_BuffGroups.PositiveEffects));
			}
		}
	}
}
