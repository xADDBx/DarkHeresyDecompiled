using System.Collections.Generic;
using Kingmaker.View.Mechadendrites;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("0c14fd969c24449eb3a6b346f6e5a4cc")]
public class ActivateMechadendrites : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		UnitPartMechadendrites orCreate = base.Owner.GetOrCreate<UnitPartMechadendrites>();
		if (orCreate != null && !(base.Owner.View == null))
		{
			MechadendriteSettings[] componentsInChildren = base.Owner.View.GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
			}
		}
	}

	protected override void OnDeactivate()
	{
		UnitPartMechadendrites optional = base.Owner.GetOptional<UnitPartMechadendrites>();
		if (optional == null || base.Owner.View == null)
		{
			return;
		}
		foreach (KeyValuePair<MechadendritesType, MechadendriteSettings> mechadendrite in optional.Mechadendrites)
		{
			optional.UnregisterMechadendrite(mechadendrite.Value);
		}
		base.Owner.View.Data?.Remove<UnitPartMechadendrites>();
	}

	protected override void OnViewDidAttach()
	{
		OnActivateOrPostLoad();
	}
}
