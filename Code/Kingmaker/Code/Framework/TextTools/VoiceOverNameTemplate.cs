using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Code.Framework.TextTools;

public class VoiceOverNameTemplate : TextTemplate
{
	public override int MinParameters => 0;

	public override int MaxParameters => 2;

	public bool IsActiveExportToVoiceOver => VOTextHelper.IsActiveExportToVoiceOver;

	private BaseUnitEntity Unit => Game.Instance.Controllers.DialogController.ActingUnit ?? Game.Instance.Player.MainCharacterEntity;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!IsActiveExportToVoiceOver)
		{
			return Unit.CharacterName;
		}
		if (parameters.Count <= 1)
		{
			if (parameters.Count <= 0)
			{
				return string.Empty;
			}
			return parameters[0];
		}
		int valueOrDefault = (int)(((MechanicEntity)((GameLogContext.InScope ? GameLogContext.SourceEntity.Value : null) ?? Game.Instance.Controllers.DialogController.ActingUnit ?? Game.Instance.Player.MainCharacterEntity))?.GetDescriptionOptional()?.Gender).GetValueOrDefault();
		if (parameters.Count > valueOrDefault)
		{
			return parameters[valueOrDefault];
		}
		return string.Empty;
	}
}
