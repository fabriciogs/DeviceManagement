using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Persistence;
using DeviceManagement.Application.Services;
using DeviceManagement.Application.Validators;
using DeviceManagement.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddResponseCaching().AddResponseCompression();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Device Management Api", Version = "v1", Contact = new OpenApiContact { Name = "Fabricio Gabrielli da Silva" } });

    opt.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    opt.AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [] });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    opt.IncludeXmlComments(xmlPath);
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "apiWithAuthBackend",
            ValidAudience = "apiWithAuthBackend",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("!SomethingSecret!"))
        };

        // https://stackoverflow.com/a/70173244
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Headers.Authorization;
                return Task.CompletedTask;
            },
        };
    });

// Dependency Injection
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection")!;
builder.Services.AddHealthChecks().AddSqlServer(connectionString);
builder.Services.AddScoped<IDeviceRepository>(_ =>
{
    var factory = new SqlServerConnectionFactory(connectionString);
    return new DapperDeviceRepository(factory.CreateConnection());
});
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IValidator<CreateDeviceDTO>, CreateDeviceValidator>();
builder.Services.AddScoped<IValidator<UpdateDeviceDTO>, UpdateDeviceValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");

app.Run();