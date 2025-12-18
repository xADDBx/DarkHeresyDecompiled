using Kingmaker.Networking;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GamerTagAndNameVM : ViewModel
{
	private readonly ReactiveProperty<string> m_UserId;

	private readonly ReactiveProperty<string> m_Name;

	private readonly ReactiveProperty<PhotonActorNumber> m_UserNumber;

	public ReadOnlyReactiveProperty<string> UserId => m_UserId;

	public ReadOnlyReactiveProperty<PhotonActorNumber> UserNumber => m_UserNumber;

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public GamerTagAndNameVM(ReactiveProperty<string> userId, ReactiveProperty<PhotonActorNumber> userNumber, ReactiveProperty<string> name)
	{
		m_UserId = userId;
		m_UserNumber = userNumber;
		m_Name = name;
	}

	public async void ShowGamerCard()
	{
		PFLog.UI.Log($"Show card User Id {UserId.CurrentValue} / User Number {UserNumber.CurrentValue} / Name {Name.CurrentValue}");
	}
}
