using Newtonsoft.Json;
using Quartz.Plugins.SendMailJob;
using Quartz.Plugins.SendMailJob.DataLayer.Manager;
using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Quartz.Plugins.SendMailJob.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.Plugins
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class SendMailJobDefinition : IJob
    {
        private static readonly Random Random = new Random();

        public async Task Execute(IJobExecutionContext context)
        {
            //Debug.WriteLine("DummyJob > " + DateTime.Now);

            var schedulerName =context.Scheduler.SchedulerName;

            var jobName = context.JobDetail.Key.Name;
            var jobGroup = context.JobDetail.Key.Group;

            var trgName = context.Trigger.Key.Name;
            var trgGroup = context.Trigger.Key.Group;

            var jobDataKeys = context.JobDetail.JobDataMap.GetKeys();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);

                var customFormDataModel = JsonConvert.DeserializeObject<List<CustomDataModel>>(customFormData);

                #region Get CustomFormDataModel Props
                var sendDataId = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Id);
                var toData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.To);
                var subjectData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Subject);
                var sqlQueryConnectionString = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlQueryConnectionString);
                var sqlqueryData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Sqlquery);
                var sqlqueryToFieldData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlqueryToField);
                var headerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Header);
                var footerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Footer);
                var detailSqlQueryConnectionString = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlQueryConnectionString);
                var detailSqlqueryData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlquery);
                var detailData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Detail);
                var ccData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Cc);
                var bodyData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Body);
                var bccData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Bcc);
                #endregion

                var conStr = "Data Source=127.0.0.1,1000;Integrated Security=True;Initial Catalog=QuartzNetJobDb;UID=sa;PWD=I@mJustT3st1ing;Integrated Security=False";

                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    var tran =connection.BeginTransaction();
                    
                    var sendData = new SendData() {
                        Active = 1,
                        JobGroup = context.JobDetail.Key.Group,
                        JobName = context.JobDetail.Key.Name,
                        ScheduleName = context.Scheduler.SchedulerName,
                        Bcc = bccData.Value,
                        Cc = ccData.Value,
                        CreatedDate = DateTimeOffset.Now,
                        To = toData.Value,
                        ToSqlQuery = sqlqueryData.Value,
                        ToSqlQueryConStr = sqlQueryConnectionString.Value,
                        TriggerGroup = context.Trigger.Key.Group,
                        TriggerName = context.Trigger.Key.Name,
                        Type = 1 //TODO:
                    };

                    var sendDataIdValue = await SendDataManager.FindSendDataId(connection, tran, sendData);
                    sendData.Id = sendDataIdValue;

                    //if (Int32.TryParse(sendDataId.Value,out int sendDataIdValue))
                    //{
                    //    sendData.Id = sendDataIdValue;
                    //}

                    //SendData daha önce kaydedilmemiş ise kaydet
                    if (sendDataIdValue<1)
                    {
                        var res = SendDataManager.InsertSendData(connection, tran, sendData);

                        if (res > 0)
                        {
                            //Kayıt sonrası job üzerindeki customdata alanını güncelle (Id ekleniyor)
                            if (context.JobDetail.JobDataMap.Remove(ConstantHelper.CustomData))
                            {
                                customFormDataModel.Add(new CustomDataModel() { Key = ConstantHelper.CustomDataProps.Id, Value = "1" });

                                customFormData = JsonConvert.SerializeObject(customFormDataModel);
                                context.JobDetail.JobDataMap.Put(ConstantHelper.CustomData, customFormData);
                            }
                        }
                        else
                        {
                            tran.Rollback();

                            throw new InvalidOperationException("SendData kaydedilemedi !");
                        }
                    }

                    #region Initialize SendDataDetail
                    var sendDataDetail = new SendDataDetail()
                    {
                        SendDataId = sendDataIdValue,
                        Body = bodyData.Value,
                        Detail = detailData.Value,
                        DetailToSqlQuery = detailSqlqueryData.Value,
                        DetailToSqlQueryConStr = detailSqlQueryConnectionString.Value,
                        Footer = footerData.Value,
                        Header = headerData.Value,
                        Subject = subjectData.Value,
                        CreatedDate = DateTimeOffset.Now,
                        Active = 1,
                        Status = 0
                    };
                    
                    await SendDataManager.GenerateSendDataDetailFrom(connection,tran,sqlQueryConnectionString.Value, toData.Value, sqlqueryData.Value, sqlqueryToFieldData.Value, sendDataDetail);


                    #endregion
                }

            }

            await Task.Delay(TimeSpan.FromSeconds(Random.Next(1, 20)));
        }
    }
}
