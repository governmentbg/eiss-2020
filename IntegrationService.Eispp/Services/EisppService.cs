using com.ibm.msg.client.jms;
using com.ibm.msg.client.wmq.common;
using javax.jms;
using System;
using System.Collections.Generic;

namespace IntegrationService.Eispp
{
    public class EisppService : IEisppService
    {
        #region Settings

        // Communication properties

        private string qManager = Properties.Settings.Default.EisppQManager;

        private string hostName = Properties.Settings.Default.EisppHostName;

        private int port = Properties.Settings.Default.EisppPort;

        private string channel = Properties.Settings.Default.EisppChanel;

        private string qName = Properties.Settings.Default.EisppQName;

        private string resultQName = Properties.Settings.Default.EisppResultQName;

        private string appUser = Properties.Settings.Default.EisppUser;

        private string appPassword = Properties.Settings.Default.EisppPassword;

        private long timeout = Properties.Settings.Default.EisppQTimeout;

        #endregion

        private JmsConnectionFactory cf;

        public string[] ReceiveMessages()
        {
            List<string> messages = new List<string>();

            try
            {
                if (cf == null)
                {
                    InitConnectionFactory();
                }

                using (var context = cf.createContext())
                {
                    var queue = context.createQueue($"queue:///{resultQName}");

                    using (var consumer = context.createConsumer(queue))
                    {
                        var message = consumer.receive(timeout);

                        while (message != null)
                        {
                            var body = message?.getBody(typeof(String)) as string;
                            messages.Add(body);

                            message = consumer.receive(timeout);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.ErrorHelper.WriteToLog("EisppService.ReceiveMessages", ex);
            }
            finally
            {
                cf = null;
            }

            return messages.ToArray();
        }

        public bool SendMessage(string message)
        {
            bool result = false;

            try
            {
                if (cf == null)
                {
                    InitConnectionFactory();
                }

                using (var context = cf.createContext())
                {
                    var queue = context.createQueue($"queue:///{qName}");
                    var producer = context.createProducer();
                    producer.setPriority(9);
                    Message m = context.createTextMessage(message);
                    producer.send(queue, m);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Helper.ErrorHelper.WriteToLog("EisppService.SendMessage", ex);
            }

            return result;
        }

        private void InitConnectionFactory()
        {
            var ff = JmsFactoryFactory.getInstance(JmsConstants.WMQ_PROVIDER);
            cf = ff.createConnectionFactory();

            cf.setIntProperty(CommonConstants.WMQ_CONNECTION_MODE, CommonConstants.WMQ_CM_CLIENT);
            cf.setStringProperty(CommonConstants.WMQ_HOST_NAME, hostName);
            cf.setIntProperty(CommonConstants.WMQ_PORT, port);
            cf.setStringProperty(CommonConstants.WMQ_CHANNEL, channel);
            cf.setStringProperty(CommonConstants.WMQ_QUEUE_MANAGER, qManager);
            cf.setIntProperty(CommonConstants.WMQ_CCSID, 437);
            cf.setStringProperty(CommonConstants.USERID, appUser);
            cf.setStringProperty(CommonConstants.PASSWORD, appPassword);
            cf.setIntProperty(CommonConstants.JMS_PRIORITY, 9);
            cf.setIntProperty(CommonConstants.WMQ_PRIORITY, 9);
        }
    }
}