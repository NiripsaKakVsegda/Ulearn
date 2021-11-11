using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ulearn.Common
{
	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum InterpretNonJsonOutputType : short
	{
		[XmlEnum("SE")]
		[Lexer("SE")]
		SandboxError,
		
		[XmlEnum("WA")]
		[Lexer("WA")]
		WrongAnswer,

		[XmlEnum("CE")]
		[Lexer("CE")]
		CompilationError,
	}
}