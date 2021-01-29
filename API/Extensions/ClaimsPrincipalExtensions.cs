using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /*
            User.GetUsername() (in UsersController) gets the username from a User claims principle
            We don't want the user to provide this. We want to get it from what we're authenticating against which is the token.
            Inside a Controller we have access to a claims principle of the User. This contains info about their identity.
            We want to find the claim that matches the name identifier, which is the claim that we give the user in their token.
            This method is an extension method that helps us do this everytime we want to get the username from the User claims principle.
        */
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // ClaimTypes.Name is the UniqueName proeprty that we set inside our TokenService. (event thought it's a different name)
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            // ClaimTypes.NameIdentifier is the NameId proeprty that we set inside our TokenService. (event thought it's a different name)
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}