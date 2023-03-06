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

    }
}
