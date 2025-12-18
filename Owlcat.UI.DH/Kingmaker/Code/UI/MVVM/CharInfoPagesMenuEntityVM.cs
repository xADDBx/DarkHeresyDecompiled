using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPagesMenuEntityVM : SelectionGroupEntityVM
{
	public readonly string Label;

	public readonly CharInfoPageType PageType;

	public CharInfoPagesMenuEntityVM(CharInfoPageType pageType, ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(allowSwitchOff: false)
	{
		PageType = pageType;
		Label = UIStrings.Instance.CharacterSheet.GetMenuLabel(pageType);
		AddDisposable(unit?.Subscribe(UpdateState));
	}

	protected override void DisposeImplementation()
	{
	}

	protected override void DoSelectMe()
	{
	}

	private void UpdateState(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			SetAvailableState(PageType != CharInfoPageType.PsykerPowers || unit.GetOptional<PartPsyker>() != null);
		}
	}
}
