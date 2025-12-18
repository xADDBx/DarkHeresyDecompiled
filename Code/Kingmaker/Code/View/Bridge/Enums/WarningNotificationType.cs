using OwlPack.Runtime;

namespace Kingmaker.Code.View.Bridge.Enums;

[OwlPackOldName("Kingmaker.Code.UI.MVVM.WarningNotificationType, Code")]
public enum WarningNotificationType
{
	None,
	GameLoaded,
	GameSaved,
	GameSavedQuick,
	GameSavedAuto,
	SavingImpossible,
	DifficultyChanged,
	Other,
	SavingImpossibleIronman,
	GameSavedInProgress,
	EquipInCombatIsImpossible,
	SavingError,
	NoQuickSaves,
	SavingFailed,
	SavingImpossibleIronmanWillSavedAutomatically,
	EquipInLevlUpIsImpossible
}
