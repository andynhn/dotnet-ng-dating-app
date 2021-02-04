using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    // Signal R hub that allows for tracking presences on our app (online vs offline, etc.)
    /*
        But we have no way of knowing exactly who is cnnected inside here alone
        Because if we were in a web farm and we had more than one server, we'd have no way of 
        getting the connection information from the other server. This service is confined to the server it's running on.
        Scalable Strategy: Redis, scalable, stores the tracking infromation which can be distributed among many different servers
        What we will do instead for this small app: Create a class to track who is connected and store that in a dictionary
        Just know that to scale, need to use a service like Redis or store this info in your database.
    */
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        // OnConnectedAsync() is virtual so we can override it.
        public override async Task OnConnectedAsync()
        {
            // update our presence tracker with the connected user...
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);


            // "UserIsOnline" is the name of the client-side method.
            // we have access to the Context with the token inside.
            // note that Signal R (or web sockets) cannot send an Authentication header.
            // We need to use a query string with Signal R
            // only send UserIsOnline to the connected clients, apart from the caller.
            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }
            // everytime a user connects to a hub, they are given a connection id.


            // send the updated list of current users back to everyone that is connected
            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            // "UserIsOnline" is the name of the client-side method.
            // we have access to the Context with the token inside.
            if (isOffline) 
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }

            // pass up the exception to the base, the parent class.
            await base.OnDisconnectedAsync(exception);
        }
    }
}