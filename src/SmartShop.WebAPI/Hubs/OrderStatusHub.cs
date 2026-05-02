using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartShop.WebAPI.Hubs;

[Authorize]
public class OrderStatusHub : Hub;

