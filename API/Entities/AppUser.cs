using System;
using System.Collections;
using System.Collections.Generic;
using API.Extensions;

namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
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

        // need "Get" specifically because we will be using AutoMapper. 
        // we set a property in the MemberDto called Age. AutoMapper will know to populate it with the value returned from this GetAge method.
        // public int GetAge()
        // {
        //     return DateOfBirth.CalculateAge();
        // }
    }
}