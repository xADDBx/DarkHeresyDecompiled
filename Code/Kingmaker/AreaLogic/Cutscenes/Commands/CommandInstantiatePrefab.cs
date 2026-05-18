using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("3439f4ec59ea85f498b68f45a4e7aef0")]
public class CommandInstantiatePrefab : CommandBase
{
	[SerializeField]
	[SerializeReference]
	private TransformEvaluator m_Placeholder;

	[SerializeField]
	private GameObject m_Prefab;

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[HideIf("IsContinuous")]
	private float m_Lifetime;

	private bool m_Finished;

	private GameObject m_PrefabInstance;

	public override bool IsContinuous => m_Continuous;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = false;
		if (!m_PrefabInstance)
		{
			m_PrefabInstance = FxHelper.SpawnFxOnGameObject(m_Prefab, m_Placeholder.GetValue().gameObject);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		m_Finished = true;
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		m_Finished = !m_Continuous && time >= (double)m_Lifetime;
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if ((bool)m_PrefabInstance)
		{
			FxHelper.Destroy(m_PrefabInstance);
			m_PrefabInstance = null;
		}
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		string text = (m_Prefab ? m_Prefab.name : "");
		return "<b>Instantiate prefab:</b> " + (text ?? "???");
	}
}
