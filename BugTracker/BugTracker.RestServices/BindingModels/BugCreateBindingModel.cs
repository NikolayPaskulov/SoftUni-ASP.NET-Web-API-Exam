
namespace BugTracker.RestServices.BindingModels
{

    using System.ComponentModel.DataAnnotations;

    public class BugCreateBindingModel
    {
        [Required]
        [MinLength(1)]
        public string Title { get; set; }

        public string Description  { get; set; }
    }
}