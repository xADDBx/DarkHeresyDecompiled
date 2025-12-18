using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NewSaveSlotVM : SaveSlotVM
{
	public NewSaveSlotVM(SaveInfo saveInfo, ReadOnlyReactiveProperty<SaveLoadMode> mode, SaveLoadActions actions = default(SaveLoadActions))
		: base(saveInfo, mode, actions, Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad)
	{
	}
}
