using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("Animation/PlayLoopAnimationByBuff")]
[TypeId("ccdeb99837c64fb79ebc26eb36f2f47b")]
public class PlayLoopAnimationByBuff : UnitBuffComponentDelegate, ICustomLoopActionTypeProvider
{
	[SerializeField]
	private BlueprintCustomLoopActionType.Reference m_Type;

	BlueprintCustomLoopActionType ICustomLoopActionTypeProvider.Type => m_Type.Blueprint;

	protected override void OnActivateOrPostLoad()
	{
		PlayAnimation();
	}

	protected override void OnDeactivate()
	{
		StopAnimation();
	}

	protected override void OnViewDidAttach()
	{
		PlayAnimation();
	}

	protected override void OnViewWillDetach()
	{
		StopAnimation();
	}

	private void PlayAnimation()
	{
		base.Owner.GetOrCreate<PartBuffAnimation>().PlayAnimation(this);
	}

	private void StopAnimation()
	{
		base.Owner.GetOptional<PartBuffAnimation>()?.StopAnimation(this);
	}
}
