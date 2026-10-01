var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api/identity/swagger/v1/swagger.json", "Identity v1");
        options.DocumentTitle = "VectraPKM API";
    });
}

app.MapReverseProxy();

app.Run();
