using System;
using System.Collections;
using System.Collections.Generic;
using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        // note that we do not have Id, UserName, PasswordHash, or PasswordSalt anymore because that's provided by IdentityUser.
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // one to many - 1 user can have many photos
        public ICollection<Photo> Photos { get; set; }
        // implement like feature. LikedByUsers = list of users that like the currently logged in user
        public ICollection<UserLike> LikedByUsers { get; set; }
        // LikedUsers = list of users that the currently logged in user has liked
        public ICollection<UserLike> LikedUsers { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}