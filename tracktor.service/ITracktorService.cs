using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using tracktor.model;

namespace tracktor.service
{
    [ServiceContract]
    public interface ITracktorService
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        int CreateUser(string userName);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TSummaryModelDto GetSummaryModel(TContextDto context);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TStatusModelDto GetStatusModel(TContextDto context);

        [OperationContract]
        [WebInvoke(BodyStyle=WebMessageBodyStyle.Wrapped)]
        TEntriesModelDto GetEntriesModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int startNo, int maxEntries);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TReportModelDto GetReportModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TTaskDto UpdateTask(TContextDto context, TTaskDto task);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TProjectDto UpdateProject(TContextDto context, TProjectDto project);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TEntryDto GetEntry(TContextDto context, int entryID);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        TEntryDto UpdateEntry(TContextDto context, TEntryDto entry);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        void StopTask(TContextDto context, int currentTaskID);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        void StartTask(TContextDto context, int newTaskID);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SwitchTask(TContextDto context, int currentTaskID, int newTaskId);
    }
}
