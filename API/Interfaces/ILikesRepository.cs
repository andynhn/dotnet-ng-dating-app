using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        // predicate meaning "what are we looking for: list of users that have been liked or liked by?"
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}