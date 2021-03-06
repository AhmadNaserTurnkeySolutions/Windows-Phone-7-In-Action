﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.BackgroundTransfer;

namespace Ringtones
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void SaveRingtoneButton_Click(object sender, EventArgs e)
        {
            SaveRingtoneTask task = new SaveRingtoneTask()
            {
                DisplayName = "ringtones in action",
                Source = new Uri("appdata:/mytone.wma"),
                IsShareable = true,
            };
            task.Completed += saveRingtoneTask_Completed;
            task.Show();
        }

        void saveRingtoneTask_Completed(object sender, TaskEventArgs e)
        {
            //string chooserResult = "";
            if (e.Error != null)
                chooserResult.Text = e.Error.Message;
            else if (e.TaskResult == TaskResult.Cancel)
                chooserResult.Text = "user canceled";
            else if (e.TaskResult == TaskResult.None)
                chooserResult.Text = "no result";
            else if (e.TaskResult == TaskResult.OK)
                chooserResult.Text = "ok";
            System.Diagnostics.Debug.WriteLine(chooserResult.Text);
        }
    }
}