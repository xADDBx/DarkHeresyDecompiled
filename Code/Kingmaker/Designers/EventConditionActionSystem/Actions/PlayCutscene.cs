using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Tutorial;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[ComponentName("Actions/PlayCutscene")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("507aef8c6c6218c49aaf0987b355f400")]
public class PlayCutscene : GameAction, ICutsceneReference
{
	[ValidateNotNull]
	[SerializeField]
	private CutsceneReference m_Cutscene;

	private CutscenePlayerView m_CutscenePlayerView;

	[ShowIf("PutInQueue")]
	public bool PutInQueue;

	[ShowIf("PutInQueue")]
	public bool CheckExistence = true;

	public ParametrizedContextSetter Parameters;

	public BlueprintCutscene Cutscene
	{
		get
		{
			return m_Cutscene?.Get();
		}
		set
		{
			m_Cutscene = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<CutsceneReference>();
		}
	}

	public CutscenePlayerData CutsceneData => m_CutscenePlayerView?.PlayerData;

	protected override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			throw new Exception("The cutscene can't be started from the preview unit!");
		}
		if ((bool)ContextData<TutorialIsActiveContext>.Current)
		{
			throw new Exception("The cutscene can't be started from the tutorial!");
		}
		BlueprintCutscene cutscene = Cutscene;
		if (cutscene != null && cutscene.PreventCopies)
		{
			NamedParametersContext namedParametersContext = BuildProbeParameters(cutscene);
			foreach (CutscenePlayerData cutscene2 in Game.Instance.EntityPools.Cutscenes)
			{
				if (cutscene2.Cutscene == cutscene && !cutscene2.IsFinished && namedParametersContext.IsTheSame(cutscene2.Parameters))
				{
					return;
				}
			}
			foreach (EntitySpawnController.SpawnEntry item in Game.Instance.Controllers.EntitySpawner.CreationQueue)
			{
				if (item.Entity is CutscenePlayerData cutscenePlayerData && cutscenePlayerData.Cutscene == cutscene && namedParametersContext.IsTheSame(cutscenePlayerData.Parameters))
				{
					return;
				}
			}
		}
		if (PutInQueue && CheckExistence)
		{
			CutscenePlayerData cutscenePlayerData2 = CutscenePlayerData.Queue.FirstOrDefault((CutscenePlayerData c) => c.PlayActionId == name);
			if (cutscenePlayerData2 != null)
			{
				cutscenePlayerData2.PreventDestruction = true;
				cutscenePlayerData2.Stop();
				cutscenePlayerData2.PreventDestruction = false;
			}
		}
		SceneEntitiesState state = ContextData<SpawnedUnitData>.Current?.State;
		m_CutscenePlayerView = CutscenePlayerView.Play(cutscene, Parameters, PutInQueue, state);
		m_CutscenePlayerView.PlayerData.PlayActionId = name;
		m_CutscenePlayerView.PlayerData.OriginBlueprint = base.Owner;
	}

	private NamedParametersContext BuildProbeParameters(BlueprintCutscene cutscene)
	{
		NamedParametersContext namedParametersContext = new NamedParametersContext();
		if (ContextData<NamedParametersContext.ContextData>.Current != null)
		{
			foreach (KeyValuePair<string, INamedParameterValue> param in ContextData<NamedParametersContext.ContextData>.Current.Context.Params)
			{
				namedParametersContext.Params.TryAdd(param.Key, param.Value);
			}
		}
		if (Parameters != null)
		{
			if (Parameters.Parameters != null)
			{
				ParametrizedContextSetter.ParameterEntry[] parameters = Parameters.Parameters;
				foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in parameters)
				{
					namedParametersContext.Params[parameterEntry.Name] = parameterEntry.GetValue();
				}
			}
			foreach (KeyValuePair<string, INamedParameterValue> additionalParam in Parameters.AdditionalParams)
			{
				namedParametersContext.Params[additionalParam.Key] = additionalParam.Value;
			}
		}
		if (cutscene.DefaultParameters?.Parameters != null)
		{
			ParametrizedContextSetter.ParameterEntry[] parameters = cutscene.DefaultParameters.Parameters;
			foreach (ParametrizedContextSetter.ParameterEntry parameterEntry2 in parameters)
			{
				namedParametersContext.Params.TryAdd(parameterEntry2.Name, parameterEntry2.GetValue());
			}
		}
		return namedParametersContext;
	}

	public override string GetCaption()
	{
		return string.Format("Play scene {0}", Cutscene ? Cutscene.name : "??");
	}

	public bool GetUsagesFor(BlueprintCutscene cutscene)
	{
		return cutscene == Cutscene;
	}
}
