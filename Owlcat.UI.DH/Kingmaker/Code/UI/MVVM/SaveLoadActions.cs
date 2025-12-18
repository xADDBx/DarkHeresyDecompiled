using System;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Code.UI.MVVM;

public struct SaveLoadActions
{
	public Action<SaveInfo> Select;

	public Action<SaveInfo> SaveOrLoad;

	public Action<SaveInfo> Delete;

	public Action<SaveSlotVM> ShowScreenshot;

	public Action<SaveInfo> DeleteWithoutBox;
}
