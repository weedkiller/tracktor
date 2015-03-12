using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using tracktor.model;

namespace tracktor.service
{
    [ServiceContract]
    public interface ITracktorService
    {
        [OperationContract]
        int CreateUser(string userName);

        [OperationContract]
        TSummaryModelDto GetSummaryModel(TContextDto context);

        [OperationContract]
        TStatusModelDto GetStatusModel(TContextDto context);

        [OperationContract]
        TEntriesModelDto GetEntriesModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int startNo, int maxEntries);

        [OperationContract]
        TReportModelDto GetReportModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID);

        [OperationContract]
        TTaskDto UpdateTask(TContextDto context, TTaskDto task);

        [OperationContract]
        TProjectDto UpdateProject(TContextDto context, TProjectDto project);

        [OperationContract]
        TEntryDto GetEntry(TContextDto context, int entryID);

        [OperationContract]
        TEntryDto UpdateEntry(TContextDto context, TEntryDto entry);

        [OperationContract]
        void StopTask(TContextDto context, int currentTaskID);

        [OperationContract]
        void StartTask(TContextDto context, int newTaskID);

        [OperationContract]
        void SwitchTask(TContextDto context, int currentTaskID, int newTaskId);
    }
}
