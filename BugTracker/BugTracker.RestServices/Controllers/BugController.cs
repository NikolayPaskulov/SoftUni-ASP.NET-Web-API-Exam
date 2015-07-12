

namespace BugTracker.RestServices.Controllers
{

    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Description;
    using BugTracker.Data.Models;
    using BugTracker.Data;
    using BugTracker.RestServices.BindingModels;
    using Microsoft.AspNet.Identity;

    [RoutePrefix("api/bugs")]
    public class BugsController : ApiController
    {
        private BugTrackerDbContext Data = new BugTrackerDbContext();

        // GET api/Bugs
        [HttpGet]
        public IHttpActionResult GetBugs()
        {
            var bugsToReturn = Data.Bugs
                                   .OrderByDescending(b => b.Date)
                                   .Select(b => new
                                   {
                                       b.Id,
                                       b.Title,
                                       Status = b.Status.ToString(),
                                       Author = (b.Author == null) ? null : b.Author.UserName,
                                       DateCreated = b.Date
                                   }).ToList();

            return this.Ok(bugsToReturn);
        }

        // GET api/Bugs/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetBugById(int id)
        {
            var bug = Data.Bugs.Find(id);
            if (bug == null)
            {
                return NotFound();
            }

            var bugComments = bug.Comments
                                 .OrderByDescending(c => c.PublishDate)
                                 .Select(c => new
                                 {
                                     c.Id,
                                     c.Text,
                                     Author = c.Author == null ? null : c.Author.UserName,
                                     c.PublishDate
                                 });

            return Ok(
                new
                {
                    bug.Id,
                    bug.Title,
                    bug.Description,
                    Author = bug.Author == null ? null : bug.Author.UserName,
                    Status = bug.Status.ToString(),
                    Comments = bugComments.ToList()
                }
            );
        }

        // POST api/Bugs
        [HttpPost]
        public IHttpActionResult PostBug(BugCreateBindingModel model)
        {

            if(model == null) {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.Find(currentUserId);

            var bug = new Bug()
            {
                Title = model.Title,
                Description = model.Description,
                Status = BugStatuses.Open,
                Date = DateTime.Now,
                AuthorId = currentUser == null ? null : currentUser.Id
            };

            Data.Bugs.Add(bug);
            Data.SaveChanges();

            if (currentUser == null)
            {
                return CreatedAtRoute(
                        "DefaultApi",
                        new { Id = bug.Id },
                        new { Id = bug.Id, Message = "Anonymous bug submitted." });
            }

            return CreatedAtRoute(
                    "DefaultApi",
                    new { Id = bug.Id },
                    new
                    {
                        Id = bug.Id,
                        Message = "User bug submitted.",
                        Author = currentUser.UserName
                    });
        }

        // PATCH api/Bug/5
        [HttpPatch]
        [Route("{id}")]
        public IHttpActionResult PatchBug(int id, BugPatchecBindingModel model)
        {
            if (model == null)
            {
                return this.Ok(new { Message = "Nothing is changed!" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bug = Data.Bugs.Find(id);

            if (bug == null)
            {
                return this.NotFound();
            }

            if (model.Title != null)
            {
                bug.Title = model.Title;
            }

            if (model.Description != null)
            {
                bug.Description = model.Description;
            }

            if (model.Status != 0)
            {
                bug.Status = model.Status;
            }

            Data.SaveChanges();

            return this.Ok(new
            {
                Message = "Bug #"+ id +" patched."
            }); 
        }

       

        // DELETE api/Bug/5
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteBug(int id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bug = Data.Bugs.Find(id);

            if (bug == null)
            {
                return this.NotFound();
            }

            Data.Bugs.Remove(bug);
            Data.SaveChanges();

            return this.Ok(new
            {
                Message = "Bug #" + id + " deleted."
            }); 
        }

        [HttpGet]
        [Route("filter")]
        public IHttpActionResult FilterBugs([FromUri]FilterBugsBindingModel model)
        {

            if (model == null)
            {
                return this.Ok(new { Message = "No filter passed to URI!" }); 
            }

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }


            var bugs = Data.Bugs.AsQueryable();

            if (model.Author != null && model.Author != "")
            {
                bugs = bugs.Where(b => b.Author.UserName == model.Author);
            }

            if (model.Statuses != null && model.Statuses != "")
            {
                var statuses = model.Statuses.Split('|');
                bugs = bugs.Where(b => statuses.Contains(b.Status.ToString()));
            }

            if (model.Keyword != null && model.Keyword != "")
            {
                bugs = bugs.Where(b => b.Title.IndexOf(model.Keyword) >= 0);
            }

            var bugsToReturn = bugs.OrderByDescending(b => b.Date)
                                   .Select(b => new
                                   {
                                       b.Id,
                                       b.Title,
                                       Status = b.Status.ToString(),
                                       DateCreated = b.Date,
                                       Author = b.Author == null ? null : b.Author.UserName
                                   });

            return this.Ok(bugsToReturn);
        }

        // GET /api/bugs/{id}/comments
        [HttpGet]
        [Route("{id}/comments")]
        public IHttpActionResult GetCommentsById(int id)
        {
            var bug = Data.Bugs.Find(id);

            if (bug == null)
            {
                return this.NotFound();
            }

            var bugComments = bug.Comments
                                 .OrderByDescending(c => c.PublishDate)
                                 .Select(c => new
                                 {
                                     c.Id,
                                     c.Text,
                                     Author = c.Author == null ? null : c.Author.UserName,
                                     DateCreated = c.PublishDate
                                 });

            return Ok(bugComments);
        }

        // POST /api/bugs/{id}/comments
        [HttpPost]
        [Route("{id}/comments")]
        public IHttpActionResult PostCommentInBug(int id, CommentCreateBindingModel model)
        {
            var bug = Data.Bugs.Find(id);

            if (bug == null)
            {
                return this.NotFound();
            }

            if (model == null)
            {
                return this.BadRequest();
            }

            if(!ModelState.IsValid) {
                return this.BadRequest(this.ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.Find(currentUserId);

            var comment = new Comment()
            {
                Text = model.Text,
                BugId = bug.Id,
                PublishDate = DateTime.Now,
                Author = currentUser
            };

            Data.Comments.Add(comment);
            Data.SaveChanges();

            if (currentUser == null)
            {
                return this.Ok(new
                {
                    Id = comment.Id,
                    Message = "Added anonymous comment for bug #" + id
                });
            }

            return this.Ok(new
            {
                Id = comment.Id,
                Author = currentUser.UserName,
                Message = "User comment added for bug #" + id
            });
        }
    }
}