

namespace BugTracker.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using BugTracker.Data.Models;
    using BugTracker.Tests.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BugCommentsIntegrationTests
    {

        [TestMethod]
        public void CreateComments_ListThem_ShouldListCreatedCommentsCorrectly()
        {
            TestingEngine.CleanDatabase();

            var bug = new BugModel()
            {
                Title = "Comment Test",
                Description = "Comment Test"
            };

            var httpPostResponse = TestingEngine.SubmitBugHttpPost(bug.Title, bug.Description);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);

            var createdBug = TestingEngine.HttpClient.GetAsync("/api/bugs").Result;
            var bugsFromService = createdBug.Content.ReadAsAsync<List<BugModel>>().Result;


            var commentsToAdds = new CommentModel[]
            {
                new CommentModel() { Text = "First Comment" },
                new CommentModel() { Text = "Second Comment"},
                new CommentModel() { Text = "Third Comment" }
            };

            // Act -> submit a few bugs
            foreach (var comment in commentsToAdds)
            {
                var commentPost = TestingEngine.SubmitCommentHttpPost(bugsFromService[0].Id, comment.Text);
                Thread.Sleep(500);

                // Assert -> ensure each bug is successfully submitted
                Assert.AreEqual(HttpStatusCode.OK, commentPost.StatusCode);
            }

            // Assert -> list the bugs and assert their count, order and content are correct
            var httpResponse = TestingEngine.HttpClient.GetAsync("/api/comments").Result;
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);

            var commentsFromService = httpResponse.Content.ReadAsAsync<List<CommentModel>>().Result;
            Assert.AreEqual(commentsToAdds.Count(), commentsFromService.Count);

            var reversedAddedComments = commentsToAdds.Reverse().ToList();
            for (int i = 0; i < reversedAddedComments.Count; i++)
            {
                Assert.IsTrue(commentsFromService[i].Id != 0);
                Assert.AreEqual(reversedAddedComments[i].Text, commentsFromService[i].Text);
            }
        }

        [TestMethod]
        public void CreateComments_AddThemToBug_ListThem_ShouldListCurrentComments()
        {
            TestingEngine.CleanDatabase();

            var bug1 = new BugModel()
            {
                Title = "First Bug",
                Description = "Comment Test"
            };

            var bug2 = new BugModel()
            {
                Title = "Sec Bug",
                Description = "Comment Test"
            };

            var httpPostResponseBug1 = TestingEngine.SubmitBugHttpPost(bug1.Title, bug1.Description);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponseBug1.StatusCode);

            var httpPostResponseBug2 = TestingEngine.SubmitBugHttpPost(bug2.Title, bug2.Description);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponseBug2.StatusCode);

            var createdBug = TestingEngine.HttpClient.GetAsync("/api/bugs").Result;
            var bugsFromService = createdBug.Content.ReadAsAsync<List<BugModel>>().Result;

            var commentToBug1 = new CommentModel() { Text = "First Comment" };
            var commentToBug2 =  new CommentModel() { Text = "Second Comment"};

            var commentPost1 = TestingEngine.SubmitCommentHttpPost(bugsFromService[1].Id, commentToBug1.Text);
            Thread.Sleep(500);

            var commentPost2 = TestingEngine.SubmitCommentHttpPost(bugsFromService[0].Id, commentToBug2.Text);
            Thread.Sleep(500);

            // Assert -> list the bugs and assert their count, order and content are correct
            var httpResponseBug1 = TestingEngine.HttpClient.GetAsync(
                "api/bugs/" + bugsFromService[1].Id + "/comments").Result;
            Assert.AreEqual(HttpStatusCode.OK, httpResponseBug1.StatusCode);

            var httpResponseBug2 = TestingEngine.HttpClient.GetAsync(
                "api/bugs/" + bugsFromService[0].Id + "/comments").Result;
            Assert.AreEqual(HttpStatusCode.OK, httpResponseBug2.StatusCode);


            var readBug1Comments = httpResponseBug1.Content.ReadAsAsync<List<CommentModel>>().Result[0];
            var readBug2Comments = httpResponseBug2.Content.ReadAsAsync<List<CommentModel>>().Result[0];

            Assert.AreEqual(readBug1Comments.Text, commentToBug1.Text);
            Assert.AreEqual(readBug2Comments.Text, commentToBug2.Text);
        }

        [TestMethod]
        public void CreateComments_AddThemToBug_AddAuthor_ListThem_ShouldListCurrentComments()
        {
            TestingEngine.CleanDatabase();

            var bug1 = new BugModel()
            {
                Title = "First Bug",
                Description = "Comment Test"
            };

            var httpPostResponseBug = TestingEngine.SubmitBugHttpPost(bug1.Title, bug1.Description);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponseBug.StatusCode);

            // Act
            var postContent = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("username", "niki"),
                new KeyValuePair<string, string>("password", "123")
            });
            var httpResponse = TestingEngine.HttpClient.PostAsync("/api/user/register", postContent).Result;
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            var userSession = httpResponse.Content.ReadAsAsync<UserSessionModel>().Result;

            var commentToBug = new CommentModel() { Text = "First Comment" };

            var bugResponse = TestingEngine.HttpClient.GetAsync("/api/bugs").Result;
            Assert.AreEqual(HttpStatusCode.OK, bugResponse.StatusCode);

            var bugsFromService = bugResponse.Content.ReadAsAsync<List<BugModel>>().Result;

            var commentPost = TestingEngine.SubmitCommentHttpPost(bugsFromService[0].Id, commentToBug.Text);

            var responsMsg = commentPost.Content.ReadAsAsync<Object>().Result;

        }
    }
}
