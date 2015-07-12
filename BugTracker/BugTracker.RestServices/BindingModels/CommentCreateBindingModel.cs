
namespace BugTracker.RestServices.BindingModels
{
    using System.ComponentModel.DataAnnotations;

    public class CommentCreateBindingModel
    {
        [Required]
        [MinLength(1)]
        public string Text { get; set; }
    }
}