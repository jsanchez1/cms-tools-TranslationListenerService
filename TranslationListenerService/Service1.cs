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




        #region "Queue scan, batch assignment, and XML file output"
        private void ProcessQueue()
        {          

            DataTable UnAssignedRequests = access.DataBaseToDataTable();

            if (UnAssignedRequests.Rows.Count > 0)
            {
                DataTable AssignedRequests = AssignBatches(UnAssignedRequests);
                access.UpdateDataBase(UnAssignedRequests);
            }
        }


        private DataTable AssignBatches(DataTable table)
        {
            var simplifiedList = from DataRow row in table.Rows
                                 select new
                                 {
                                     RequestID = row.Field<long>("TranslationRequestID"),
                                     InputLCID = row.Field<int>("InputLangID"),
                                     OutputLCID = row.Field<int>("TargetLangID")
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
                long newBatchID = access.GenerateNewBatchID("AssignedBatchID");

                foreach (long request in row.Requests)
                {
                    table.Rows.Find(request)["AssignedBatchID"] = newBatchID;
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
