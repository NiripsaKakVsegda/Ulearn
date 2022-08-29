using System.IdentityModel.Tokens.Jwt;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Core.External.Kontur;

public class PassportClient
{
	private readonly string clientId;
	private readonly string[] scopes;
	private const string issuerUriString = "https://passport.skbkontur.ru/";
	private const string authority = issuerUriString + "v3/";
	private const string authorizationEndpoint = authority + "connect/authorize";
	private readonly SigningKeysProvider signingKeysProvider;

	public PassportClient(string clientId, string[] scopes)
	{
		this.clientId = clientId;
		this.scopes = scopes;
		signingKeysProvider = new SigningKeysProvider(authority);
	}
		
	public AuthenticationResult Authenticate(
		IDictionary<string, string> queryString,
		string state)
	{
		if (queryString == null)
			throw new ArgumentNullException(nameof (queryString));
		var authenticationResult = new AuthenticationResult();
		if (queryString.ContainsKey("error"))
		{
			authenticationResult.ErrorMessage = $"Не удалось выполнить вход. Сервер вернул ошибку: {queryString["error"]}";
			return authenticationResult;
		}
		if (!queryString.ContainsKey("id_token"))
			return null;
		if (!string.IsNullOrEmpty(state) && (!queryString.ContainsKey(nameof (state)) || queryString[nameof (state)] != state))
		{
			authenticationResult.ErrorMessage = "Не удалось выполнить вход. Сервер вернул неверное состояние";
			return authenticationResult;
		}
		var validatedToken = GetValidatedToken(queryString["id_token"]);
		authenticationResult.Claims = validatedToken.Claims;
		authenticationResult.Authenticated = true;
		return authenticationResult;
	}
		
	public Uri GetLoginUri(Uri redirectUri, string state)
	{
		var request = new RequestUrl(authorizationEndpoint);
		var lower = string.Join(" ", scopes).ToLower();
		var absoluteUri = redirectUri.AbsoluteUri;
		var state1 = state.NullIfEmptyOrWhitespace();
		var nonce = Guid.NewGuid().ToString("N");
		return new Uri(request.CreateAuthorizeUrl(clientId, "id_token", lower, absoluteUri, state1, nonce, responseMode: "form_post"));
	}

	private JwtSecurityToken GetValidatedToken(string token)
	{
		var keys = signingKeysProvider.GetSigningKeysAsync().Result;
		var securityTokenHandler = new JwtSecurityTokenHandler();
		var validationParameters = new TokenValidationParameters
		{
			ValidIssuer = issuerUriString,
			ValidAudience = clientId,
			IssuerSigningKeys = keys,
			ValidateIssuerSigningKey = true,
			RequireExpirationTime = true
		};
		try
		{
			securityTokenHandler.ValidateToken(token, validationParameters, out var securityToken);
			return (JwtSecurityToken) securityToken;
		}
		catch (SecurityTokenException ex)
		{
			throw new ArgumentException("Токен не свалидирован. Подробности в innerException", nameof (token), ex);
		}
	}
}