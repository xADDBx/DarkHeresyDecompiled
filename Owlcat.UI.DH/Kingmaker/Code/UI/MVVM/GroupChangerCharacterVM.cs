using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GroupChangerCharacterVM : ViewModel
{
	private readonly ReactiveCommand<GroupChangerCharacterVM> m_Click = new ReactiveCommand<GroupChangerCharacterVM>();

	private readonly ReactiveProperty<bool> m_IsInParty = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsLock = new ReactiveProperty<bool>(value: false);

	public readonly BlueprintUnit BpUnit;

	public readonly UnitReference UnitRef;

	public readonly UnitHealthPartVM HealthPartVm;

	public readonly UnitBuffBlockVM BuffBlockVM;

	public readonly UnitPortraitPartVM PortraitPartVm;

	public readonly bool IsRemovable;

	public readonly bool IsLevelUp;

	public readonly bool IsCharacterOverload;

	public readonly bool IsPartyOverload;

	public readonly string CharacterName;

	public readonly int CharacterLevel;

	public readonly Encumbrance CharacterEncumbrance;

	private bool m_IsFocused;

	public Observable<GroupChangerCharacterVM> Click => m_Click;

	public ReadOnlyReactiveProperty<bool> IsInParty => m_IsInParty;

	public ReadOnlyReactiveProperty<bool> IsLock => m_IsLock;

	public bool IsFocused => m_IsFocused;

	public GroupChangerCharacterVM(UnitReference unit, bool isLock)
	{
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)unit.Entity;
		BpUnit = baseUnitEntity.Blueprint;
		UnitRef = unit;
		m_IsLock.Value = isLock;
		IsRemovable = baseUnitEntity.GetCompanionOptional()?.CanRemoveFromParty ?? false;
		IsLevelUp = baseUnitEntity.Progression.CanLevelUp;
		CharacterEncumbrance = baseUnitEntity.EncumbranceData?.Value ?? Encumbrance.Light;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		Encumbrance encumbrance = ((loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) ? Game.Instance.Player.Encumbrance : Encumbrance.Light);
		IsCharacterOverload = CharacterEncumbrance == Encumbrance.Overload;
		AreaPersistentState loadedAreaState2 = Game.Instance.LoadedAreaState;
		IsPartyOverload = (loadedAreaState2 == null || !loadedAreaState2.Settings.CapitalPartyMode) && encumbrance == Encumbrance.Overload;
		CharacterName = baseUnitEntity.CharacterName;
		CharacterLevel = baseUnitEntity.Progression.CharacterLevel;
		HealthPartVm = new UnitHealthPartVM(baseUnitEntity).AddTo(this);
		BuffBlockVM = new UnitBuffBlockVM(baseUnitEntity).AddTo(this);
		PortraitPartVm = new UnitPortraitPartVM().AddTo(this);
		BuffBlockVM.SetUnitData(baseUnitEntity);
		PortraitPartVm.SetUnitData(baseUnitEntity);
	}

	public void OnClick()
	{
		m_Click.Execute(this);
	}

	public void SetIsInParty(bool isInParty)
	{
		m_IsInParty.Value = isInParty;
	}

	public void SetFocused(bool isFocused)
	{
		m_IsFocused = isFocused;
	}
}
