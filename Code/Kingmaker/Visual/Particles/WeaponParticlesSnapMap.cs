using UnityEngine;

namespace Kingmaker.Visual.Particles;

[DisallowMultipleComponent]
public class WeaponParticlesSnapMap : SnapMapBase
{
	public enum WeaponSlot
	{
		Unknown = -1,
		PrimaryHand,
		SecondaryHand,
		Additional1,
		Additional2,
		Additional3,
		Additional4,
		Additional5,
		Additional6,
		Additional7,
		Additional8
	}

	public WeaponSlot Slot;

	protected override void OnInitialize()
	{
	}

	private void Start()
	{
		if (!base.Initialized)
		{
			Init();
		}
	}
}
