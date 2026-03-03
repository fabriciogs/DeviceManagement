using DeviceManagement.Application.Notifications;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace DeviceManagement.Api.Filters;

public class NotificationFilter(NotificationContext notificationContext) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (notificationContext.HasNotifications)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;

            var notifications = JsonSerializer.Serialize(notificationContext.Notifications, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true // Optional: for pretty-printed JSON
            });
            await context.HttpContext.Response.WriteAsync(notifications);

            return;
        }

        await next();
    }
}