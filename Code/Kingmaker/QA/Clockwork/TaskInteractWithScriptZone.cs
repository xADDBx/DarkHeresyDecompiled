using System.Collections;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.QA.Clockwork;

public class TaskInteractWithScriptZone : ClockworkRunnerTask
{
	private Entity m_ZoneEntity;

	public TaskInteractWithScriptZone(ClockworkRunner runner, Entity entity)
		: base(runner)
	{
		m_ZoneEntity = entity;
	}

	protected override IEnumerator Routine()
	{
		ScriptZoneEntity scriptZoneEntity = m_ZoneEntity as ScriptZoneEntity;
		yield return new TaskMovePartyToPoint(Runner, scriptZoneEntity.Config.Shapes.First().Center());
		Runner.MarkAsInteracted(m_ZoneEntity.UniqueId);
	}

	public override string ToString()
	{
		return "Interact with script zone";
	}
}
