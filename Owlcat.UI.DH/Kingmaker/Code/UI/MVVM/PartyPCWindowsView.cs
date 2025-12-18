using Kingmaker.Code.View.Bridge.Root;

namespace Kingmaker.Code.UI.MVVM;

public class PartyPCWindowsView : PartyPCView, IInitializable
{
	protected override void Awake()
	{
	}

	public void Initialize()
	{
		ResetCharacters();
		m_Characters.ForEach(delegate(PartyCharacterPCView elem)
		{
			elem.SetSwitchAction(base.DragCharacter);
		});
	}
}
