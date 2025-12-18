namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseOvertipMapObjectView : BaseOvertipView<OvertipMapObjectVM>
{
	protected bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.MapObjectEntity.IsInGame && base.ViewModel.MapObjectEntity.IsRevealed && base.ViewModel.MapObjectEntity.IsAwarenessCheckPassed && !base.ViewModel.MapObjectEntity.IsInFogOfWar && base.ViewModel.MapObjectEntity.IsInCameraFrustum && (!base.ViewModel.IsCutscene || base.ViewModel.IsBarkActive.CurrentValue) && !base.ViewModel.IsInDialog && !base.ViewModel.ForceHideInCombat.CurrentValue && ((base.ViewModel.IsEnabled.CurrentValue && !base.ViewModel.NotAvailable) || base.ViewModel.IsBarkActive.CurrentValue))
			{
				return !base.ViewModel.MapObjectEntity.VisibilitySuppressedByFlashlight();
			}
			return false;
		}
	}
}
