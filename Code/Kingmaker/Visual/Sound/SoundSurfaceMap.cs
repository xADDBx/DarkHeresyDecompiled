using System;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SoundSurfaceMap : RegisteredBehaviour
{
	public const float TileSize = 0.2f;

	[SerializeField]
	[HideInInspector]
	private byte[] _serializedData;

	[SerializeField]
	[HideInInspector]
	private TextAsset _soundCacheFile;

	public Bounds Bounds;

	[SerializeField]
	[Tooltip("Allows editing of Y extent")]
	private bool _unboundY;

	[SerializeField]
	[Tooltip("Will return SoundSurface only for objects within bounds")]
	private bool _useOnlyInBounds;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _raycastThickness;

	private byte[] _runtimeData;

	public int Width { get; private set; }

	public int Length { get; private set; }

	public TextAsset SoundCacheFile => _soundCacheFile;

	public float RaycastThickness => _raycastThickness;

	public bool UseOnlyInBounds => _useOnlyInBounds;

	protected override void OnEnabled()
	{
		UpdateValues();
	}

	private void OnValidate()
	{
		if (!_unboundY)
		{
			Vector3 extents = Bounds.extents;
			Bounds.extents = new Vector3(extents.x, Math.Max(extents.x, extents.z), extents.z);
		}
	}

	public static SurfaceType? GetSurfaceSoundTypeSwitch(Vector3 worldPos)
	{
		foreach (SoundSurfaceMap item in ObjectRegistry<SoundSurfaceMap>.Instance)
		{
			if (item.TryGetSurfaceType(worldPos, out var surfaceType))
			{
				return (SurfaceType)surfaceType;
			}
		}
		return null;
	}

	public bool TryGetSurfaceType(Vector3 worldPos, out byte surfaceType)
	{
		surfaceType = 0;
		if (_runtimeData == null)
		{
			return false;
		}
		if (!TryGetCoordinates(worldPos, out var x, out var z))
		{
			return false;
		}
		surfaceType = Get(GetIndex(x, z));
		return true;
	}

	public byte Get(int index)
	{
		if (index >= 0 && index < _runtimeData.Length)
		{
			return _runtimeData[index];
		}
		return 0;
	}

	public bool TryGetCoordinates(Vector3 worldPos, out int x, out int z)
	{
		x = (int)((worldPos.x - Bounds.min.x) / 0.2f);
		z = (int)((worldPos.z - Bounds.min.z) / 0.2f);
		if (_useOnlyInBounds && !Bounds.Contains(worldPos))
		{
			return false;
		}
		if (x >= 0)
		{
			return z >= 0;
		}
		return false;
	}

	public int GetIndex(int x, int z)
	{
		return x + Width * z;
	}

	public void UpdateValues()
	{
		Width = Mathf.CeilToInt(Bounds.size.x / 0.2f);
		Length = Mathf.CeilToInt(Bounds.size.z / 0.2f);
		_runtimeData = (_soundCacheFile ? _soundCacheFile.bytes : _serializedData);
	}

	public void SetData(TextAsset soundCacheFile)
	{
		_serializedData = null;
		_soundCacheFile = soundCacheFile;
		_runtimeData = SoundCacheFile.bytes;
	}
}
