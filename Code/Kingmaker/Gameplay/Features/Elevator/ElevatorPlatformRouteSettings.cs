using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
public sealed class ElevatorPlatformRouteSettings : IElevatorPlatformRouteSettings
{
	[ValidateNotNull]
	public ElevatorPlatformStopView From;

	[ValidateNotNull]
	public ElevatorPlatformStopView To;

	[InfoBox("Промежуточные точки перемещения между From и To")]
	public Transform[] Waypoints = new Transform[0];

	[InfoBox("Настройки перемещения между точками маршрута From -> (Waypoints) -> To. Количество транзишенов всегда равно количеству Waypoints + 1")]
	public ElevatorPlatformTransitionSettings[] Transitions = new ElevatorPlatformTransitionSettings[0];

	EntityRef<ElevatorPlatformStopEntity> IElevatorPlatformRouteSettings.From => new EntityRef<ElevatorPlatformStopEntity>(From.UniqueId);

	EntityRef<ElevatorPlatformStopEntity> IElevatorPlatformRouteSettings.To => new EntityRef<ElevatorPlatformStopEntity>(To.UniqueId);

	IEnumerable<ElevatorPlatformTransform> IElevatorPlatformRouteSettings.Waypoints => Waypoints.Select((Transform i) => new ElevatorPlatformTransform(i.position, i.rotation.eulerAngles.y)).Prepend(new ElevatorPlatformTransform(From.transform.position, From.transform.rotation.eulerAngles.y)).Append(new ElevatorPlatformTransform(To.transform.position, To.transform.rotation.eulerAngles.y));

	IEnumerable<ElevatorPlatformTransitionSettings> IElevatorPlatformRouteSettings.Transitions => Transitions;
}
