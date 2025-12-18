using System;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Collections;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Framework.Pathfinding;

[ExecuteInEditMode]
[HelpURL("https://confluence.owlcat.games/pages/viewpage.action?pageId=145591950")]
public class GridObstacle : NavmeshClipper, IGridObstacle
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("GridObstacle");

	public const string Name = "GridObstacle";

	public const string ContainerName = "OBSTACLES";

	public const string GeneratedContainerName = "GENERATED_OBSTACLES";

	private const float PositionUpdateThreshold = 0.1f;

	private const float PositionUpdateThresholdY = 0.5f;

	[Tooltip("GridObstacle по умолчанию прилипает сетке: Y координата выбирается равной Y координате самой низкой клетки из тех, на которые влияет GridObstacle. Если эта галка - true, то GridObstacle не будет прилипать к сетке по Y координате.")]
	public bool FreeYPosition;

	[Tooltip("GridObstacle задает наличие кавера/лосблокер между клетками A<->B. Если эта галка включена, то можно задать по отдельности наличие кавера/лосблокера A->B и B->A.")]
	public bool _Asymmetric;

	[Tooltip("GridObstacle по умолчанию тянется вдоль оси X. Если эта галка true, то он будет тянуться вдоль оси Z. Значение можно менять с помощью вращения объекта вокруг оси Y.")]
	public bool _Rotate;

	[Tooltip("Устанавливает высоту LosBlocker в 0.")]
	[ShowIf("IsLosBlocker")]
	public bool _ZeroHeight;

	[Tooltip("Оставляет связи между клетками.")]
	[ShowIf("IsLosBlocker")]
	public bool _KeepConnections;

	[Tooltip("Высота GridObstacle вдоль оси Υ. Значение умножается на скейл объекта вдоль оси Υ.")]
	public float _LosBlockerHeight;

	[FormerlySerializedAs("Cover")]
	public LosCalculations.CoverType Type = LosCalculations.CoverType.Cover;

	[FormerlySerializedAs("_CoverBackward")]
	[ShowIf("_Asymmetric")]
	public LosCalculations.CoverType _TypeBackward = LosCalculations.CoverType.Cover;

	private Transform _transform;

	private Vector3 _prevPosition;

	private LosCalculations.CoverType _prevType;

	private LosCalculations.CoverType _prevTypeBackward;

	private bool _prevZAligned;

	private bool _prevFreeYPosition;

	private float _prevHeight;

	private float _prevHeightBackward;

	private bool _prevZeroHeight;

	private bool _prevKeepConnections;

	[CanBeNull]
	private EntityViewBase _entityView;

	private bool IsLosBlocker
	{
		get
		{
			if (Type == LosCalculations.CoverType.LosBlocker)
			{
				return TypeBackward == LosCalculations.CoverType.LosBlocker;
			}
			return false;
		}
	}

	private Transform Transform
	{
		get
		{
			if (!(_transform == null))
			{
				return _transform;
			}
			return _transform = base.transform;
		}
	}

	public LosCalculations.CoverType TypeBackward
	{
		get
		{
			if (!_Asymmetric)
			{
				return Type;
			}
			return _TypeBackward;
		}
	}

	public bool ZAligned => _Rotate == (Mathf.RoundToInt(Transform.rotation.eulerAngles.y / 90f) % 2 == 0);

	public int Height => Type switch
	{
		LosCalculations.CoverType.Obstacle => 0, 
		LosCalculations.CoverType.Cover => 1110, 
		LosCalculations.CoverType.LosBlocker => (!_ZeroHeight) ? LosBlockerHeight : 0, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public int HeightBackward => TypeBackward switch
	{
		LosCalculations.CoverType.Obstacle => 0, 
		LosCalculations.CoverType.Cover => 1110, 
		LosCalculations.CoverType.LosBlocker => (!_ZeroHeight) ? LosBlockerHeight : 0, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private int LosBlockerHeight => (int)((float)Math.Max((int)(_LosBlockerHeight * 1000f), 1510) * Transform.localScale.y);

	public bool KeepConnections
	{
		get
		{
			if (IsLosBlocker)
			{
				return _KeepConnections;
			}
			return false;
		}
	}

	[CanBeNull]
	public EntityViewBase EntityView => _entityView;

	public static event Action<GridObstacle> OnAwakeCallback;

	public static event Action<GridObstacle> OnDestroyCallback;

	private void OnValidate()
	{
		if (!base.name.Contains("GridObstacle"))
		{
			base.name = "GridObstacle";
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_entityView = GetComponentInParent<EntityViewBase>();
		EventBus.RaiseEvent(delegate(IGridObstacleAwakeHandler h)
		{
			h.HandleGridObstacleAwake(this);
		});
		GridObstacle.OnAwakeCallback?.Invoke(this);
	}

	private void OnDestroy()
	{
		GridObstacle.OnDestroyCallback?.Invoke(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			EventBus.RaiseEvent(delegate(IGridObstacleEnabledHandler h)
			{
				h.HandleGridObstacleEnabled(this, enabled: true);
			});
		}
		GridObstacleCache.Instance?.Invalidate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			EventBus.RaiseEvent(delegate(IGridObstacleEnabledHandler h)
			{
				h.HandleGridObstacleEnabled(this, enabled: false);
			});
		}
		GridObstacleCache.Instance?.Invalidate();
	}

	public override void NotifyUpdated(GridLookup<NavmeshClipper>.Root previousState)
	{
		_prevPosition = Transform.position;
		_prevType = Type;
		_prevTypeBackward = TypeBackward;
		_prevZAligned = ZAligned;
		_prevFreeYPosition = FreeYPosition;
		_prevHeight = Height;
		_prevHeightBackward = HeightBackward;
		_prevZeroHeight = _ZeroHeight;
		_prevKeepConnections = _KeepConnections;
		GridObstacleCache.Instance?.Invalidate();
	}

	public void Init()
	{
	}

	public override Rect GetBounds(GraphTransform graphTransform, float radiusMargin)
	{
		Vector3 vector = graphTransform.InverseTransform(Transform.position);
		float num = (int)vector.x;
		float num2 = (int)vector.z;
		Vector2 vector2;
		if (ZAligned)
		{
			num2 += 0.5f;
			vector2 = new Vector2(0.1f, 1f);
		}
		else
		{
			num += 0.5f;
			vector2 = new Vector2(1f, 0.1f);
		}
		num = ((Math.Abs(num - vector.x) <= Math.Abs(num + 1f - vector.x)) ? num : (num + 1f));
		num2 = ((Math.Abs(num2 - vector.z) <= Math.Abs(num2 + 1f - vector.z)) ? num2 : (num2 + 1f));
		return new Rect(new Vector2(num, num2) - vector2 / 2f, vector2);
	}

	public override bool RequiresUpdate(GridLookup<NavmeshClipper>.Root previousState)
	{
		bool flag = _prevType != Type || _prevTypeBackward != TypeBackward || _prevZAligned != ZAligned || _prevFreeYPosition != FreeYPosition || _prevZeroHeight != _ZeroHeight || _prevKeepConnections != _KeepConnections || Math.Abs(_prevHeight - (float)Height) > 0.05f || Math.Abs(_prevHeightBackward - (float)HeightBackward) > 0.05f;
		if (!flag)
		{
			Vector3 position = Transform.position;
			flag = Math.Abs(_prevPosition.x - position.x) > 0.1f || Math.Abs(_prevPosition.y - position.y) > 0.5f || Math.Abs(_prevPosition.z - position.z) > 0.1f;
		}
		return flag;
	}

	public override void ForceUpdate()
	{
		_prevHeight = -2.1474836E+09f;
	}

	public (GridNodeIndex forwardNode, GridNodeIndex backwardNode) GetAffectedNodes(GraphTransform graphTransform)
	{
		return GetAffectedNodes(GetBounds(graphTransform, 0f));
	}

	public static (GridNodeIndex forwardNode, GridNodeIndex backwardNode) GetAffectedNodes(Rect rect)
	{
		GridNodeIndex item;
		GridNodeIndex item2;
		if (rect.width > rect.height)
		{
			item = new GridNodeIndex((int)rect.center.x, (int)(rect.center.y + 0.5f));
			item2 = new GridNodeIndex((int)rect.center.x, (int)(rect.center.y - 0.5f));
		}
		else
		{
			item = new GridNodeIndex((int)(rect.center.x + 0.5f), (int)rect.center.y);
			item2 = new GridNodeIndex((int)(rect.center.x - 0.5f), (int)rect.center.y);
		}
		return (forwardNode: item, backwardNode: item2);
	}

	public Bounds GetMechanicBounds(GraphTransform graphTransform, bool includeOuterNodes)
	{
		float num = (float)Math.Max(Height, HeightBackward) / 1000f;
		Vector3 center = graphTransform.Transform(GetBounds(graphTransform, 0f).center.To3D()) + new Vector3(0f, num / 2f, 0f);
		Vector3 size = (ZAligned ? new Vector3(includeOuterNodes ? 2.Cells().Meters : 0f, num, 1.Cells().Meters) : new Vector3(1.Cells().Meters, num, includeOuterNodes ? 2.Cells().Meters : 0f));
		return new Bounds(center, size);
	}
}
