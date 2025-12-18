namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerPartyCharacterConsoleView : GroupChangerCharacterBaseView
{
	protected override void SetState(bool isInParty, bool isLock)
	{
		base.gameObject.SetActive(isInParty || isLock);
	}
}
