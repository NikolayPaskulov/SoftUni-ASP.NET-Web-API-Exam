namespace BugTracker.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Text { get; set; }

        public string AuthorId { get; set; }

        public virtual User Author { get; set; }

        public DateTime PublishDate { get; set; }

        [Required]
        public int BugId { get; set; }

        public virtual Bug Bug { get; set; }
    }
}
