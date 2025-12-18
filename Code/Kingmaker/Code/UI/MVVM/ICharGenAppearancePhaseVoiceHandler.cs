using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenAppearancePhaseVoiceHandler : ISubscriber
{
	void HandleChangeVoice([NotNull] BlueprintUnitAsksList blueprint);
}
