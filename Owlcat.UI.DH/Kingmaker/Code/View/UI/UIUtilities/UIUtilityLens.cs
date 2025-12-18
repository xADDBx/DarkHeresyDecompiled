using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityLens
{
	public static void MoveLensPosition(Transform lens, Vector3 target, float duration, bool withSound = true)
	{
		lens.DOLocalMove(target, duration).OnStart(delegate
		{
			if (withSound)
			{
				UIUtilitySound.PlaySelectorSound();
			}
		}).OnComplete(delegate
		{
			if (withSound)
			{
				UIUtilitySound.StopSelectorSound();
			}
		})
			.SetUpdate(isIndependentUpdate: true);
	}

	public static void MoveXLensPosition(Transform lens, float target, float duration, bool withSound = true)
	{
		lens.DOLocalMoveX(target, duration).OnStart(delegate
		{
			if (withSound)
			{
				UIUtilitySound.PlaySelectorSound();
			}
		}).OnComplete(delegate
		{
			if (withSound)
			{
				UIUtilitySound.StopSelectorSound();
			}
		})
			.SetUpdate(isIndependentUpdate: true);
	}
}
