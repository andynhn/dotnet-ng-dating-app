using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        
        // fully define the relationship with AppUser. EF will make AppUserId nullable. 
        // And make onDelete ReferentialAction.Cascade because this is a one to many relationship between AppUsers and Photos.
        // cascade so that if a user is deleted, their connected photos will be deleted as well
        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }
    }
}