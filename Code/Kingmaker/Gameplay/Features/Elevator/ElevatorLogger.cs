using System.Diagnostics;
using System.IO;
using System.Text;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

public static class ElevatorLogger
{
	public static bool Enabled;

	private static StreamWriter? _writer;

	[Conditional("UNITY_EDITOR")]
	public static void LogPosition(Entity entity)
	{
		_ = Enabled;
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogOrientation(Entity entity)
	{
		_ = Enabled;
	}

	[Conditional("UNITY_EDITOR")]
	public static void CheckViewDesync(Entity? entity, Transform viewTransform)
	{
		if (Enabled && entity is ElevatorPlatformEntity { IsIdle: false })
		{
			Vector3 position = entity.Position;
			Vector3 position2 = viewTransform.position;
			float orientation = entity.Orientation;
			float y = viewTransform.rotation.eulerAngles.y;
			if ((position - position2).sqrMagnitude > 0.0001f)
			{
				_ = position2 - position;
			}
			Mathf.Abs(Mathf.DeltaAngle(orientation, y));
			_ = 0.5f;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogState(Entity entity, ElevatorPlatformState from, ElevatorPlatformState to)
	{
		_ = Enabled;
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogPassengers(Entity entity, ElevatorPlatformPassenger[] passengers)
	{
		if (!Enabled)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"collected {passengers.Length}:");
		foreach (ElevatorPlatformPassenger elevatorPlatformPassenger in passengers)
		{
			MechanicEntity entity2 = elevatorPlatformPassenger.Entity;
			if (entity2 != null)
			{
				stringBuilder.Append($" {FormatEntity(entity2)} pos={entity2.Position}");
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	private static void Write(Entity entity, ElevatorLogEvent evt, string message, string? stackTrace = null)
	{
		int frameCount = Time.frameCount;
		_writer.Write($"[frame={frameCount:00000000}] [{FormatEntity(entity)}] [{evt.ToString().ToUpperInvariant()}] {message}");
		_writer.WriteLine();
		if (stackTrace == null)
		{
			return;
		}
		string[] array = stackTrace.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length > 0)
			{
				_writer.Write("    ");
				_writer.WriteLine(text);
			}
		}
	}

	private static string FormatEntity(Entity entity)
	{
		return entity.GetType().Name + "#" + entity.UniqueId;
	}

	[Conditional("UNITY_EDITOR")]
	private static void EnsureWriter()
	{
		if (_writer == null)
		{
			string text = Path.Combine(Application.dataPath, "..", "Logs");
			Directory.CreateDirectory(text);
			_writer = new StreamWriter(Path.Combine(text, "ElevatorLog.txt"), append: false)
			{
				AutoFlush = true
			};
		}
	}
}
