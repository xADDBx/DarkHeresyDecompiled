using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public struct ClueUIData
{
	public LocalizedString Name;

	public LocalizedString Description;

	public Sprite Icon;

	public BlueprintClue.UIType UIType;
}
