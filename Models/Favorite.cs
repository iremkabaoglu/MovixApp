using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovixApp.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }   // Otomatik Primary Key

        [Required]
        public string UserId { get; set; } = default!;  // Kullanıcı Id'si (AspNetUsers tablosundan)

        [Required]
        public int MovieId { get; set; }                // Favoriye eklenen film Id'si

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Eklenme zamanı

        // 🔹 Navigation property (ilişki)
        [ForeignKey("MovieId")]
        public Movie Movie { get; set; } = default!;
    }
}
