using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ResetForcedMusicState")]
[AllowMultipleComponents]
[TypeId("828f6b74fa544a599747e61caf30c9f9")]
public class ResetForcedMusicState : GameAction
{
	[SerializeField]
	private AkStateReference m_State;

	public override string GetCaption()
	{
		return "Sets " + m_State.Group + " to None";
	}

	protected override void RunAction()
	{
		SoundState.Instance.MusicStateHandler.ResetMusicStateOverride(m_State);
	}
}
