namespace Ticketingtool.Models
{
    public class RequestTaskDetails
    {
        public string projectJiraKey { get; set; }

        public string epicIssueKey { get; set; }

        public string taskType { get; set; }

        public string issueKey { get; set; }

        public string summary { get; set; }

        public string description { get; set; }

        public string employeeEmail { get; set; }

        public string isBacklog { get; set; }
        public string ragStatus { get; set; }

        public string billingEffort { get; set; }
        public string workstream { get; set; }
        public DateTime actualStartDate { get; set; }
        public DateTime actualEndDate { get; set; }
        public DateTime prodReleaseDate { get; set; }
        public DateTime dueDate { get; set; }

        public string status { get; set; }

        public string subStatus { get; set; }
        public string activityType { get; set; }

        public string priority { get; set; }

        public string origonCountry { get; set; }
    }
}
