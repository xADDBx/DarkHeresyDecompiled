using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class VeilThicknessLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventVeilChanged>
{
	public void HandleEvent(GameLogEventVeilChanged evt)
	{
		TooltipBaseTemplate template = CombatLogTooltipService.CreateTooltipTemplateGlossary("VeilThickness");
		if (evt.Delta != 0)
		{
			GameLogContext.VeilThicknessDelta = evt.Delta;
			GameLogContext.VeilThicknessValue = evt.NewValue;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.VeilThicknessValueChanged.CreateCombatLogMessage(), template));
		}
	}
}
