using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.GameConst;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
public class TitlesPCView : TitlesBaseView
{
	[SerializeField]
	private TextMeshProUGUI m_HoldToSpeedupTitle;

	[SerializeField]
	private Color m_HintColor = Color.white;

	private string HintColorTag => ColorUtility.ToHtmlStringRGB(m_HintColor);

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OpenCancelSettingsDialog).AddTo(this);
		string prettyString = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString();
		m_HoldToSpeedupTitle.text = "<color=#" + HintColorTag + ">[" + prettyString + "]</color> " + UIStrings.Instance.Credits.SpeedUp.Text;
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOn, delegate
		{
			SpeedUp(state: true);
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOff, delegate
		{
			SpeedUp(state: false);
		}).AddTo(this);
	}
}
