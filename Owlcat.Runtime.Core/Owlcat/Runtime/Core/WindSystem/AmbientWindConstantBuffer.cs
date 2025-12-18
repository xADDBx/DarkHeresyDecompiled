using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.WindSystem;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@ac854826f94a\\Runtime\\WindSystem\\AmbientWindConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct AmbientWindConstantBuffer
{
	public const int kWindStrengthOctaveCount = 2;

	public const int kWindShiftOctaveCount = 2;

	public float _GlobalWindEnabled;

	public Vector3 _Pad0;

	public Vector2 _WindVector;

	public float _StrengthNoiseWeight;

	public float _StrengthNoiseContrast;

	[HLSLArray(2, typeof(Vector4))]
	public unsafe fixed float _CompressedStrengthOctaves[8];

	[HLSLArray(2, typeof(Vector4))]
	public unsafe fixed float _CompressedShiftOctaves[8];

	public unsafe void SetStrengthOctave(int index, Vector4 octave)
	{
		for (int i = 0; i < 4; i++)
		{
			_CompressedStrengthOctaves[index * 4 + i] = octave[i];
		}
	}

	public unsafe void SetShiftOctave(int index, Vector4 octave)
	{
		for (int i = 0; i < 4; i++)
		{
			_CompressedShiftOctaves[index * 4 + i] = octave[i];
		}
	}
}
