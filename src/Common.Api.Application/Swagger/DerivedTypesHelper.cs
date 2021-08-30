using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ulearn.Common.Api.Swagger
{
	internal static class DerivedTypesHelper
	{
		public static IEnumerable<Type> GetDerivedTypes(Type baseType)
		{
			var derivedTypes = Assembly
				.GetAssembly(baseType)
				.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && (baseType.IsInterface ? baseType.IsAssignableFrom(t) : t.IsSubclassOf(baseType)));
			return derivedTypes;
		}

		public static Dictionary<Type, string> GetType2JsonTypeName(IEnumerable<Type> types)
		{
			var customDisplayNameTypes = types
				.Where(x => x
					.GetCustomAttributes(false)
					.Any(y => y is DisplayNameAttribute));

			var type2Name = customDisplayNameTypes
				.ToDictionary(
					t => t,
					t => t
						.GetCustomAttributes(false)
						.OfType<DisplayNameAttribute>()
						.First()
						.DisplayName
				);
			return type2Name;
		}
	}
}