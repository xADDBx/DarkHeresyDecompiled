using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoiceSelectorCommonView : BaseCharGenAppearancePageComponentView<CharGenVoiceSelectorVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private CharGenVoiceSelectorGroupView m_VoiceSelectorGroupView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		m_HeaderLabel.text = UIStrings.Instance.CharGen.Voice;
		m_VoiceSelectorGroupView.Bind(base.ViewModel.VoiceSelector);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}
}
