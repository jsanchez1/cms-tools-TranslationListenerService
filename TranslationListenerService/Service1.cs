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

namespace TranslationListenerService
{
    public partial class TranslationListener : ServiceBase
    {
        public TranslationListener()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServiceTimer.Interval = Properties.Settings.Default.ServiceIntervalInMinutes * (1000 * 60);
            ServiceTimer.Start();
        }

        protected override void OnStop()
        {
        }

        private void ServiceTimer_Tick(object sender, EventArgs e)
        {

        }
    }
}
