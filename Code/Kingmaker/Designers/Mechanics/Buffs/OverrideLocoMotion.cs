using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("151c0db8bb630d045ade4bbc9932b176")]
public class OverrideLocoMotion : UnitBuffComponentDelegate
{
	public UnitAnimationActionLocomotion Locomotion;

	protected override void OnActivate()
	{
		base.OnActivate();
		TryOverride();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		TryOverride();
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		base.Owner.View?.AnimationManager?.ResetLocoMotion();
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		TryOverride();
	}

	private void TryOverride()
	{
		UnitAnimationManager unitAnimationManager = base.Owner?.View?.AnimationManager;
		if (unitAnimationManager != null && unitAnimationManager.LocoMotionHandle?.Action != Locomotion)
		{
			unitAnimationManager.ResetLocoMotion();
			unitAnimationManager.ChangeLocoMotion(Locomotion);
		}
	}
}
