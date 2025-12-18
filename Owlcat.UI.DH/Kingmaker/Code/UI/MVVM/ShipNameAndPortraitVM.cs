using Kingmaker.EntitySystem.Entities;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ShipNameAndPortraitVM : CharInfoComponentVM
{
	public Sprite StarShipImage => null;

	public string StarShipName => string.Empty;

	public string StarShipDescription => string.Empty;

	public ShipNameAndPortraitVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}
}
