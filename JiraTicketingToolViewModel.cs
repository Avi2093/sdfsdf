using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Ticketingtool.Models
{
    public class JiraTicketingToolViewModel
    {
        public string SelectedProject { get; set; }
        public SelectList ProjectSelectList { get; set; }

        public string SelectedReporter { get; set; }
        public SelectList ReporterSelectList { get; set; }

        public string SelectedInitiative { get; set; }
        public SelectList InitiativeSelectList { get; set; }

        public string SelectedEpic { get; set; }
        public SelectList EpicSelectList { get; set; }
        public string SelectedTask { get; set; }
        public SelectList TaskSelectList { get; set; }
        public string SelectedStory { get; set; }
        public SelectList StorySelectList { get; set; }

        public Boolean iSTaskOrStory { get; set; }
        public string SelectedStatus { get; set; }
        public string SelectedSubStatus { get; set; }

        public SelectList StatusSelectList { get; set; }
        public List<SelectListItem> SubStatusSelectList { get; set; }



        public string FullName => $"{xref_productdevelopment_team.firstName} {xref_productdevelopment_team.lastName}";
        public xref_productdevelopment_team xref_productdevelopment_team { get; set; }
    }
}
