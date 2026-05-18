using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Editor;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public sealed class BarkInteractionModule : InteractionModule, IBarkSource
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString? Bark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	IEnumerable<LocalizedString> IBarkSource.Barks
	{
		get
		{
			if (Bark == null)
			{
				return Enumerable.Empty<LocalizedString>();
			}
			return new LocalizedString[1] { Bark };
		}
	}

	bool IBarkSource.IsVoIdForced => ForceVoId;

	List<string> IBarkSource.ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	bool IBarkSource.Spammable => IsSpammable;

	public override string GetCaption()
	{
		return "Bark";
	}

	public override Task Execute(BaseUnitEntity initiator, MapObjectEntity target)
	{
		LocalizedString bark = Bark;
		if (bark != null && !bark.Empty)
		{
			using (ContextData<ActionExecutionContextData>.Request().Setup(ActionExecutionContextData.Type.Interaction))
			{
				string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, target);
				BarkPlayer.Bark(target, bark, VoiceOverType.Bark, voGuidBySourceAndTarget, -1f, initiator);
			}
		}
		return Task.CompletedTask;
	}
}
