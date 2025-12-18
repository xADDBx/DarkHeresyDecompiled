using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound.Base;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Play3DSound")]
[AllowMultipleComponents]
[TypeId("be3026f011f344f448094a75ed64a9f5")]
public class Play3DSound : GameAction
{
	[Tooltip("Ak event name from Wwise library")]
	public string SoundName;

	public EntityReference SoundSourceObject;

	[Tooltip("Sets Ak switch on player's Sex")]
	public bool SetSex;

	[Tooltip("Sets Ak switch on player's Race")]
	public bool SetRace;

	[Tooltip("Sets SoundSourceObject as current dialog speaker")]
	public bool SetCurrentSpeaker;

	protected override void RunAction()
	{
		Play(SoundName, SoundSourceObject, SetSex, SetRace, SetCurrentSpeaker, this);
	}

	public static void Play(string soundName, EntityReference soundSourceObject, bool setSex, bool setRace, bool setCurrentSpeaker, object logContext = null)
	{
		object context = logContext ?? typeof(Play3DSound);
		if (string.IsNullOrEmpty(soundName))
		{
			Element.LogError(context, "Sound name is Empty. Can't play sound.");
			return;
		}
		GameObject gameObject = null;
		if (setCurrentSpeaker)
		{
			if (Game.Instance.Controllers.DialogController.CurrentSpeaker != null)
			{
				gameObject = Game.Instance.Controllers.DialogController.CurrentSpeaker.View.gameObject;
			}
			if (!gameObject)
			{
				Element.LogError(context, "CurrentSpeaker is NULL");
				return;
			}
		}
		else
		{
			EntityViewBase entityViewBase = (EntityViewBase)(soundSourceObject?.FindView());
			if (!entityViewBase)
			{
				Element.LogError(context, "Target object for sound play is NULL");
				return;
			}
			if (entityViewBase is UnitSpawnerBase { SpawnedUnit: var spawnedUnit })
			{
				if (spawnedUnit == null)
				{
					return;
				}
				gameObject = spawnedUnit.View.gameObject;
			}
			else
			{
				gameObject = entityViewBase.gameObject;
			}
		}
		if (setSex)
		{
			SoundUtility.SetGenderFlags(gameObject);
		}
		if (setRace)
		{
			SoundUtility.SetRaceFlags(gameObject);
		}
		SoundEventsManager.PostEvent(soundName, gameObject, canBeStopped: true);
	}

	public override string GetCaption()
	{
		return "Sound 3D (" + SoundName + ")";
	}
}
