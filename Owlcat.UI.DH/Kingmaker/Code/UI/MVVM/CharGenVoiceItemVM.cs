using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Visual.Sound;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVoiceItemVM : SelectionGroupEntityVM
{
	public readonly string DisplayName;

	public readonly BlueprintUnitAsksList Asks;

	public readonly string VoGuid;

	public readonly MusicStateHandler.MusicChargenPCVoice MusicChargenVoice;

	public Action OnClicked;

	public CharGenVoiceItemVM(BlueprintUnitAsksList asks, BlueprintCharGenRoot.VoiceEntry settings = null)
		: base(allowSwitchOff: true)
	{
		Asks = asks;
		VoGuid = settings?.VoId.Guid;
		MusicChargenVoice = settings?.MusicChargenVoice ?? MusicStateHandler.MusicChargenPCVoice.None;
		DisplayName = (asks.DisplayName.Empty ? asks.AssetName : ((string)asks.DisplayName));
	}

	protected override void DoSelectMe()
	{
	}
}
