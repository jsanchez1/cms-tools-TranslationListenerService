﻿namespace TranslationListenerService
{
    partial class TranslationListener
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ServiceTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // ServiceTimer
            // 
            this.ServiceTimer.Tick += new System.EventHandler(this.ServiceTimer_Tick);
            // 
            // TranslationListener
            // 
            this.ServiceName = "TranslationListener";

        }

        #endregion

        private System.Windows.Forms.Timer ServiceTimer;
    }
}
