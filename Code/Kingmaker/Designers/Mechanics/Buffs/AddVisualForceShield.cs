using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("7f88b1ffc2c3465fa25234cf97093e7f")]
public class AddVisualForceShield : UnitBuffComponentDelegate
{
	public ShieldType ShieldType;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.ShieldType = ShieldType;
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		base.Owner.ShieldType = ShieldType.None;
		base.OnDeactivate();
	}

	protected override void OnViewDidAttach()
	{
		base.Owner.ShieldType = ShieldType;
		base.OnViewDidAttach();
	}

	protected override void OnViewWillDetach()
	{
		base.Owner.ShieldType = ShieldType.None;
		base.OnViewDidAttach();
	}
}
