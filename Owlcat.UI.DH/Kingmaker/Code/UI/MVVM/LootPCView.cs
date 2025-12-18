using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class LootPCView : LootView<LootCollectorPCView, InteractionSlotPartPCView, PlayerStashPCView>
{
	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
	}
}
