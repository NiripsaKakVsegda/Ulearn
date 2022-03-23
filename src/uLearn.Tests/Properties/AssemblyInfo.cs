using System.Runtime.InteropServices;
using ApprovalTests.Reporters;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d91a04bc-1edd-473e-932e-07feac6f83e9")]

[assembly: UseReporter(typeof(DiffReporter), typeof(ClipboardReporter))]