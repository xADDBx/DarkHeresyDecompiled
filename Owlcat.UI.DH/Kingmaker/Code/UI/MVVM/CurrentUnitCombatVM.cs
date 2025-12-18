using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.AR;
using Kingmaker.Utility.GameConst;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CurrentUnitCombatVM : ViewModel
{
	public CurrentUnitCombatVM()
	{
		SettingsEntityKeyBindingPair highlightObjects = SettingsRoot.Controls.Keybindings.General.HighlightObjects;
		Game.Instance.Keyboard.Bind(highlightObjects.Key + UIConsts.SuffixOn, HighlightOn).AddTo(this);
		Game.Instance.Keyboard.Bind(highlightObjects.Key + UIConsts.SuffixOff, HighlightOff).AddTo(this);
	}

	private void HighlightOn()
	{
		CombatHUDRenderer.Instance.ForceDrawThreatArea = true;
	}

	private void HighlightOff()
	{
		CombatHUDRenderer.Instance.ForceDrawThreatArea = false;
	}
}
