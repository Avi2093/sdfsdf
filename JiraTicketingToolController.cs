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

                // Create a list of SelectListItem objects for the SubStatusSelectList property of the view model.
               // var subStatusSelectList = new SelectList(statusList);
                //viewModel.SubStatusSelectList = subStatusSelectList.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();

                // Updated assignment

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

                string username = "officialidofmine1993@gmail.com";
                string password = "ATATT3xFfGF0Q34eDaKu3efpNsBb9eStyY6mLfPLxVlMz0m-zGY3mElJ8cAfUhaIVomGAJlX4gYdrfFFfsaSbahQ4sSToEwp8QOB6RXnBToa48BQ25udfVbUTrhlar4ysf8Km9E1Fa_s17lFUc4Vph0q4mR0_8z9BLLAgFPd4sWu181pusdDTQI=7BE027BD";
                string URL = "geek1121.atlassian.net";

                var client = new RestClient("https://" + URL + "/rest/api/2/issue/");
                client.Authenticator = new HttpBasicAuthenticator(username, password);
                var request = new RestRequest(Method.POST);

                var jsonString = string.Empty;
                //if (!string.IsNullOrEmpty(issueType))
                //{
                //    var str = new Fields
                //    {
                //        description = "Creating of an issue using project keys and issue type names using the REST API Test123",
                //        summary = "REST ye merry gentlemen Test123",
                //        project = new Project { key = projectJiraKey },
                //        issuetype = new IssueType { name = issueType },
                //        parent = new Parent { key = epicIssueKey }
                //    };
                //    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(str);
                //}
                //else
                //{
                //    string taskType = "Sub-Task";
                //    var str = new Fields
                //    {
                //        description = "Creating of an issue using project keys and issue type names using the REST API Test123",
                //        summary = "REST ye merry gentlemen Test123",
                //        project = new Project { key = projectJiraKey },
                //        issuetype = new IssueType { name = taskType },
                //        parent = new Parent { key = issueKey }
                //    };
                //    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(str);
                //}

                var str = new Fields
                {
                    description = requestTaskDetails.description,
                    summary = requestTaskDetails.summary,
                    project = new Project { key = requestTaskDetails.projectJiraKey },
                    issuetype = new IssueType { name = requestTaskDetails.taskType },
                    parent = new Parent { key = requestTaskDetails.epicIssueKey != "null" ? requestTaskDetails.epicIssueKey : requestTaskDetails.issueKey }
                };
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(str);



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

                //var email = new MimeMessage();
                //email.Sender = MailboxAddress.Parse("narayan@avitester.com");
                //email.To.Add(MailboxAddress.Parse("muzafarmmulla@gmail.com"));
                //email.Subject = "Test subject";
                //var builder = new BodyBuilder();
                //if (mailRequest.Attachments != null)
                //{
                //    byte[] fileBytes;
                //    foreach (var file in mailRequest.Attachments)
                //    {
                //        if (file.Length > 0)
                //        {
                //            using (var ms = new MemoryStream())
                //            {
                //                file.CopyTo(ms);
                //                fileBytes = ms.ToArray();
                //            }
                //            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                //        }
                //    }
                //}
                //builder.HtmlBody = "Test body";
                //email.Body = builder.ToMessageBody();
                //using var smtp = new SmtpClient();
                //smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                //smtp.Authenticate("narayan@avitester.com", "HGFG#%@&$2223");
                // smtp.SendAsync(email);
                //smtp.Disconnect(true);

                //using (MailMessage mm = new MailMessage("muzafarmmulla@gmail.com", "narayan@avitester.com"))
                //{
                //    mm.Subject ="Test Mail";
                //    mm.Body = "Ticket created";
                //    //if (model.Attachment.Length > 0)
                //    //{
                //    //    string fileName = Path.GetFileName(model.Attachment.FileName);
                //    //    mm.Attachments.Add(new Attachment(model.Attachment.OpenReadStream(), fileName));
                //    //}
                //    mm.IsBodyHtml = false;
                //    using (SmtpClient smtp = new SmtpClient())
                //    {
                //        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                //        smtp.Host = "smtp.gmail.com";
                //        smtp.EnableSsl = true;
                //        NetworkCredential NetworkCred = new NetworkCredential("muzafarmmulla@gmail.com", "mrugqsawpoaygvmf");
                //        smtp.UseDefaultCredentials = false;
                //        smtp.Credentials = NetworkCred;
                //        smtp.Port = 587;
                //        smtp.Send(mm);
                //        ViewBag.Message = "Email sent.";
                //    }
                //}

                // Return the select list as JSON.
                return Json(response.Content);
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
