using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Kingmaker.MemoryPack.Formatters;
using Kingmaker.Pathfinding;
using OwlPack.Runtime;

[assembly: OwlPackExternalSerializer(typeof(ForcedPath), typeof(ForcedPathSerializer))]
[assembly: AssemblyVersion("0.0.0.0")]
