﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChatTcpUnicast {
    public class MessageContentPresenter : ContentControl {
        public MessageContentPresenter() {
            base.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
        }
        public DataTemplate RightTemplate { get; set; }
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate NotificationTemplate { get; set; }

        protected override void OnContentChanged(object oldContent, object newContent) {
            base.OnContentChanged(oldContent, newContent);
            
            // apply the required template
            Message message = newContent as Message;
            if (message.Side == MessageType.Left) {
                ContentTemplate = LeftTemplate;
            }
            else if (message.Side == MessageType.Right) {
                ContentTemplate = RightTemplate;
            }
            else {
                ContentTemplate = NotificationTemplate;
            }
        }       
    }
}
