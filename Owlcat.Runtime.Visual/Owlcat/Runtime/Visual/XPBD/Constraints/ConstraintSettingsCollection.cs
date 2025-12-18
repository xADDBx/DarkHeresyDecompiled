using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class ConstraintSettingsCollection
{
	public static readonly int ConstraintTypeCount;

	public DistanceConstraintSettings DistanceConstraintSettings = new DistanceConstraintSettings();

	public BendConstraintSettings BendConstraintSettings = new BendConstraintSettings();

	public ShapeConstraintSettings ShapeConstraintSettings = new ShapeConstraintSettings();

	public AngularConstraintSettings AngularConstraintSettings = new AngularConstraintSettings();

	public SimplexConstraintSettings SimplexConstraintSettings = new SimplexConstraintSettings();

	public AerodynamicsConstraintSettings AerodynamicsConstraintSettings = new AerodynamicsConstraintSettings();

	public FoliageConstraintSettings FoliageConstraintSettings = new FoliageConstraintSettings();

	public ConstraintSettings this[ConstraintType type] => type switch
	{
		ConstraintType.Distance => DistanceConstraintSettings, 
		ConstraintType.Bend => BendConstraintSettings, 
		ConstraintType.Shape => ShapeConstraintSettings, 
		ConstraintType.Angular => AngularConstraintSettings, 
		ConstraintType.Simplex => SimplexConstraintSettings, 
		ConstraintType.Aerodynamics => AerodynamicsConstraintSettings, 
		ConstraintType.Foliage => FoliageConstraintSettings, 
		_ => throw new NotImplementedException(), 
	};

	public static ConstraintSettingsCollection Default
	{
		get
		{
			ConstraintSettingsCollection constraintSettingsCollection = new ConstraintSettingsCollection();
			for (int i = 0; i < ConstraintTypeCount; i++)
			{
				constraintSettingsCollection[(ConstraintType)i].Enabled = true;
			}
			return constraintSettingsCollection;
		}
	}

	static ConstraintSettingsCollection()
	{
		int num = 0;
		foreach (ConstraintType value in Enum.GetValues(typeof(ConstraintType)))
		{
			_ = value;
			num++;
		}
		ConstraintTypeCount = num;
	}

	public uint GetEnabledMask()
	{
		uint num = 0u;
		for (int i = 0; i < ConstraintTypeCount; i++)
		{
			num = ((!this[(ConstraintType)i].Enabled) ? (num & (uint)(~(1 << i))) : (num | (uint)(1 << i)));
		}
		return num;
	}

	internal float4 GetPackedSettings(ConstraintType type)
	{
		return this[type].GetPackedSettings();
	}

	public int CalculateHash()
	{
		int value = DistanceConstraintSettings.CalculateHash();
		value = HashCode.Combine(BendConstraintSettings.CalculateHash(), value);
		value = HashCode.Combine(ShapeConstraintSettings.CalculateHash(), value);
		value = HashCode.Combine(AngularConstraintSettings.CalculateHash(), value);
		value = HashCode.Combine(SimplexConstraintSettings.CalculateHash(), value);
		value = HashCode.Combine(FoliageConstraintSettings.CalculateHash(), value);
		return HashCode.Combine(AerodynamicsConstraintSettings.CalculateHash(), value);
	}
}
