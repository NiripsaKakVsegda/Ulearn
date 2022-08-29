// using Microsoft.AspNetCore.Authentication;
//
// namespace uLearn.Web.Core.External.Kontur;
//
// public class KonturPassportAuthenticationMiddleware : AuthenticationMiddleware //<KonturPassportAuthenticationOptions>
// {
// 	// private readonly PassportClient passportClient;
// 	//
// 	// //        public AuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
// 	// public KonturPassportAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider scheme, IApplicationBuilder app, KonturPassportAuthenticationOptions options)
// 	// 	: base(next, scheme)
// 	// {
// 	// 	passportClient = new PassportClient(options.ClientId, new []{"openid", "profile", "email"});
// 	//
// 	// 	if (Options.StateDataFormat == null)
// 	// 	{
// 	// 		var dataProtector = app.CreateDataProtector(
// 	// 			typeof(KonturPassportAuthenticationMiddleware).FullName,
// 	// 			Options.AuthenticationType,
// 	// 			"v1");
// 	// 		Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
// 	// 	}
// 	// }
// 	//
// 	// protected override AuthenticationHandler CreateHandler() //<KonturPassportAuthenticationOptions>
// 	// {
// 	// 	return new KonturPassportAuthenticationHandler(passportClient);
// 	// }
// }