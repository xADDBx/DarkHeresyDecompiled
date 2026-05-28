using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using OwlPack.Runtime;
using OwlPack.Unity;
using UnityEngine;

[assembly: OwlPackExternalSerializer(typeof(Vector2), typeof(Vector2Serializer))]
[assembly: OwlPackExternalSerializer(typeof(Vector3), typeof(Vector3Serializer))]
[assembly: OwlPackExternalSerializer(typeof(Vector4), typeof(Vector4Serializer))]
[assembly: OwlPackExternalSerializer(typeof(Quaternion), typeof(QuaternionSerializer))]
[assembly: AssemblyVersion("0.0.0.0")]
