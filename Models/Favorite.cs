using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovixApp.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }   

        [Required]
        public string UserId { get; set; } = default!; 

        [Required]
        public int MovieId { get; set; }                

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        
        [ForeignKey("MovieId")]
        public Movie Movie { get; set; } = default!;
    }
}
