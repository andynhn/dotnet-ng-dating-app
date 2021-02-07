using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
            IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            /*
                User.GetUsername() gets the username from a User claims principle
                We don't want the user to provide this. We want to get it from what we're authenticating against which is the token.
                Inside a Controller we have access to a claims principle of the User. This contains info about their identity.
                We want to find the claim that matches the name identifier, which is the claim that we give the user in their token.
            */
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            // if gender is empty in the userParams, auto set it. So if the user is male, default the params gender to female. 
            // if the user is female, default the params gender to male (so that the query returns that gender)
            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female" : "male";

            // filters the returned users based on our userParams from the query
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            // we always have access to the Response in here.
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            /*
                we want to use AutoMapper here because it helps us be more specific with the data that we send back to the client.
                automapper maps user data that we specify in MemberDto.cs (e.g. we exclude the Password, etc.).
                AutoMapper does this partly by comparing the property names between the objects.
                var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);  // the source object goes in ().
                need the Ok() result here because of IEnumerable
            */
            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _unitOfWork.UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            /*
                This gives us the user's username from the token that the api uses to authenticate this user.
                we made a ClaimsPrincipalExtension helper method for that.
                this is who we are updating in this case. 
            */
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // get user from the ClaimsPrincipal (extension method for this)
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            // get results back from photo service
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                // this result is coming from cloudinary.
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // if first photo uploading, set to main
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                // send route name, route values, and value in order to send back a 201.
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            // validating that this is the user we authenticated based on the JWT. Eager Loading here gives us the photos as well.
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            // turn current main IsMain to false.
            if (currentMain != null) currentMain.IsMain = false;

            // set new main IsMain to true.
            photo.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            // if the photo is in cloudinary
            if (photo.PublicId != null)
            {
                // delete the photo from cloudinary 
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            // remove the photo from our db
            user.Photos.Remove(photo);

            // save db changes
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}