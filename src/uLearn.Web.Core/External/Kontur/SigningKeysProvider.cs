using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using IdentityModel.Client;

namespace uLearn.Web.Core.External.Kontur;

public class SigningKeysProvider
{
	private readonly DiscoveryCache cache;

	public SigningKeysProvider(string authority)
	{
		var policy = new DiscoveryPolicy
		{
			ValidateIssuerName = false
		};
		cache = new DiscoveryCache(authority, () => new HttpClient(), policy);
	}

	public async Task<List<SecurityKey>> GetSigningKeysAsync()
	{
		return (await cache.GetAsync().ConfigureAwait(false)).KeySet.GetSigningKeys();
	}
}

public static class JwksExtensions
{
	public static List<SecurityKey> GetSigningKeys(this IdentityModel.Jwk.JsonWebKeySet jwks)
	{
		var securityKeyList = new List<SecurityKey>();
		foreach (var key in jwks.Keys)
		{
			if (!StringComparer.Ordinal.Equals(key.Kty, "RSA") || (!string.IsNullOrWhiteSpace(key.Use) && !StringComparer.Ordinal.Equals(key.Use, "sig")))
				continue;
			if (key.X5c != null)
			{
				foreach (var s in key.X5c)
				{
					try
					{
						SecurityKey securityKey = new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(s)));
						securityKey.KeyId = key.Kid;
						securityKeyList.Add(securityKey);
					}
					catch (CryptographicException ex)
					{
						throw new InvalidOperationException($"Unable to create an X509Certificate2 from the X509Data: '{s}'. See inner exception for additional details.", ex);
					}
					catch (FormatException ex)
					{
						throw new InvalidOperationException($"Unable to create an X509Certificate2 from the X509Data: '{s}'. See inner exception for additional details.", ex);
					}
				}
			}

			if (string.IsNullOrWhiteSpace(key.E))
				continue;
			{
				if (string.IsNullOrWhiteSpace(key.N))
					continue;
				try
				{
					var rsaParameters = new RSAParameters
					{
						Exponent = Base64UrlEncoder.DecodeBytes(key.E),
						Modulus = Base64UrlEncoder.DecodeBytes(key.N)
					};
					SecurityKey securityKey = new RsaSecurityKey(rsaParameters);
					securityKey.KeyId = key.Kid;
					securityKeyList.Add(securityKey);
				}
				catch (CryptographicException ex)
				{
					throw new InvalidOperationException($"Unable to create an RSA public key from the Exponent and Modulus found in the JsonWebKey: E: '{key.E}', N: '{key.N}'. See inner exception for additional details.", ex);
				}
				catch (FormatException ex)
				{
					throw new InvalidOperationException($"Unable to create an RSA public key from the Exponent and Modulus found in the JsonWebKey: E: '{key.E}', N: '{key.N}'. See inner exception for additional details.", ex);
				}
			}
		}

		return securityKeyList;
	}
}