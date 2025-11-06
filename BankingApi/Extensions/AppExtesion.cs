namespace BankingApi.Extensions
{
    public static class AppExtesion
    {

        public static void UseSwaggerExtension (this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions();

                if (versionDescriptions != null && versionDescriptions.Any())
                {
                    foreach (var apiVersion in versionDescriptions) {
                        var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                        var name = $"Banking API- {apiVersion.GroupName.ToUpperInvariant()}";
                        opt.SwaggerEndpoint(url, name);

                    }
                }
            });
        }
    }
}
