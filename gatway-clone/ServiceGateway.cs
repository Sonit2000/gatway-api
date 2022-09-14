
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Data;
using gatway_clone.Objects;
using gatway_clone.Utils;
using gatway_clone;
using gatway_clone.Utils.CacheBase;
using gatway_clone.ThreadPools;
using gatway_clone.Examples;

namespace ServiceGateway;

public class ServiceGateway : WorkerService
{
    public static Task Main(string[] args) => RunAsync<ServiceGateway>(args,
             hostBuilder => hostBuilder.ConfigureWebHostDefaults(configure =>
                 configure.UseStartup<ServiceGateway>()
                 .UseUrls(Configuration.ApplicationUrl)
                 .UseKestrel(options => options.AddServerHeader = false)));


    protected override async Task StartAsync()
    {
        CacheFactory.Init();
        SettlementThreadPool.Init();
        if (Configuration.AutoSettlement)
        {
            TimerUtil.RegisterTimer(async () =>
            {
                string lmid = LogUtil.GenerateLMID() + "_AUTOSETTLEMENT";
                try
                {
                    //if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
                    //{
                    //    await DeviceDailyLimitCache.Instance.ReloadAsync(lmid);
                    //    await MerchantDailyLimitCache.Instance.ReloadAsync(lmid);
                    //}
                    //TerminalDB terminalDb = new();
                    //foreach (DataRow dr in terminalDb.GetTerminalSettlement().Rows)
                    //{
                    //    TerminalObject terminal = await TerminalCache.Instance.GetAsync(lmid, dr["MerchantCode"].ToString(), dr["TerminalCode"].ToString(), dr["BankCode"].ToString());
                    //    if (terminal != null)
                    //    {
                    //        terminal.LMID = LogUtil.GenerateLMID() + "_AUTOSETTLEMENT";
                    //        terminal.IsAutoSettlement = true;
                    //        await SettlementThreadPool.Enqueue(terminal);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, lmid);
                }
            }, TimeSpan.FromMinutes(1), name: "AutoSettlementTimer");
        }
        await NginxUtil.Join();
    }

    protected override async Task StopAsync()
    {
       
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _logger.LogInformation("Configuring Services");
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddAuthentication(options => options.DefaultAuthenticateScheme = options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = JWTUtil.ValidationTokenParams;
        });
        IMvcBuilder mvcBuilder = services.AddControllers(options =>
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes<IInputFormatter>("InputFormatter"))
            {
                options.InputFormatters.Insert(0, Activator.CreateInstance(type) as IInputFormatter);
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes<IOutputFormatter>("OutputFormatter"))
            {
                options.OutputFormatters.Insert(0, Activator.CreateInstance(type) as IOutputFormatter);
            }
        });
        mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
        mvcBuilder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                if (actionContext is ActionExecutingContext context && context.ActionArguments.Values.FirstOrDefault() is RequestDTO requestDTO)
                {
                    _logger.LogDebug($"{requestDTO.LMID}: Service: {actionContext.HttpContext.Request.Path.ToString()[1..]}. IP: {actionContext.HttpContext.GetIP()}. User: {actionContext.HttpContext.User.Identity?.Name ?? "guest"}. Body: {DataMasking.MaskJson(context.ActionArguments.Values.FirstOrDefault())}");
                    ResponseDTO<object> responseDTO = new()
                    {
                        LMID = requestDTO.LMID,
                        CreatedDate = requestDTO.CreatedDate,
                        Data = context.ModelState.Select(modelError => new { ErrorField = modelError.Key, ErrorDescription = modelError.Value.Errors.Select(x => x.ErrorMessage) }),
                        RequestDateTime = requestDTO.RequestDateTime,
                        RequestID = requestDTO.RequestID,
                        ResponseCode = ResponseStatus.BadRequest.GetResponseCode(),
                        Status = ResponseStatus.BadRequest,
                        Description = ResponseStatus.BadRequest.GetDescription("en")
                    };
                    _logger.LogDebug($"{requestDTO.LMID}: RC: {responseDTO.ResponseCode}[{responseDTO.Status}]. Elapsed: {DateTime.Now.Subtract(responseDTO.CreatedDate)}. ResData: {DataMasking.MaskJson(responseDTO.Data) ?? "null"}. Desc: {responseDTO.Description}");
                    return new OkObjectResult(responseDTO);
                }
                return new BadRequestObjectResult(actionContext.ModelState);
            };
        });
        if (Configuration.IsEnableSwagger)
        {
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.EnableAnnotations(true, true);
                setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
                });
                setupAction.OperationFilter<ExamplesOperationFilter>();
                setupAction.TagActionsBy(api => new[] { api.RelativePath.Split('/')[0] });
                setupAction.DocInclusionPredicate((name, api) => true);
            });
        }
        _logger.LogInformation("Configured Services");
    }

    public void Configure(IApplicationBuilder app)
    {
        _logger.LogInformation("Configuring");
        if (Configuration.IsEnableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/v1/swagger.json", Configuration.AppName);
                setupAction.DisplayRequestDuration();
                setupAction.DefaultModelRendering(ModelRendering.Model);
                setupAction.DefaultModelsExpandDepth(0);
                setupAction.EnableDeepLinking();
            });
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
        }
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        _logger.LogInformation("Configured");
    }
}