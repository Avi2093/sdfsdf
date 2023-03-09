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
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using static System.Net.Mime.MediaTypeNames;

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
                    StorySelectList = new SelectList(new List<SelectListItem>()),
                    SubStatusSelectList = new List<SelectListItem>(), // Initialize the fifth dropdown with an empty list.
                };

                // Get all distinct statuses from the jirastatus table.
                var statusList = _context.jirastatus
                    .Select(j => j.Status)
                    .Distinct()
                    .ToList();
                // Create a select list for the status dropdown.
                var statusSelectList = new SelectList(statusList);
                viewModel.StatusSelectList = statusSelectList;

              

                return View(viewModel);
            }
            catch (Exception ex)
            {
               
               

                // Return an error view.
                return View("Error");
            }
        }


        //SignOut from Azure AD
        public IActionResult signin()
        {
            return RedirectToAction(nameof(JiraTicketingToolController.Index), "Index");
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    // Redirect to home page if the user is authenticated.
            //    return RedirectToAction(nameof(JiraTicketingToolController.Index), "Index");
            //}

            return RedirectToAction(nameof(JiraTicketingToolController.Index), "Index");
        }

        // This action method retrieves a list of projects for a given initiative description and populates the project dropdown.
        public IActionResult GetProjects(string description)
        {
            try
            {
                // Get all distinct jiraProjectName values for the selected description.
                var projects = _context.JiraTaskDetails
                    .Where(j => j.description == description)
                    //.Where(j => j.description.Contains(description))
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

        public IActionResult GetSubStatus(string status)
        {
            try
            {
                // Query the jirastatus table to get all SubStatus values that match the selected Status value.
                var subStatuses = _context.jirastatus
                    .Where(j => j.Status == status)
                    .Select(j => j.SubStatus)
                    .ToList();

                // Create a select list for the SubStatus dropdown.
                var subStatusSelectList = new SelectList(subStatuses);

                // Return the select list as a JSON object to be consumed by the AJAX request.
                return Json(subStatusSelectList);

            }
            catch (Exception ex)
            {
                // If an exception occurs, return a JSON object with an error message.
                return Json(new { error = "An error occurred while processing your request. Please try again later." });
            }
        }

        [HttpGet]
        public IActionResult GetAssignee()
        {
            var employees = _context.xref_productdevelopment_team
                .Where(e => e.active == 1)
                .Select(e => new { e.firstName, e.lastName, e.employeeEmail })
                .ToList();

            return Json(employees);
        }

        public IActionResult GetJiraWorkstream()
        {
            try
            {
                // Get all distinct project categories from the jira_workstreams table.
                var projectCategories = _context.jira_workstreams
                    .Select(j => j.projectCategoryName)
                    .Distinct()
                    .ToList();

                // Create a select list for the project category dropdown.
                var projectCategorySelectList = new SelectList(projectCategories);

                // Return a JSON result with the select list data.
                return Json(projectCategorySelectList);
            }
            catch (Exception ex)
            {
                // Return an error JSON result.
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult GetActivity()
        {
            var activityTypes = _context.JiraTaskDetails
                .Select(a => a.activityType)
                .Distinct()
                .ToList();

            return Json(activityTypes);
        }












        [HttpPost]
        public IActionResult CreateJiraTicket(RequestTaskDetails requestTaskDetails)
        {
            try
            {
                string username = " ";
                string password = " ";
                string URL = "geek1121.atlassian.net";

                var client = new RestClient("https://" + URL + "/rest/api/2/issue/");
                client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");

                var fields = new Fields
                {
                    description = requestTaskDetails.description,
                    summary = requestTaskDetails.summary,
                    project = new Project { key = requestTaskDetails.projectJiraKey },
                    issuetype = new IssueType { name = requestTaskDetails.taskType },
                    parent = new Parent { key = requestTaskDetails.epicIssueKey != "null" ? requestTaskDetails.epicIssueKey : requestTaskDetails.issueKey },
                    assignee = new Assignee { name = requestTaskDetails.employeeEmail },
                    customfield_10925 = requestTaskDetails.isBacklog,
                    customfield_10658 = new customfield_10658 { value = requestTaskDetails.ragStatus },
                    customfield_10843 = new customfield_10843 { value = requestTaskDetails.billingEffort },
                    customfield_11018 = new customfield_11018 { value = requestTaskDetails.workstream },
                    customfield_10841 = requestTaskDetails.actualStartDate,
                    customfield_10842 = requestTaskDetails.actualEndDate,
                    customfield_10828 = new customfield_10828 { value = requestTaskDetails.status },
                    customfield_10829 = new customfield_10829 { value = requestTaskDetails.subStatus },
                    customfield_10088 = new customfield_10088 { value = requestTaskDetails.activityType },
                    priority = new priority { name = requestTaskDetails.priority },
                    customfield_10691 = new customfield_10691 { value = requestTaskDetails.origonCountry }
                };

                var fieldsObject = new { fields };
                var jsonString = JsonConvert.SerializeObject(fieldsObject);
                request.AddParameter("application/json", jsonString, ParameterType.RequestBody);

                var response = client.Execute(request);

                return Json(response);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }




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

    
    public Assignee assignee { get; set; }


    //Custom fields IsBacklog
    public string customfield_10925 { get; set; }

    //RAG Status:
    public customfield_10658 customfield_10658 { get; set; }

    //Billing Effort:
    public customfield_10843 customfield_10843 { get; set; }

    //work stream
    public customfield_11018 customfield_11018 { get; set; }

    //startdate
    public DateTime customfield_10841 { get; set; }
    //Endate
    public DateTime customfield_10842 { get; set; }

    //status
    public customfield_10828 customfield_10828 { get; set; }
    //sub stauts
    public customfield_10829 customfield_10829 { get; set; }
    //Activity Type
    public customfield_10088 customfield_10088 { get; set; }
    //priority
    public priority priority { get; set; }
    //Originating Country/Group
    public customfield_10691 customfield_10691 { get; set; }

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

}
public class Assignee
{
    public string name { get; set; }

}

//workstream
public class customfield_11018
{
    public string value { get; set; }
}

//RagStatus
public class customfield_10658
{
    public string value { get; set; }
}
//Billing Effort
public class customfield_10843
{
    public string value { get; set; }
}
//status
public class customfield_10828
{
    public string value { get; set; }
}

//SubStatus
public class customfield_10829
{
    public string value { get; set; }
}
//Activity Type
public class customfield_10088
{
    public string value { get; set; }
}

public class priority
{
    public string name { get; set; }
}

//Originating Country/Group
public class customfield_10691
{
    public string value { get; set; }
}















