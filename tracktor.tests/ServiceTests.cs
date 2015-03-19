using System;
using System.Linq;
using NUnit.Framework;
using tracktor.service;
using tracktor.model.DAL;
using System.Threading;
using System.ServiceModel.Web;

namespace tracktor.tests
{
    [TestFixture]
    public class ServiceTests
    {
        private ITracktorContext _db;
        private ITracktorService _service;
        private TContextDto _requestUTC;
        private TContextDto _requestLocal;
        private TContextDto _intruderRequest;
        private int _userID;
        private int _intruderUserID;
        private int _projectID_A;
        private int _projectID_B;
        private int _taskID_A;
        private int _taskID_B;
        private int _entryID_A;
        private int _entryID_B;

        [SetUp]
        public void Initialize()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
            TracktorStartup.AppInitialize();
            _db = new TracktorContext();
            _service = new TracktorService(_db);
            _userID = _db.TUsers.Single(u => u.Name == "guest").TUserID;
            _intruderUserID = _db.TUsers.Single(u => u.Name == "intruder").TUserID;
            _projectID_A = _db.TProjects.OrderBy(p => p.TProjectID).First(p => p.TUserID == _userID).TProjectID;
            _projectID_B = _db.TProjects.OrderByDescending(p => p.TProjectID).First(p => p.TUserID == _userID).TProjectID;
            _taskID_A = _db.TTasks.First(t => t.TProjectID == _projectID_A).TTaskID;
            _taskID_B = _db.TTasks.First(t => t.TProjectID == _projectID_B).TTaskID;
            _entryID_A = _db.TEntries.First(t => t.TTaskID == _taskID_A).TTaskID;
            _entryID_B = _db.TEntries.First(t => t.TTaskID == _taskID_B).TTaskID;
            _requestUTC = new TContextDto {
                TUserID = _userID,
                UTCOffset = 0
            };
            _requestLocal = new TContextDto {
                TUserID = _userID,
                UTCOffset = -720
            };
            _intruderRequest = new TContextDto {
                TUserID = _intruderUserID,
                UTCOffset = 0
            };
        }

        [TestCase]
        public void NullTests()
        {
            Assert.IsNotNull(_service.GetEntriesModel(_requestUTC, null, null, 0, 0, 999));
            Assert.IsNotNull(_service.GetEntry(_requestUTC, _entryID_A));
            Assert.IsNotNull(_service.GetReportModel(_requestUTC, null, null, 0));
            Assert.IsNotNull(_service.GetStatusModel(_requestUTC));
            Assert.IsNotNull(_service.GetSummaryModel(_requestUTC));
        }

        [TestCase]
        public void IsolationTests()
        {
            Assert.Throws(typeof(WebFaultException<string>), delegate { _service.GetEntry(_intruderRequest, _entryID_A); });
            var entry = _service.GetEntry(_requestLocal, _entryID_A);
            Assert.Throws(typeof(WebFaultException<string>), delegate { _service.StartTask(_intruderRequest, _taskID_A); });
            Assert.IsNull(_service.UpdateEntry(_intruderRequest, entry));
            var newUserId = _service.CreateUser("new");
            Assert.Greater(newUserId, 0);
            var newModel = _service.GetSummaryModel(new TContextDto { TUserID = newUserId, UTCOffset = 0 });
            var newEntries = _service.GetEntriesModel(new TContextDto { TUserID = newUserId, UTCOffset = 0 }, null, null, 0, 0, 9999);
            Assert.AreEqual(newModel.Projects.Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth)), 0);
            Assert.AreEqual(newEntries.Entries.Count, 0);
        }

        [TestCase]
        public void TaskTests()
        {
            var model = _service.GetSummaryModel(_requestLocal);
            var contribBefore_A = model.Projects.Where(p => p.TProjectID == _projectID_A).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            var contribBefore_B = model.Projects.Where(p => p.TProjectID == _projectID_B).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));

            _service.StartTask(_requestUTC, _taskID_A);
            Thread.Sleep(100);

            model = _service.GetSummaryModel(_requestLocal);
            var statusModel = _service.GetStatusModel(_requestUTC);
            var contribAfter_A = model.Projects.Where(p => p.TProjectID == _projectID_A).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            Assert.IsTrue(model.InProgress);
            Assert.IsTrue(statusModel.InProgress);
            Assert.AreEqual(statusModel.TTaskInProgress.TTaskID, _taskID_A);
            Assert.AreEqual(statusModel.LatestEntry.TTaskID, _taskID_A);
            Assert.IsTrue(contribAfter_A > contribBefore_A);
            Assert.IsTrue(model.Projects.Single(p => p.InProgress).TProjectID == _projectID_A);
            Assert.IsTrue(model.Projects.Single(p => p.InProgress).TTasks.Single(t => t.InProgress).TTaskID == _taskID_A);

            _service.SwitchTask(_requestLocal, _taskID_A, _taskID_B);
            Thread.Sleep(100);

            model = _service.GetSummaryModel(_requestLocal);
            statusModel = _service.GetStatusModel(_requestUTC);
            var contribAfter_B = model.Projects.Where(p => p.TProjectID == _projectID_B).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            Assert.IsTrue(model.InProgress);
            Assert.IsTrue(statusModel.InProgress);
            Assert.AreEqual(statusModel.TTaskInProgress.TTaskID, _taskID_B);
            Assert.AreEqual(statusModel.LatestEntry.TTaskID, _taskID_B);
            Assert.IsTrue(contribAfter_B > contribBefore_B);
            Assert.IsTrue(model.Projects.Single(p => p.InProgress).TProjectID == _projectID_B);
            Assert.IsTrue(model.Projects.Single(p => p.InProgress).TTasks.Single(t => t.InProgress).TTaskID == _taskID_B);

            _service.StopTask(_requestLocal, _taskID_B);

            model = _service.GetSummaryModel(_requestLocal);
            statusModel = _service.GetStatusModel(_requestUTC);
            var contribFinal_A = model.Projects.Where(p => p.TProjectID == _projectID_A).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            var contribFinal_B = model.Projects.Where(p => p.TProjectID == _projectID_B).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            Assert.IsFalse(model.InProgress);
            Assert.IsFalse(statusModel.InProgress);
            Assert.AreEqual(statusModel.LatestEntry.TTaskID, _taskID_B);
            Assert.IsTrue(contribFinal_A > contribAfter_A);
            Assert.IsTrue(contribFinal_B > contribBefore_B);

            var entry = _service.GetEntriesModel(_requestLocal, null, null, _projectID_A, 0, 9999).Entries.OrderByDescending(e => e.TEntryID).First();
            var entryContribBefore = entry.Contrib;
            entry.StartDate = entry.StartDate.AddHours(-1);
            _service.UpdateEntry(_requestLocal, entry);

            model = _service.GetSummaryModel(_requestLocal);
            entry = _service.GetEntriesModel(_requestLocal, null, null, _projectID_A, 0, 9999).Entries.OrderByDescending(e => e.TEntryID).First();
            var entryContribAfter = entry.Contrib;
            var contribEdited_A = model.Projects.Where(p => p.TProjectID == _projectID_A).Sum(p => p.TTasks.Sum(t => t.Contrib.ThisMonth));
            Assert.AreEqual(contribEdited_A, contribFinal_A + 3600, 0.1);
            Assert.AreEqual(entryContribAfter, entryContribBefore + 3600, 0.1);
        }

        [TestCase]
        public void TimezoneTests()
        {
            var project = _service.UpdateProject(_requestUTC, new TProjectDto {
                Name = "test project"
            });

            var task = _service.UpdateTask(_requestUTC, new TTaskDto {
                Name = "test task",
                TProjectID = project.TProjectID
            });

            _service.StartTask(_requestUTC, task.TTaskID);
            var statusModel = _service.GetStatusModel(_requestUTC);
            Assert.IsTrue(statusModel.InProgress);
            int entryID = statusModel.LatestEntry.TEntryID;
            _service.StopTask(_requestUTC, task.TTaskID);
            var entry = _service.GetEntry(_requestUTC, entryID);
            var today = DateTime.Today;
            entry.StartDate = today.AddSeconds(-2);
            entry.EndDate = today.AddSeconds(-1);
            _service.UpdateEntry(_requestUTC, entry);

            var entryUTC = _service.GetEntry(_requestUTC, entryID);
            var entryLocal = _service.GetEntry(_requestLocal, entryID);
            Assert.AreEqual(entryUTC.Contrib, 1, 0.1);
            Assert.AreEqual(entryLocal.Contrib, 1, 0.1);

            var summaryUTC = _service.GetSummaryModel(_requestUTC);
            var summaryLocal = _service.GetSummaryModel(_requestLocal);
            var utcContrib = summaryUTC.Projects.Single(p => p.TProjectID == project.TProjectID).TTasks.Single(t => t.TTaskID == task.TTaskID).Contrib;
            var localContrib = summaryLocal.Projects.Single(p => p.TProjectID == project.TProjectID).TTasks.Single(t => t.TTaskID == task.TTaskID).Contrib;
            Assert.AreEqual(utcContrib.ThisMonth, 1);
            Assert.AreEqual(localContrib.ThisMonth, 1);
            Assert.AreEqual(utcContrib.Today, 0);
            Assert.AreNotEqual(localContrib.Today, 0);

            var reportUTC = _service.GetReportModel(_requestUTC, today.AddDays(-1), today.AddDays(1), project.TProjectID);
            var reportLocal = _service.GetReportModel(_requestLocal, today.AddDays(-1), today.AddDays(1), project.TProjectID);
            Assert.AreEqual(reportUTC.DayContribs[today.AddDays(-1)], 1);
            Assert.AreEqual(reportLocal.DayContribs[today], 1);
            Assert.AreEqual(reportUTC.TaskContribs[task.TTaskID], 1);
            Assert.AreEqual(reportLocal.TaskContribs[task.TTaskID], 1);
        }
    }
}
