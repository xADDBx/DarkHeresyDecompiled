using Kingmaker.Visual.Sound;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVoiceItemVM : SelectionGroupEntityVM
{
	public readonly string DisplayName;

	public readonly bool IsEmptyVoice;

	public readonly BlueprintUnitAsksList Voice;

	public CharGenVoiceItemVM(BlueprintUnitAsksList voice)
		: base(allowSwitchOff: false)
	{
		Voice = voice;
		DisplayName = voice.DisplayName;
		IsEmptyVoice = string.IsNullOrEmpty(voice?.PreviewSound);
	}

	protected override void DoSelectMe()
	{
	}
}
