using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Ticketingtool.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
//using Atlassian.Jira;
using Newtonsoft.Json.Linq;
using RestSharp.Authenticators;
using RestSharp;
using Microsoft.AspNetCore.Connections.Features;

namespace Ticketingtool.Controllers
{
    // This is the controller for the Jira ticketing tool.
    public class JiraTicketingToolController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JiraTicketingToolController(ApplicationDbContext context)
        {
            // Set the context for the controller.
            _context = context;
        }

        // This action method retrieves a list of initiatives from the database and populates the dropdowns for the index view.
        public IActionResult Index()
        {
            try
            {
                // Get all distinct descriptions where issuetype = Initiative from the database.
                var initiatives = _context.JiraTaskDetails
                    .Where(j => j.issuetype == "Initiative")
                    .Select(j => j.description)
                    .Distinct()
                    .ToList();

                // Create a select list for the description dropdown.
                var descriptionSelectList = new SelectList(initiatives);

                // Create a view model for the index view.
                var viewModel = new JiraTicketingToolViewModel
                {
                    InitiativeSelectList = descriptionSelectList,
                    ProjectSelectList = new SelectList(new List<SelectListItem>()),
                    EpicSelectList = new SelectList(new List<SelectListItem>()), // Initialize the third dropdown with an empty list.
                    TaskSelectList = new SelectList(new List<SelectListItem>()), // Initialize the fourth dropdown with an empty list.
                    StorySelectList = new SelectList(new List<SelectListItem>())

                };
                //var taskSelectList = viewModel.TaskSelectList; // Temporary line to check value of TaskSelectList
                //Console.WriteLine(taskSelectList);

                // Render the index view with the view model.
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // If an exception occurs, render the error view with a message.
                var errorViewModel = new ErrorViewModel { Message = "An error occurred while processing your request. Please try again later." };
                return View("Error", errorViewModel);
            }
        }

        // This action method retrieves a list of projects for a given initiative description and populates the project dropdown.
        public IActionResult GetProjects(string description)
        {
            try
            {
                // Get all distinct jiraProjectName values for the selected description.
                var projects = _context.JiraTaskDetails
                    //.Where(j => j.description == description)
                    .Where(j => j.description.Contains(description))
                    .Select(j => new JiraTaskDetail
                    {
                        jiraProjectName = j.jiraProjectName,
                        //Text = j.jiraProjectName,
                        jiraProjectKey = j.jiraProjectKey,
                    })
                    .Distinct()
                    .ToList();

                // Create a select list for the project dropdown.
                //  var projectSelectList = new SelectList(projects, "jiraProjectName", "jiraProjectName");

                // Return the select list as JSON.
                return Json(projects);
            }
            catch (Exception ex)
            {
                // If an exception occurs, return a bad request with a message.
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        //This action method retrieves a list of epics for a given initiative description and project and populates the epic dropdown.
        public IActionResult GetEpics(string description, string project)
        {
            try
            {
                // Get the LOB value for the selected initiative description.
                var lob = _context.JiraTaskDetails
                    .Where(j => j.description == description && j.issuetype == "Initiative")
                    .Select(j => j.LOB)
                    .FirstOrDefault();

                // Get all distinct epic descriptions for the selected LOB and project where issuetype = Epic.
                var epics = _context.JiraTaskDetails
                    .Where(j => j.LOB == lob && j.jiraProjectName == project && j.issuetype == "Epic")
                    .Select(j => new JiraTaskDetail
                    {
                        EpicName = j.description,
                        issueKey = j.issueKey
                    })
                    .Distinct()
                    .ToList();

                // Create a select list for the epic dropdown.
                //var epicSelectList = new SelectList(epics, "EpicName", "EpicName");

                // Return the select list as JSON.
                return Json(epics);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }


        // This action method retrieves a list of tasks for a given epic description and populates the task dropdown.
        public IActionResult GetTasks(string description, string project)
        {
            try
            {
                // Get the parent key for the selected epic.
                var parentKey = _context.JiraTaskDetails
                    .Where(j => j.description == description && j.jiraProjectName == project)
                    .Select(j => j.issueKey)
                    .FirstOrDefault();

                // Get all distinct task descriptions for the selected parent key, project, and issuetype = Story.
                var tasks = _context.JiraTaskDetails
                    .Where(j => j.parentKey == parentKey && j.jiraProjectName == project && j.issuetype == "Task")
                    .Select(j => new JiraTaskDetail
                    {
                        TaskName = j.description,
                        issueKey = j.issueKey
                    })
                    .Distinct()
                    .ToList();

                // Create a select list for the task dropdown.
                // var taskSelectList = new SelectList(tasks, "Value", "Text");

                // Return the select list as JSON.
                return Json(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }


        public IActionResult GetStory(string description, string project)
        {
            try
            {
                // Get the parent key for the selected epic.
                var parentKey = _context.JiraTaskDetails
                    .Where(j => j.description == description && j.jiraProjectName == project)
                    .Select(j => j.issueKey)
                    .FirstOrDefault();

                // Get all distinct task descriptions for the selected parent key, project, and issuetype = Story.
                var story = _context.JiraTaskDetails
                    .Where(j => j.parentKey == parentKey && j.jiraProjectName == project && j.issuetype == "Story")
                    .Select(j => new JiraTaskDetail
                    {
                        StoryDesc = j.description,
                        issueKey = j.issueKey
                    })
                    .Distinct()
                    .ToList();

                // Create a select list for the task dropdown.
                //var storySelectList = new SelectList(story, "Value", "Text");

                // Return the select list as JSON.
                return Json(story);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost]
        public IActionResult CreateJiraTicket(string projectJiraKey, string epicIssueKey, string issueType, string issueKey)
        {
            try
            {

                string username = "abcd@gmail.com";
                string password = " ";
                string URL = "baseurl.atlassian.net";

                var client = new RestClient("https://" + URL + "/rest/api/2/issue/");
                client.Authenticator = new HttpBasicAuthenticator(username, password);
                var request = new RestRequest(Method.POST);

                var jsonString = string.Empty;
                if (!string.IsNullOrEmpty(issueType))
                {
                   var str = new Fields
                    {
                        description = "Creating of an issue using project keys and issue type names using the REST API Test123",
                        summary = "REST ye merry gentlemen Test123",
                        project = new Project { key = projectJiraKey },
                        issuetype = new IssueType { name = issueType },
                        parent = new Parent { key = epicIssueKey }
                    };
                      jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(str);
                }
                else
                {
                    string taskType = "Sub-Task";
                   var  str = new Fields
                    {
                        description = "Creating of an issue using project keys and issue type names using the REST API Test123",
                        summary = "REST ye merry gentlemen Test123",
                        project = new Project { key = projectJiraKey },
                        issuetype = new IssueType { name = taskType },
                        parent = new Parent { key = issueKey }
                    };
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(str);
                }

                

                //   var dynamicObject = JsonConvert.SerializeObject<dynamic>(str)!;
                //var json1 = new JavaScriptSerializer().Serialize(str);

                JObject json = JObject.Parse(jsonString);
                //request.AddJsonBody(json);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                // Console.WriteLine("Issue: {0} successfully created", response.Content);


                // Return the select list as JSON.
                return Json(response.Content);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        //public IActionResult SendDetailTosJiraAPI()
        //{

        //    string apiUrl = @"https://URL/rest/api/3/search?jql=project=ProjectName&maxResults=10";


        //    using (var httpClient = new HttpClient())
        //    {
        //        using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiUrl))
        //        {
        //            var base64authorization =
        //                Convert.ToBase64String(Encoding.ASCII.GetBytes("emailId:CU2NhBYhT2Di48DA9"));
        //            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

        //            var response = await httpClient.SendAsync(request);

        //            // I can Json response in this variable 
        //            var jsonString = await response.Content.ReadAsStringAsync();

        //            // How to populate the response object
        //            var jsonContent = JsonConvert.DeserializeObject<JiraResponse>(jsonString);
        //        }
        //    }
        //}
        //private string RestCall()
        //{
        //    var result = string.Empty;
        //    try
        //    {
        //        string url = "\"https://geek1121.atlassian.net\";
        //        var client = new RestClient(url + "/rest/api/2/search?jql=");
        //        var request = new RestRequest
        //        {
        //            Method = Method.GET,
        //            RequestFormat = DataFormat.Json
        //        };
        //        request.AddHeader("Authorization", "Basic " + api_token);
        //        var response = client.Execute(request);
        //        result = response.Content;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return result;
        //}
    }

}

public class Issue
{
    public string key { get; set; }
    public Fields fields { get; set; }
}

public class Fields
{
    public Project project { get; set; }
    public IssueType issuetype { get; set; }
    public Parent parent { get; set; }
    public string summary { get; set; }
    public string description { get; set; }
}

public class Project
{
    public string key { get; set; }
}
public class Parent
{
    public string key { get; set; }
}

public class IssueType
{
    public string name { get; set; }

    public string epic { get; set; }
}
