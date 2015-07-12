
namespace BugTracker.RestServices.BindingModels
{
    using BugTracker.Data.Models;

    public class BugPatchecBindingModel
    {
        public string Title { get; set; }

        public string Description  { get; set; }

        public BugStatuses Status { get; set; }
    }
}