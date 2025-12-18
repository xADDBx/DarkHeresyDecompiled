using System;
using System.Collections.Generic;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.ForcedCulling;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>
	{
		typeof(EquipmentOffsets),
		typeof(Billboard),
		typeof(Character),
		typeof(StandardMaterialController),
		typeof(ForcedCullingService),
		typeof(FxFadeOut),
		typeof(ParticlesMaterialController),
		typeof(UnitFxVisibilityManager)
	};

	public static readonly List<Type> ILateUpdateables = new List<Type>
	{
		typeof(EntityFader),
		typeof(AbstractUnitEntityView.LateUpdateDriver),
		typeof(SkeletonUpdateService)
	};
}
