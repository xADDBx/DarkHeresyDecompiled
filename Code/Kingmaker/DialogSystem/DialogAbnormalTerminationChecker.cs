using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.DialogSystem;

public class DialogAbnormalTerminationChecker
{
	public BlueprintDialog Dialog { get; set; }

	public BlueprintCue CurrentCue { get; set; }

	public BlueprintAnswer LastSelectedAnswer { get; set; }

	public List<BlueprintAnswer> Answers { get; set; }

	public bool Check(out string errorMessage)
	{
		errorMessage = null;
		if (Dialog == null)
		{
			return true;
		}
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (!Answers.Any())
		{
			stringBuilder.AppendLine("No final answers were shown.");
			flag = true;
		}
		else if (LastSelectedAnswer.NextCue.Cues.Count > 0)
		{
			stringBuilder.AppendLine("Final answer '" + LastSelectedAnswer.name + "' has next cues present.");
			flag = true;
		}
		if (!IsPossibleEnding(CurrentCue, LastSelectedAnswer))
		{
			stringBuilder.AppendLine(LastSelectedAnswer.IsExit() ? ("Dialog was interrupted. Cue " + CurrentCue.name + " is not possible ending") : ("Dialog was interrupted. Last cue: " + CurrentCue.name + ", last answer: " + LastSelectedAnswer.name));
			flag = true;
		}
		if (IsSpeakerAbsent(CurrentCue))
		{
			stringBuilder.AppendLine("Speaker for cue " + CurrentCue.name + " doesnt exist");
			flag = true;
		}
		if (flag)
		{
			if (CurrentCue != null)
			{
				stringBuilder.AppendLine("Current cue: '" + CurrentCue.name + "'");
			}
			errorMessage = stringBuilder.ToString();
		}
		return !flag;
	}

	private static bool IsSpeakerAbsent(BlueprintCue currentCue)
	{
		if (currentCue.Speaker.NeedsEntity)
		{
			if (currentCue.Speaker.TryGetSpeakerEntity(currentCue, out var _))
			{
				return currentCue.Speaker.ReplacedSpeakerWithErrorSpeaker;
			}
			return true;
		}
		return false;
	}

	private static bool IsPossibleEnding(BlueprintCue lastShownCue, BlueprintAnswer lastAnswer)
	{
		bool flag = IsEndingCue(lastShownCue);
		bool flag2 = lastAnswer.IsExit();
		if (!(flag2 && flag))
		{
			if (!flag2)
			{
				return IsEndingAnswer(lastAnswer);
			}
			return false;
		}
		return true;
	}

	private static bool IsEndingCue(BlueprintCue currentCue)
	{
		if (!currentCue.IsPossibleEnding)
		{
			if (!currentCue.Answers.Any())
			{
				return !currentCue.Continue.Cues.Any();
			}
			return false;
		}
		return true;
	}

	private static bool IsEndingAnswer(BlueprintAnswer answer)
	{
		if (answer.NextCue.Cues.Any())
		{
			return answer.IsPossibleEnding;
		}
		return true;
	}
}
