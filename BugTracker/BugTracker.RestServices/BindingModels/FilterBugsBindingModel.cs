
namespace BugTracker.RestServices.BindingModels
{
    public class FilterBugsBindingModel
    {
        public string Keyword { get; set; }

        public string Statuses { get; set; }

        public string Author { get; set; }
    }
}