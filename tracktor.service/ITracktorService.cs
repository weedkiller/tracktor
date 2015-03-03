using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace tracktor.service
{
    [ServiceContract]
    public interface ITracktorService
    {
        [OperationContract]
        CModel GetModel(CContext context);

        [OperationContract]
        TTaskDto UpdateTask(CContext context, TTaskDto task);

        [OperationContract]
        TProjectDto UpdateProject(CContext context, TProjectDto project);

        [OperationContract]
        TEntryDto UpdateEntry(CContext context, TEntryDto entry);

        [OperationContract]
        List<TEntryDto> GetEntries(CContext context, DateTime? startDate, DateTime endDate, int projectID, int maxEntries);

        [OperationContract]
        TracktorReportDto GetReport(CContext context, DateTime? startDate, DateTime endDate, int projectID);

        [OperationContract]
        TEntryDto StopTask(CContext context, int taskID);

        [OperationContract]
        bool StartTask(CContext context, int taskID);
    }
}
