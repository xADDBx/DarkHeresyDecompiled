using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.InputSystems;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventPCView : BookEventBaseView, IHasBlueprintInfo
{
	public BlueprintScriptableObject Blueprint => Game.Instance.Controllers.DialogController.Dialog;

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.IsShowHistory.CurrentValue)
			{
				EventBus.RaiseEvent(delegate(IEscMenuHandler h)
				{
					h.HandleOpen();
				});
			}
		}).AddTo(this);
	}
}
