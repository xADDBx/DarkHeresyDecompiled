using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhasePresetEntityVM : SelectionGroupEntityVM
{
	public readonly NewGamePreset Preset;

	public readonly string DisplayName;

	public readonly string Description;

	public readonly Sprite Picture;

	public NewGamePhasePresetEntityVM(NewGamePreset preset)
		: base(allowSwitchOff: false)
	{
		Preset = preset;
		DisplayName = preset.DisplayName;
		Description = preset.Description;
		Picture = preset.Picture;
	}

	protected override void DoSelectMe()
	{
	}
}
