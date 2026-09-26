using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IReportService
    {
        List<EventReportItemModel> GetEventSummaries(CurrentUserModel currentUser, int? organizationId);
        byte[] ExportEventCsv(int eventId, CurrentUserModel currentUser, out string fileName);
        byte[] ExportSummaryCsv(CurrentUserModel currentUser, int? organizationId);
        DashboardModel GetDashboard(CurrentUserModel currentUser, string name);
    }
}
