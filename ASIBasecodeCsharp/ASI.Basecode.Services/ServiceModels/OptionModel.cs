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
    }
}
