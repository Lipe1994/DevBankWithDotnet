using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace DevBankWithDotnet.ErrorHandler;

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseErrorHandler(
                                      this IApplicationBuilder appBuilder,
                                      ILoggerFactory loggerFactory)
    {
        return appBuilder.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerFeature = context
                                                .Features
                                                .Get<IExceptionHandlerFeature>();

                if (exceptionHandlerFeature != null)
                {
                    var logger = loggerFactory.CreateLogger("ErrorHandler");
                    logger.LogError($"Error: {exceptionHandlerFeature.Error}");

                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

                    await context.Response.WriteAsync("");
                }
            });
        });
    }
}