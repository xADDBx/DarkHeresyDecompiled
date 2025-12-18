using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryDollAdditionalStatsVM : CharInfoComponentWithLevelUpVM, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>
{
	private readonly ReactiveProperty<string> m_ArmorDeflection = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_ArmorAbsorption = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Dodge = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_DodgeReduction = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Resolve = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Parry = new ReactiveProperty<string>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DeflectionTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_AbsorptionTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DodgeTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ReadOnlyReactiveProperty<string> ArmorDeflection => m_ArmorDeflection;

	public ReadOnlyReactiveProperty<string> ArmorAbsorption => m_ArmorAbsorption;

	public ReadOnlyReactiveProperty<string> Dodge => m_Dodge;

	public ReadOnlyReactiveProperty<string> DodgeReduction => m_DodgeReduction;

	public ReadOnlyReactiveProperty<string> Resolve => m_Resolve;

	public ReadOnlyReactiveProperty<string> Parry => m_Parry;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DeflectionTooltip => m_DeflectionTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> AbsorptionTooltip => m_AbsorptionTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DodgeTooltip => m_DodgeTooltip;

	private ArmorSlot ArmorSlot => Unit.CurrentValue?.Body.Armor;

	public InventoryDollAdditionalStatsVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
		base.PreviewUnit.Subscribe(delegate
		{
			HandleUpdatePreviewUnit();
		}).AddTo(this);
	}

	private void HandleUpdatePreviewUnit()
	{
		UpdateData();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		if (base.PreviewUnit.CurrentValue is UnitEntity)
		{
			m_DeflectionTooltip.Value = new TooltipTemplateHint("Obsolete");
			m_AbsorptionTooltip.Value = new TooltipTemplateHint("Obsolete");
			m_ArmorDeflection.Value = "Obsolete";
			m_ArmorAbsorption.Value = "Obsolete";
			m_Dodge.Value = "Obsolete";
			m_DodgeTooltip.Value = new TooltipTemplateHint("Obsolete");
			m_DodgeReduction.Value = "Obsolete";
			ModifiableValue statOptional = Unit.CurrentValue.GetStatOptional(StatType.Resolve);
			m_Resolve.Value = ((statOptional != null) ? $"{statOptional.ModifiedValue}" : "—");
			m_Parry.Value = "Obsolete";
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (!Unit.CurrentValue.IsDisposed && slot == ArmorSlot)
		{
			RefreshData();
		}
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		RefreshData();
	}

	public override void HandleUICommitChanges()
	{
		base.HandleUICommitChanges();
		UpdateData();
	}
}
