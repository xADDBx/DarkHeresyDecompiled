using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface IContextMenuCollectionEntity
{
	bool IsValid();

	bool IsHeader();

	bool IsInteractable();

	bool IsEmpty();

	bool IsEnabled();

	Sprite GetIcon();

	LocalizedString GetTitleLocalized();

	string GetTitle();

	string GetSubTitle();

	void Execute();

	ButtonSoundsEnum GetClickSoundType();

	ButtonSoundsEnum GetHoverSoundType();
}
