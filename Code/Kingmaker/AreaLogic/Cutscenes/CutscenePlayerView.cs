using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[KnowledgeDatabaseID("a31f060d9d48c194eafe99981b1b4c73")]
public class CutscenePlayerView : EntityViewBase, ICutscenePlayerView, IEntityView
{
	public BlueprintCutscene Cutscene { get; set; }

	public CutscenePlayerData PlayerData => (CutscenePlayerData)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutscenePlayerData(Cutscene, this));
	}

	public static CutscenePlayerView Play(BlueprintCutscene cutscene)
	{
		return Play(cutscene, null);
	}

	public static CutscenePlayerView Play(BlueprintCutscene cutscene, ParametrizedContextSetter context, bool queued = false, SceneEntitiesState state = null)
	{
		CutscenePlayerView cutscenePlayerView = new GameObject("[cutscene player " + cutscene.name + "]").AddComponent<CutscenePlayerView>();
		cutscenePlayerView.Cutscene = cutscene;
		cutscenePlayerView.UniqueId = Uuid.Instance.CreateGuidForSceneObject(cutscenePlayerView);
		if (state == null)
		{
			state = Game.Instance.State.LoadedAreaState.MainState;
		}
		Game.Instance.Controllers.EntitySpawner.SpawnEntityWithView(cutscenePlayerView, state);
		if (ContextData<NamedParametersContext.ContextData>.Current != null && ContextData<NamedParametersContext.ContextData>.Current.Context.Params.Any())
		{
			foreach (KeyValuePair<string, INamedParameterValue> param in ContextData<NamedParametersContext.ContextData>.Current.Context.Params)
			{
				cutscenePlayerView.PlayerData.Parameters.Params.TryAdd(param.Key, param.Value);
			}
		}
		if (context != null)
		{
			cutscenePlayerView.PlayerData.ParameterSetter = context;
			ParametrizedContextSetter.ParameterEntry[] array = context.Parameters.EmptyIfNull();
			foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
			{
				cutscenePlayerView.PlayerData.Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
			}
			foreach (KeyValuePair<string, INamedParameterValue> additionalParam in context.AdditionalParams)
			{
				cutscenePlayerView.PlayerData.Parameters.Params[additionalParam.Key] = additionalParam.Value;
			}
		}
		cutscenePlayerView.PlayerData.Start(queued);
		return cutscenePlayerView;
	}

	GameObject ICutscenePlayerView.get_gameObject()
	{
		return base.gameObject;
	}

	string IEntityView.get_name()
	{
		return base.name;
	}
}
