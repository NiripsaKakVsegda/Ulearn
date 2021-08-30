using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace uLearn.Web.Extensions
{
	public static class OwinRequestExtensions
	{
		public static async Task<Dictionary<string, string>> GetRequestParameters(this IOwinRequest request)
		{
			var bodyParameters = await request.GetBodyParameters().ConfigureAwait(false);
			var queryParameters = request.GetQueryParameters();
			var headerParameters = request.GetHeaderParameters();
			return bodyParameters.Concat(queryParameters).Concat(headerParameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static Dictionary<string, string> GetQueryParameters(this IOwinRequest request)
		{
			var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var pair in request.Query)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}

			return dictionary;
		}

		public static async Task<Dictionary<string, string>> GetBodyParameters(this IOwinRequest request)
		{
			var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
			var formCollection = await request.ReadFormAsync().ConfigureAwait(false);
			foreach (var pair in formCollection)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}

			return dictionary;
		}

		public static Dictionary<string, string> GetHeaderParameters(this IOwinRequest request)
		{
			var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var pair in request.Headers)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}

			return dictionary;
		}

		private static string GetJoinedValue(string[] value)
		{
			return value != null ? string.Join(",", value) : null;
		}
	}
}