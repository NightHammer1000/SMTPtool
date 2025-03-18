using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using SMTPtestTool;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using MailKit.Net.Smtp;
using MimeKit;

namespace SMTPtool
{
    class SMTPsender
    {
        SmtpClient client;       
        Main linkToMain;
        int mailNumber = 0;
        int maxMailCount = 1;

        List<MimeMessage> mailList = new List<MimeMessage>();

        internal void Init(int port, String server, Main linkToMain)
        {
            this.linkToMain = linkToMain;
            client = new SmtpClient();
            client.Connect(server, port, false);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
        }

        public void Run()
        {
            for (int i = 1; i <= mailList.Count; i++)
            {
                try
                {
                    DateTime sendStart = DateTime.Now;
                    client.Send(mailList[i - 1]);
                    linkToMain.Invoke((MethodInvoker)delegate()
                    {
                        TimeSpan duration = DateTime.Now - linkToMain.mailTab.sendStart;
                        linkToMain.notifyAboutSentMessage("success", duration);
                    });
                }
                catch (Exception exception)
                {
                    linkToMain.Invoke((MethodInvoker)delegate()
                    {
                        TimeSpan duration = DateTime.Now - linkToMain.mailTab.sendStart;
                        linkToMain.notifyAboutSentMessage(exception.Message, duration);

                    });                    
                }

                if (linkToMain.chbSaveInOutbox.Checked == true)
                {
                    linkToMain.remailTab.createDirectories();
                    Debug.Write("MESSAGE SAVE TO OUTBOX TRIGGERED");
                    client = new SmtpClient();
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\mailbox\\Outbox\\"; ;
                    client.Send(mailList[0]);
                    linkToMain.Invoke((MethodInvoker)delegate()
                    {
                        linkToMain.mailTab.addLogMessage("Message saved in outbox!");
                        linkToMain.remailTab.triggerTreeViewRebuild();
                    }); 

                }
            }

            
        }


        internal void setMailNumber(int a)
        {
            this.mailNumber = a;
        }

        internal void setMailCount(int maxMailCount)
        {
            this.maxMailCount = maxMailCount;
        }

        internal void addMailList(List<MimeMessage> mailList)
        {
            this.mailList = mailList;
        }

    }
}
