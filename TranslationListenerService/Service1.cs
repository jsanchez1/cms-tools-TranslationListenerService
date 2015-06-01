using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using LocalizationDatabaseAccess;
using XMLSerialization;
using System.Data.SqlClient;
using System.IO;

namespace TranslationListenerService
{
    public partial class TranslationListener : ServiceBase
    {
        private LocalizationDatabaseAccessor access;
        private XMLSerializer serializer;

        public TranslationListener()
        {
            InitializeComponent();

            access = new LocalizationDatabaseAccessor(
                connectionString: Properties.Settings.Default.ConnectionString,
                storedProcedureName: Properties.Settings.Default.StoredProcedureName,
                tableName: Properties.Settings.Default.TableName,
                primaryKey: Properties.Settings.Default.PrimaryKey);

            serializer = new XMLSerializer(access);
        }

        protected override void OnStart(string[] args)
        {
            ServiceTimer.Interval = Properties.Settings.Default.ServiceIntervalInMinutes * (1000 * 60);
            ServiceTimer.Start();
        }

        protected override void OnStop()
        {
            ServiceTimer.Stop();
        }

        private void ServiceTimer_Tick(object sender, EventArgs e)
        {
            ProcessQueue();
            ScanHandBack();
        }




        #region "Queue processing"
        private void ProcessQueue()
        {
            DataTable UnAssignedRequests = access.DataBaseToDataTable();

            if (UnAssignedRequests.Rows.Count > 0)
            {
                DataTable AssignedRequests = AssignBatches(UnAssignedRequests);
                access.UpdateDataBase(AssignedRequests);

                newFindBatches(AssignedRequests);

                foreach (BatchInfo batch in newFindBatches(AssignedRequests))
                {
                    serializer.DataBaseToXMLFile(GetXMLOutputPath(batch), batch.BatchID);
                }
            }
        }


        private string GetXMLOutputPath(BatchInfo batch)
        {
            //"<TargetLangId>_<SourceLangID>_<BatchID>_<DateStamp>.xml"
            string filename = String.Format("{0}_{1}_{2}_{3:MM-dd-yyyy}", batch.OutputLCID, batch.InputLCID, batch.BatchID, DateTime.Now);

            string path = Path.ChangeExtension(Path.Combine(Properties.Settings.Default.HandoffFolderPath, filename), "xml");

            return path;
        }


        private List<long> FindBatches(DataTable table)
        {

            var newBatchIDs = (from DataRow row in table.Rows
                               select ((long)row[Properties.Settings.Default.BatchKey])).Distinct().ToList();

            return newBatchIDs;
        }


        private struct BatchInfo
        {
            public long BatchID;
            public int InputLCID;
            public int OutputLCID;
        }

        private List<BatchInfo> newFindBatches(DataTable table)
        {

            List<BatchInfo> newBatchIDs = (from DataRow row in table.Rows
                                           select new BatchInfo
                                           {
                                               BatchID = (long)row[Properties.Settings.Default.BatchKey],
                                               InputLCID = (int)row[Properties.Settings.Default.InputLCIDKey],
                                               OutputLCID = (int)row[Properties.Settings.Default.OuputLCIDKey]
                                           }).Distinct().ToList();



            return newBatchIDs;
        }


        private DataTable AssignBatches(DataTable table)
        {
            var simplifiedList = from DataRow row in table.Rows
                                 select new
                                 {
                                     RequestID = row[access.PrimaryKey],
                                     InputLCID = row[Properties.Settings.Default.InputLCIDKey],
                                     OutputLCID = row[Properties.Settings.Default.OuputLCIDKey]
                                 };

            var groupedByLCID = from row in simplifiedList
                                group row.RequestID by new
                                {
                                    row.InputLCID,
                                    row.OutputLCID
                                } into newList
                                select new
                                {
                                    InputLCID = newList.Key.InputLCID,
                                    OutputLCID = newList.Key.OutputLCID,
                                    Requests = newList.ToList()
                                };

            foreach (var row in groupedByLCID)
            {
                long newBatchID = access.GenerateNewBatchID(Properties.Settings.Default.BatchKey);

                foreach (long request in row.Requests)
                {
                    table.Rows.Find(request)[Properties.Settings.Default.BatchKey] = newBatchID;
                }

            }

            return table;
        }

        #endregion







        private void ScanHandBack()
        {

        }


    }
}
