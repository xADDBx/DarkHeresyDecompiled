using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoiceSelectorCommonView : BaseCharGenAppearancePageComponentView<CharGenVoiceSelectorVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private CharGenVoiceSelectorGroupView m_VoiceSelectorGroupView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public IConsoleEntity ConsoleEntityProxy => m_VoiceSelectorGroupView;

	protected override void BindViewImplementation()
	{
		m_HeaderLabel.text = UIStrings.Instance.CharGen.Voice;
		m_VoiceSelectorGroupView.Bind(base.ViewModel.VoiceSelector);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_VoiceSelectorGroupView.SetFocus(value);
	}
}
