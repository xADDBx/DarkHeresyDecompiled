using System;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(menuName = "Owlcat/CharacterStudio/Portrait Preset", fileName = "PortraitPreset")]
public class CharacterStudioPortraitPreset : ScriptableObject
{
	public enum BodyPart
	{
		Head,
		Torso,
		Legs,
		Arms,
		Eyebrows,
		Facial
	}

	[Serializable]
	public struct PortraitPose
	{
		public float PositionX;

		public float PositionY;

		public float PositionZ;

		public float RotationX;

		public float RotationY;

		public float RotationZ;

		public float Fov;

		public Vector3 Position => new Vector3(PositionX, PositionY, PositionZ);

		public Vector3 Rotation => new Vector3(RotationX, RotationY, RotationZ);
	}

	public CharacterStudio.Race Race;

	public CharacterStudio.Gender Gender;

	[HideInInspector]
	public PortraitPose Head;

	[HideInInspector]
	public PortraitPose Torso;

	[HideInInspector]
	public PortraitPose Legs;

	[HideInInspector]
	public PortraitPose Arms;

	[HideInInspector]
	public PortraitPose Eyebrows;

	[HideInInspector]
	public PortraitPose Facial;

	public PortraitPose GetPose(BodyPart part)
	{
		return part switch
		{
			BodyPart.Torso => Torso, 
			BodyPart.Legs => Legs, 
			BodyPart.Arms => Arms, 
			BodyPart.Eyebrows => Eyebrows, 
			BodyPart.Facial => Facial, 
			_ => Head, 
		};
	}
}
