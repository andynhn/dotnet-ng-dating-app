using System;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    /*
        Class contains methods that help log user activity, so that we can update LastActive properties
    */
    public class LogUserActivity : IAsyncActionFilter
    {
        /*
            we get acess to the context before
            we return the action executed after.
            This method will be used to update the LastActive property for a logged in user to NOW
        */
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            // make sure user is authenticated first.
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            // get userId from User claims principle and extension method.
            var userId = resultContext.HttpContext.User.GetUserId();
            // get repository
            var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            // get user from repository using username
            var user = await repo.GetUserByIdAsync(userId);
            // set LastActive to Now.
            user.LastActive = DateTime.Now;
            // async save to repository.
            await repo.SaveAllAsync();
            // NOTE: and make sure to add this to AddApplicationServices
            // AND: then add this to our BaseApiController so that all of our controllers have access to this.
        }
    }
}