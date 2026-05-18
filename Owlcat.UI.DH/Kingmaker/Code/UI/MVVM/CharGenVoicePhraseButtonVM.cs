using System;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVoicePhraseButtonVM : ViewModel
{
	public readonly string Label;

	public readonly LocalizedString String;

	private readonly string m_VoGuid;

	private readonly Func<LocalizedString, string, bool> m_PlayAction;

	public ReactiveProperty<bool> IsPlaying { get; } = new ReactiveProperty<bool>(value: false);


	public CharGenVoicePhraseButtonVM(LocalizedString str, string voGuid, Func<LocalizedString, string, bool> playAction)
	{
		Label = str.Text;
		String = str;
		m_VoGuid = voGuid;
		m_PlayAction = playAction;
	}

	public void PlayPhrase()
	{
		m_PlayAction(String, m_VoGuid);
	}
}
