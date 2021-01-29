using System.Linq;
using API.DTOs;
using API.Entities;
using AutoMapper;
using API.Extensions;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        // helps us map from one object to another (in our case, user to member)
        public AutoMapperProfiles()
        {
            // We can specify for individual properties. When we map an individual property, we specify the destination property
            // (dest.PhotoUrl, in this case), then we tell it where we want to map from (src is Photos. We want the first or default, then get the Url property from that).
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => 
                    src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                    src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
        }
    }
}