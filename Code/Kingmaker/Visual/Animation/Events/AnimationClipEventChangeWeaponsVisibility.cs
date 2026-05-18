namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventChangeWeaponsVisibility : AnimationClipEvent
{
	public bool IsActive;

	public bool IsOffHandWeaponActive;

	public AnimationClipEventChangeWeaponsVisibility(float time)
		: base(time)
	{
	}

	private AnimationClipEventChangeWeaponsVisibility(float time, bool isActive, bool isOffHandWeaponActive)
		: base(time)
	{
		IsActive = isActive;
		IsOffHandWeaponActive = isOffHandWeaponActive;
	}

	public override void Start(IAnimationManager animationManager)
	{
		animationManager.CallbackReceiver.ChangeShowingWeapon(IsActive, IsOffHandWeaponActive);
	}

	public override object Clone()
	{
		return new AnimationClipEventChangeWeaponsVisibility(base.Time, IsActive, IsOffHandWeaponActive);
	}

	public override string ToString()
	{
		return $"Changing the display of weapons at {base.Time}";
	}
}
