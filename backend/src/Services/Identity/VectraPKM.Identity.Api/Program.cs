using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VectraPKM Identity API",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((document, request) =>
        {
            var prefix = request.Headers["X-Forwarded-Prefix"].ToString();
            document.Servers = [new OpenApiServer
            {
                Url = string.IsNullOrEmpty(prefix) ? "/" : prefix
            }];
        });
    });
}

app.Run();
