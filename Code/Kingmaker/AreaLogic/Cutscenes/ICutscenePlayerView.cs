using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.AreaLogic.Cutscenes;

public interface ICutscenePlayerView : IEntityViewBase
{
	BlueprintCutscene Cutscene { get; set; }
}
