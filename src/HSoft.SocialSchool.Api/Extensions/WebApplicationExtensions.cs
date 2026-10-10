namespace HSoft.SocialSchool.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task Configure(this WebApplication app)
        {
            var swaggerEndpoint = "swagger";

            app.UseStaticFiles();
            app.UseSwagger(c =>
                c.RouteTemplate = $"{swaggerEndpoint}/{{documentName}}/swagger.json"
            );
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/{swaggerEndpoint}/v1/swagger.json", $"{Domain.Constants.Application.ApiName} v{Domain.Constants.Application.Version}");
                c.InjectStylesheet("/swagger/custom.css");
            });
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }

        public static void LogRuntimeInformation(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<WebApplication>>();
            logger.LogWarning("Application started. Environment: {Environment}", app.Environment.EnvironmentName);
            logger.LogInformation("Application started. Environment: {Environment}", app.Environment.EnvironmentName);
        }
    }
}
