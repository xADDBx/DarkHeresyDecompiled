using System;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Levelup.Selections.Voice;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("ac599f45249f475788a1c326d8e103f7")]
public class BlueprintVoiceSelection : BlueprintSelectionWithUI
{
	public AskType[] PreviewAskTypes = new AskType[4]
	{
		AskType.AggroBattleCry,
		AskType.Pain,
		AskType.Death,
		AskType.Unconscious
	};
}
