using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace CsSandboxer.Core;

[Serializable]
public class Sandboxer : MarshalByRefObject
{
	public void ExecuteUntrustedCode(MethodInfo entryPoint)
	{
		//new PermissionSet(PermissionState.Unrestricted).Assert();
		var parameters = entryPoint.GetParameters().Length != 0 ? new object[] { new[] { "" } } : null;
		entryPoint.Invoke(null, parameters);

		//CodeAccessPermission.RevertAssert();
	}

	#region Security Test

	public static void MustNotWork()
	{
		Console.WriteLine("Security broken!!!");
	}

	public static int Secret = 42;

	#endregion
}