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
        TModelDto GetModel(TContextDto context);

        [OperationContract]
        TTaskDto UpdateTask(TContextDto context, TTaskDto task);

        [OperationContract]
        TProjectDto UpdateProject(TContextDto context, TProjectDto project);

        [OperationContract]
        TEntryDto UpdateEntry(TContextDto context, TEntryDto entry);

        [OperationContract]
        List<TEntryDto> GetEntries(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int maxEntries);

        [OperationContract]
        TracktorReportDto GetReport(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID);

        [OperationContract]
        TModelDto StopTask(TContextDto context, int currentTaskID);

        [OperationContract]
        TModelDto StartTask(TContextDto context, int newTaskID);

        [OperationContract]
        TModelDto SwitchTask(TContextDto context, int currentTaskID, int newTaskId);
    }
}
