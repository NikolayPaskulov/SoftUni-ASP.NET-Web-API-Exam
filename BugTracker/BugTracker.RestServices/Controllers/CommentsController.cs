

namespace BugTracker.RestServices.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Description;
    using BugTracker.Data.Models;
    using BugTracker.Data;

    public class CommentsController : ApiController
    {
        private BugTrackerDbContext Data = new BugTrackerDbContext();

        // GET api/Comments
        [HttpGet]
        public IHttpActionResult GetComments()
        {
            var comments = Data.Comments
                               .OrderByDescending(c => c.PublishDate)
                               .Select(c => new
                               {
                                   c.Id,
                                   c.Text,
                                   Author = c.Author == null ? null : c.Author.UserName,
                                   DateCreated = c.PublishDate,
                                   c.BugId,
                                   BugTitle = c.Bug.Title
                               });

            return this.Ok(comments);
        }

       
    }
}