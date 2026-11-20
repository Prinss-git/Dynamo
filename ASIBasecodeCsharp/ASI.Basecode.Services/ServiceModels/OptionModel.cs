using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Simple id/text pair used for dropdowns
    /// </summary>
    public class OptionModel
    {
        public int Id { get; set; }
        public string Text { get; set; }

        /// <summary>Optional parts for the searchable picker: main line (e.g. the person's name)...</summary>
        public string Label { get; set; }
        /// <summary>...second line (e.g. student number and course)...</summary>
        public string Hint { get; set; }
        /// <summary>...and a small tag (e.g. the role).</summary>
        public string Tag { get; set; }
    }
}
