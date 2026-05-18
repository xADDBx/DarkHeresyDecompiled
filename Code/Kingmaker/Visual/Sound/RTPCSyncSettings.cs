using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[TypeId("476b16f009150454eb13a4ad939f8bb7")]
public class RTPCSyncSettings : GameSyncSettings
{
	[SerializeField]
	[AkGameParameterReference]
	private string rtpc;

	[SerializeField]
	private PropertyCalculator bindingProperty;

	public override string GetCaption()
	{
		return $"[RTPC] {rtpc} set to {bindingProperty}";
	}

	public override void Sync(MechanicEntity entity, IEvalContext context, uint playingId)
	{
		float in_value = (float)bindingProperty.GetValue(entity, context) / 100f;
		AkUnitySoundEngine.SetRTPCValueByPlayingID(rtpc, in_value, playingId);
	}
}
