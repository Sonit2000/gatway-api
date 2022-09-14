using gatway_clone.Objects;
using gatway_clone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace gatway_clone.Controllers;
//[Authorize]
[ApiController]
public abstract class ServiceController<TRequest, TResponse> : ControllerBase
     where TRequest : class
     where TResponse : class, new()
{
    protected readonly ILogger _logger = LogUtil.CreateLogger();
    protected int UserID => int.Parse(User.FindFirstValue("UserID") ?? "0");
    protected TimeSpan FunctionTimeout { get; set; } = Configuration.DefaultFunctionTimeout;
    private readonly TimeSpan _maxDriftTimestamp = Configuration.MaxDriftTimestamp;

    protected RequestDTO<TRequest> RequestDTO;
    protected TRequest RequestData => RequestDTO.Data;

    private TResponse _responseData;
    protected TResponse ResponseData { get { return _responseData ??= new TResponse(); } set { _responseData = value; } }

    [HttpPost("[controller]")]
    public async Task<ResponseDTO<TResponse>> Post([FromBody] RequestDTO<TRequest> requestDTO)
    {
        if (requestDTO == null && !HttpContext.Items.ContainsKey("SessionObject"))
        {
            Response.StatusCode = 401;
            return null;
        }
        RequestDTO = requestDTO;
        string path = Request.Path.ToString().ToLower();
        ResponseDTO<TResponse> responseDTO = null;
        _logger.LogDebug($"{RequestDTO.LMID}: Service: {path[1..]}. IP: {HttpContext.GetIP()}. User: {HttpContext.User.Identity?.Name ?? "guest"}. ReqID: {RequestDTO.RequestID}. ReqDateTime: {RequestDTO.RequestDateTime}. Language: {requestDTO.Language}. X-Session-ID: {Request.Headers["X-Session-ID"].FirstOrDefault()}");
        if (!string.IsNullOrEmpty(requestDTO.UserAgent))
        {
            _logger.LogDebug($"{RequestDTO.LMID}: UserAgent: {requestDTO.UserAgent}");
        }
        try
        {
            _logger.LogDebug($"{RequestDTO.LMID}: ReqData: {DataMasking.MaskJson(requestDTO.Data)}");
            if (User.Identity.IsAuthenticated && !User.IsInRole(path)) //check authorization
            {
                responseDTO = await BuildResponse(ResponseStatus.UserForbidden);
            }
            //else if (await RequestIDCache.Instance.GetAsync(RequestDTO.LMID, RequestDTO.RequestID) != null) //check duplicate request
            //{
            //    responseDTO = await BuildResponse(ResponseStatus.DuplicationRequest);
            //}
            else if (!DateTime.TryParse(requestDTO.RequestDateTime, out DateTime requestDateTime) || Math.Abs((DateTime.Now - requestDateTime).TotalSeconds) > _maxDriftTimestamp.TotalSeconds)
            {
                responseDTO = await BuildResponse(ResponseStatus.InvalidDateTime);
            }
            else if (HttpContext.Items.ContainsKey("IsValidSignature") && !(bool)HttpContext.Items["IsValidSignature"])
            {
                responseDTO = await BuildResponse(ResponseStatus.InvalidSignature);
            }
            else
            {
                //await RequestIDCache.Instance.AddAsync(RequestDTO.LMID, RequestDTO.RequestID);
                Task<ResponseDTO<TResponse>> task = Process().HandleExceptions(requestDTO.LMID);
                GlobalTasks.Tasks.Add(task);
                responseDTO = await task.TimeoutAfter(FunctionTimeout) ? await ProcessTimeout() : await task;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RequestDTO.LMID);
            responseDTO = await BuildResponse(ResponseStatus.InternalServerError);
        }
        responseDTO.ResponseCode = responseDTO.Status.GetResponseCode();
        //responseDTO.Description = responseDTO.Status.GetDescription(requestDTO.Language);
        //_logger.LogDebug($"{RequestDTO.LMID}: RC: {responseDTO.ResponseCode}[{responseDTO.Status}]. Elapsed: {DateTime.Now.Subtract(responseDTO.CreatedDate)}. ResData: {DataMasking.MaskJson(responseDTO.Data) ?? "null"}. Desc: {responseDTO.Description}");
        return responseDTO;
    }

    protected abstract Task<ResponseDTO<TResponse>> Process();

    protected virtual Task<ResponseDTO<TResponse>> ProcessTimeout() => BuildResponse(ResponseStatus.RequestTimeout);

    protected Task<ResponseDTO<TResponse>> BuildResponse(ResponseStatus responseStatus)
        => Task.FromResult(new ResponseDTO<TResponse>
        {
            LMID = RequestDTO.LMID,
            CreatedDate = RequestDTO.CreatedDate,
            Data = _responseData,
            RequestDateTime = RequestDTO.RequestDateTime,
            RequestID = RequestDTO.RequestID,
            Status = responseStatus
        });
}