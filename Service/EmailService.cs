﻿using BBGCombination.Core.DAL;
using BBGCombination.Core.Entity;
using BBGCombination.Core.Model;
using BBGCombination.Domain.Data.DAL;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace BBGCombination.Domain.Service
{
    public class EmailService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        LoanCustomerDB database = new LoanCustomerDB();
        DataConnectorContext context = new DataConnectorContext();
        public System.Timers.Timer thisTimer;
        /// <summary>
        ///  Calling all Loan Types
        /// </summary>
        public EmailService()
        {
            thisTimer = new Timer(86400)
            {
                AutoReset = true
            };
            thisTimer.Elapsed += thistTimer_Tick;
            logger.Info(thisTimer);
            // send mail to the customer email address.
            var recepientMail = ConfigurationManager.AppSettings["EmailRecepient"].ToString();

           var result = SendTermLoanEmail(database.GetTermLoanTestData());
           //  var result = SendTermLoanEmail(database.GetTermLoanCustomerDetail());
            var result2 = SendLeaseFinanceLoanEmail(database.GetLeaseLoanCustomerDetail());
            var result3 = SendOverdraftLoanEmail(database.GetOverdraftLoanCustomerDetail());
            Console.WriteLine("Service timer checker");
            Console.ReadLine();

          

            // GetTermLoan();
            // GetLeaseLoan();
            // GetOverdraftLoan();
        }
        public void Start()
        {
            logger.Info("Service Start!!");

            thisTimer.Elapsed += thistTimer_Tick;
            thisTimer.AutoReset = true;
            thisTimer.Enabled = true;

            thisTimer.Start();

        }
        public void Stop()
        {
            thisTimer.Stop();
            logger.Info("Service Stopped!!");
        }
        private void thistTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                // call Email Sevice
                logger.Info("Service running!!");
                var recepientMail = ConfigurationManager.AppSettings["EmailRecepient"].ToString();

                var result = SendTermLoanEmail(database.GetTermLoanTestData());
                // var result = SendTermLoanEmail(database.GetTermLoanCustomerDetail());
                var result2 = SendLeaseFinanceLoanEmail(database.GetLeaseLoanCustomerDetail());
                var result3 = SendOverdraftLoanEmail(database.GetOverdraftLoanCustomerDetail());

                thisTimer.Stop();
                thisTimer.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        // Send mail for Term Loan
        public static string SendTermLoanEmail(List<TermLoan> emailDetail)
        {
            var CcAddress = "";
            var MailSubject = ConfigurationManager.AppSettings["MailSubject"].ToString();
            var IsBodyHtml = true;
            var Result = "";

            SmtpClient obj = new SmtpClient();
            MailMessage msg = new MailMessage();

            foreach (var thisEmailDetail in emailDetail)
            {
                try
                {
                    string DisplayName = ConfigurationManager.AppSettings["DisplayName"].ToString();
                    MailAddress FromAddress = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"], DisplayName);
                    msg.From = FromAddress;
                    logger.Info("Mail from Address: " + FromAddress);
                    string[] temp = null;

                    thisEmailDetail.CustomerEmail = ConfigurationManager.AppSettings["EmailRecepient"].ToString();

                    if (string.IsNullOrEmpty(thisEmailDetail.CustomerEmail))
                    {
                        logger.Info("The Customer Email:" + thisEmailDetail.CustomerEmail);
                        // Get Customer email to send mail To...
                    }
                    else
                    {
                        // Send mail to the Customer.
                        // msg.To.Add(thisEmailDetail.CustomerEmail);
                        msg.To.Add(ConfigurationManager.AppSettings["EmailRecepient"].ToString());
                        msg.Bcc.Add(ConfigurationManager.AppSettings["EmailBcc"]);
                    }
                    // Email Address to be in the Copy of the mail
                    if (string.IsNullOrEmpty(CcAddress))
                    {
                    }
                    else
                    {
                    }
                    msg.Subject = MailSubject;

                    dynamic emailServer = ConfigurationManager.AppSettings["EmailServer"];
                    

                    string rootPath = Directory.GetCurrentDirectory();
                    string path = rootPath + "\\Notice-of-Overdue-Loan-Repayment.html";

                   // string path = ConfigurationManager.AppSettings["ExpiredTermLoanTemplatePath"];  
                   // string path2 = ConfigurationManager.AppSettings["7daysTermLoanTemplatePath"];
                    string path2 = rootPath + "\\Term-Loan-Notification-Urgent.html";
                   
                   // string path3 = rootPath + "\\Term-Loan-Notification-Follow-Up.html";
                    string path3 = rootPath + "\\Term-Loan-Notification-Follow-Up";
                   // string path4 = ConfigurationManager.AppSettings["30daysTermLoanTemplatePath"];
                    string path4 = rootPath + "\\Term-Loan-Notification.html";
                    //string path5 = ConfigurationManager.AppSettings["OverdueTermLoanTemplatePath"];
                    string path5 = rootPath + "\\Notice-of-Overdue-Loan-Repayment.html";

                    DateTime sampleDate = DateTime.Parse(thisEmailDetail.DueDate);
                  
                    DateTime todayDate = new DateTime();
                    todayDate = DateTime.Now;
                    var newSpan = (sampleDate - todayDate).Days;
                   // var newSpan = 0;
                    logger.Info("No of days to send mail: " + newSpan);

                    //var preConc = (newSpan.TotalDays) / 30;
                    //var calCon = Math.Round(preConc, 0);

                    var stringPath = "";
                    if (newSpan == 0)
                    {
                        stringPath = path;
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog log = new ActivityLog
                            {
                                Activity = "Expired Term Loan.",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(log);
                            db.SaveChanges();
                        }
                        logger.Info("The No of days and path: " + newSpan);
                    }
                    else if (newSpan <= 7 && newSpan >= 6)
                    {
                        stringPath = path2;
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog log = new ActivityLog
                            {
                                Activity = "7 days Expired Term Loan.",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(log);
                            db.SaveChanges();
                        }

                        logger.Info("Path for 7 days:" + newSpan);
                    }
                    else if (newSpan <= 14 && newSpan >= 13)
                    {
                        stringPath = path3;
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog log = new ActivityLog()
                            {
                                Activity = "15 days Term Loan Expiration Notification!!",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(log);
                            db.SaveChanges();
                        }
                        //  Logger.Info("Path for Two months or more:" + path2);
                    }
                    else if(newSpan >= 30 && newSpan <= 31)
                    {
                        stringPath = path4;
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog log = new ActivityLog
                            {
                                Activity = "30 days Term Loan Expiration Notification!!",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(log);
                            db.SaveChanges();
                        }
                    }
                    else if (newSpan <= -1 && newSpan >= -2)
                    {
                        stringPath = path5;
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog log = new ActivityLog
                            {
                                Activity = "Overdue Term Loan Expiration Notification",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(log);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog ac = new ActivityLog
                            {
                                Activity = "Days of Term Loan Expiration not within Conditional Date!!",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(ac);
                            db.SaveChanges();
                        }
                    }
                  
                    StreamReader str = new StreamReader(stringPath);
                   
                    string MailText = str.ReadToEnd();
                    str.Close();
                    MailText = MailText.Replace("{CustomerName}", thisEmailDetail.AccountName);
                    MailText = MailText.Replace("{AccountNumber}", thisEmailDetail.AccountNumber);
                    MailText = MailText.Replace("{DueDate}", thisEmailDetail.DueDate);
                    MailText = MailText.Replace("{DueAmt}", Convert.ToDouble(thisEmailDetail.DueAmount).ToString());
                  
                    MailText = MailText.Replace("{OutstandingAmt}", Convert.ToDouble(thisEmailDetail.OutstandingAmt).ToString());
                    MailText = MailText.Replace("{PastDueObligationAmt}", Convert.ToDouble(thisEmailDetail.PastDueObligationAmt).ToString());
                

                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(MailText, null, "text/html");

                    var fileName = stringPath;
                    if (IsBodyHtml == true)
                    {
                        msg.AlternateViews.Add(plainView);
                    }

                    msg.IsBodyHtml = IsBodyHtml;
                    obj.Host = emailServer;
                    obj.Send(msg);
                    Result = "Successful";
                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        EmailNotify em = new EmailNotify()
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailFrom"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = true,
                            EmailSent = true,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Success"
                        };
                        db.EmailNotifies.Add(em);
                        db.SaveChanges();
                    }
                    logger.Info("The mail is: " + Result);
                    logger.Info("Success" + ConfigurationManager.AppSettings["EmailRecepient"].ToString());
                    //Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Result = "failed" + ex.Message;
                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        ErrorLog err = new ErrorLog
                        {
                            ErrorName = "Email failed!!",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            ErrorDate = DateTime.Now
                        };
                        db.ErrorLogs.Add(err);
                        db.SaveChanges();
                        EmailNotify emp = new EmailNotify
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = false,
                            EmailSent = false,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Failed"

                        };
                        db.EmailNotifies.Add(emp);
                        db.SaveChanges();
                    }
                    logger.Error(ex);
                }
            }
            return null;
        }
       //Send mail - Lease Finance Loan
        public string SendLeaseFinanceLoanEmail(List<CustomerDetails> emailDetail)
        {
            var CcAddress = "";
            var MailSubject = ConfigurationManager.AppSettings["MailSubject"].ToString();
            var IsBodyHtml = true;
            var Result = "";
            SmtpClient obj = new SmtpClient();
            MailMessage msg = new MailMessage();

            foreach (var thisEmailDetail in emailDetail)
            {
                try
                {
                    string DisplayName = ConfigurationManager.AppSettings["DisplayName"].ToString();
                    MailAddress FromAddress = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"], DisplayName);
                    msg.From = FromAddress;

                    string[] temp = null;
                    if (string.IsNullOrEmpty(thisEmailDetail.CustomerEmail))
                    {
                        // Get Customer email to send mail To...
                    }
                    else
                    {
                        // Send mail to the Customer.
                        // msg.To.Add(thisEmailDetail.CustomerEmail);
                        msg.To.Add(ConfigurationManager.AppSettings["EmailRecepient"].ToString());
                        msg.Bcc.Add(ConfigurationManager.AppSettings["EmailBcc"]);
                    }
                    // Email Address to be in the Copy of the mail
                    if (string.IsNullOrEmpty(CcAddress))
                    {
                    }
                    else
                    {
                    }
                    msg.Subject = MailSubject;

                    dynamic emailServer = ConfigurationManager.AppSettings["EmailServer"];
                    string rootPath = Directory.GetCurrentDirectory();

                    string path = rootPath + "\\Lease-Finance-Payment-Notification-Overdue.html";
                    //string path2 = ConfigurationManager.AppSettings["7daysLeaseFinanceLoanTemplatePath"];
                    string path2 = rootPath + "\\Lease-Finance-Payment-Notification.html";
                   // string path3 = ConfigurationManager.AppSettings["14daysLeaseFinanceLoanTemplatePath"];
                    string path3 = rootPath + "\\Lease-Finance-Payment-Notification-FollowUp.html";
                   // string path4 = ConfigurationManager.AppSettings["30daysLeaseFinanceLoanTemplatePath"];
                    string path4 = rootPath + "\\Lease-Finance-Payment-Notification-30days.html";



                    DateTime sampleDate = DateTime.Parse(thisEmailDetail.DueDate);

                    DateTime todayDate = new DateTime();
                    todayDate = DateTime.Now;
                     var newSpan = (sampleDate - todayDate).Days;
                    //var newSpan = 0;
                    logger.Info("No of days to send mail: " + newSpan);
                    var stringPath = "";
                    if (newSpan == 0)
                    {
                        stringPath = path;
                        logger.Info("The No of days equal to zero:" + path);
                        ActivityLog logPath = new ActivityLog
                        {
                            Activity = "Overdue Lease Finance.",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            LoanDaysLeft = newSpan,
                            ActivityDate = DateTime.Now
                        };
                        context.Activitylogs.Add(logPath);
                        context.SaveChanges();

                    }
                    else if (newSpan >= 7 && newSpan <= 6)
                    {
                        stringPath = path2;
                        ActivityLog logPath2 = new ActivityLog
                        {
                            Activity = "7 days Lease Loan Finance. ",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            LoanDaysLeft = newSpan,
                            ActivityDate = DateTime.Now
                        };
                        context.Activitylogs.Add(logPath2);
                        context.SaveChanges();
                        logger.Info("Path for one month:" + path);
                    }
                    else if (newSpan >= 14 && newSpan <= 15)
                    {
                        stringPath = path3;
                        ActivityLog logPath3 = new ActivityLog
                        {
                            Activity = "15 days Lease Loan Finance.",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            LoanDaysLeft = newSpan,
                            ActivityDate = DateTime.Now
                        };
                        context.Activitylogs.Add(logPath3);
                        context.SaveChanges();
                        logger.Info("Path for one month:" + path);
                    }
                    else if (newSpan <= 31 && newSpan >= 30)
                    {
                        stringPath = path4;
                        ActivityLog logPath4 = new ActivityLog
                        {
                            Activity = "30 days Lease Loan Finance.",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            LoanDaysLeft = newSpan,
                            ActivityDate = DateTime.Now
                        };
                        context.Activitylogs.Add(logPath4);
                        context.SaveChanges();
                    }
                    else
                    {
                        ErrorLog log1 = new ErrorLog
                        {
                            ErrorName = "Out of Date condition Range.",
                            ErrorDate = DateTime.Now
                        };
                        context.ErrorLogs.Add(log1);
                        context.SaveChanges();
                    }

                    StreamReader str = new StreamReader(stringPath);
                    string MailText = str.ReadToEnd();
                    str.Close();
                    MailText = MailText.Replace("{CustomerName}", thisEmailDetail.AccountName);
                    MailText = MailText.Replace("{AccountNumber}", thisEmailDetail.AccountNumber);
                    MailText = MailText.Replace("{DueDate}", thisEmailDetail.DueDate);
                    MailText = MailText.Replace("{DueAmt}", Convert.ToDouble(thisEmailDetail.DueAmt).ToString());
                    MailText = MailText.Replace("{DueInDays}", (thisEmailDetail.DueInDays).ToString());
                    MailText = MailText.Replace("{OutstandingAmt}", Convert.ToDouble(thisEmailDetail.OutstandingAmt).ToString());
                    MailText = MailText.Replace("{PastDueObligationAmt}", Convert.ToDouble(thisEmailDetail.PastDueObligationAmt).ToString());
                    MailText = MailText.Replace("{CustomerEmail}", thisEmailDetail.CustomerEmail);
                    MailText = MailText.Replace("{ExcessAmt}", thisEmailDetail.ExcessAmt);
                    MailText = MailText.Replace("{AgreeMonthlyVol}", thisEmailDetail.AgreeMonthlyVol);

                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(MailText, null, "text/html");

                    var fileName = stringPath;
                    if (IsBodyHtml == true)
                    {
                        msg.AlternateViews.Add(plainView);
                    }

                    msg.IsBodyHtml = IsBodyHtml;
                    obj.Host = emailServer;
                    obj.Send(msg);
                    Result = "Successful";
                    //Logger.Info("The mail is: " + Result);

                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        EmailNotify em = new EmailNotify()
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailFrom"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = true,
                            EmailSent = true,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Success"
                        };
                        db.EmailNotifies.Add(em);
                        db.SaveChanges();

                    }
                    logger.Info("The email sent was successful.");
                }
                catch (Exception ex)
                {
                    Result = "failed" + ex.Message;
                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        ErrorLog err = new ErrorLog
                        {
                            ErrorName = "Email failed!!",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            ErrorDate = DateTime.Now
                        };
                        db.ErrorLogs.Add(err);
                        db.SaveChanges();
                        EmailNotify emp = new EmailNotify
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = false,
                            EmailSent = false,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Failed"

                        };
                        db.EmailNotifies.Add(emp);
                        db.SaveChanges();
                    }
                }
            }
            return null;
        }
        // Send mail - Overdraft Loian
        public static string SendOverdraftLoanEmail(List<CustomerDetails> emailDetail)
        {
            var CcAddress = "";
            var MailSubject = ConfigurationManager.AppSettings["MailSubject"].ToString();
            var IsBodyHtml = true;
            var Result = "";
            SmtpClient obj = new SmtpClient();
            MailMessage msg = new MailMessage();

            foreach (var thisEmailDetail in emailDetail)
            {
                try
                {
                    string DisplayName = ConfigurationManager.AppSettings["DisplayName"].ToString();
                    MailAddress FromAddress = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"], DisplayName);
                    msg.From = FromAddress;

                    string[] temp = null;
                    if (string.IsNullOrEmpty(thisEmailDetail.CustomerEmail))
                    {
                        // Get Customer email to send mail To...
                    }
                    else
                    {
                        // Send mail to the Customer.
                        // msg.To.Add(thisEmailDetail.CustomerEmail);
                        msg.To.Add(ConfigurationManager.AppSettings["EmailRecepient"].ToString());
                        msg.Bcc.Add(ConfigurationManager.AppSettings["EmailBcc"]);
                    }
                    // Email Address to be in the Copy of the mail
                    if (string.IsNullOrEmpty(CcAddress))
                    {
                    }
                    else
                    {
                    }
                    msg.Subject = MailSubject;

                    dynamic emailServer = ConfigurationManager.AppSettings["EmailServer"];
                    string rootPath = Directory.GetCurrentDirectory();
                    //string path = ConfigurationManager.AppSettings["ExpiredOverdraftLoanTemplatePath"];
                    string path = rootPath + "\\Important-Notice-On-Your-Overdraft.html";

                    //string path2 = ConfigurationManager.AppSettings["OverdraftLimitTemplatePath"];
                    string path2 = rootPath + "\\Important-Notice-On-Your-Overdraft.html";
                    //string path3 = ConfigurationManager.AppSettings["FollowUpOverdraftLoanTemplatePath"];
                    string path3 = rootPath + "\\Important-Notice-On-Your-Overdraft-Follow-Up.html";
                    


                    DateTime sampleDate = DateTime.Parse(thisEmailDetail.DueDate);

                    DateTime todayDate = new DateTime();
                    todayDate = DateTime.Now;
                     var newSpan = (sampleDate - todayDate).Days;
                   // var newSpan = 0;
                    logger.Info("No of days to send mail: " + newSpan);

                    var stringPath = "";
                    if (newSpan == 0)
                    {
                        stringPath = path;
                        logger.Info("The No of days and path: ");
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog ac = new ActivityLog
                            {
                                Activity = "Expired Overdraft Loan Remider",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(ac);
                            db.SaveChanges();
                            logger.Info(newSpan + ": Expired Overdraft Loan Reminder!!");
                        }
                            
                    }
                    else if (newSpan <= 0 && newSpan >= -1)
                    {
                        stringPath = path2;
                        using(DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog ac = new ActivityLog
                            {
                                Activity = "Exceeded Overdraft Loan Reminder",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(ac);
                            db.SaveChanges();
                            logger.Info(newSpan + ": Exceeded Overdraft Loan Reminder!!");
                        }
                        logger.Info("Exceeded Overdraft Loan Reminder: " + newSpan);
                    }
                    else if (newSpan <= -1 && newSpan >= -10)
                    {
                        stringPath = path3;
                        using(DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog ac = new ActivityLog
                            {
                                Activity = "Follow Up Overdraft Loan Reminder",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(ac);
                            db.SaveChanges();
                        }
                        logger.Info("Follow Up Overdraft Loan Reminder: " + newSpan);
                    }
                    else
                    {
                        using (DataConnectorContext db = new DataConnectorContext())
                        {
                            ActivityLog ac = new ActivityLog
                            {
                                Activity = "Overdraft out of Date Range!!",
                                EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                                LoanDaysLeft = newSpan,
                                ActivityDate = DateTime.Now
                            };
                            db.Activitylogs.Add(ac);
                            db.SaveChanges();
                        }   
                    }

                    StreamReader str = new StreamReader(stringPath);
                    string MailText = str.ReadToEnd();
                    str.Close();
                    MailText = MailText.Replace("{CustomerName}", thisEmailDetail.AccountName);
                    MailText = MailText.Replace("{AccountNumber}", thisEmailDetail.AccountNumber);
                    MailText = MailText.Replace("{DueDate}", thisEmailDetail.DueDate);
                    MailText = MailText.Replace("{DueAmt}", Convert.ToDouble(thisEmailDetail.DueAmt).ToString());
                    MailText = MailText.Replace("{DueInDays}", (thisEmailDetail.DueInDays).ToString());
                    MailText = MailText.Replace("{OutstandingAmt}", Convert.ToDouble(thisEmailDetail.OutstandingAmt).ToString());
                    //MailText = MailText.Replace("{PastDueObligationAmt}", Convert.ToDouble(thisEmailDetail.PastDueObligationAmt).ToString());
                    MailText = MailText.Replace("{CustomerEmail}", thisEmailDetail.CustomerEmail);
                    MailText = MailText.Replace("{ExcessAmt}", thisEmailDetail.ExcessAmt);
                    MailText = MailText.Replace("{AgreeMonthlyVol}", thisEmailDetail.AgreeMonthlyVol);

                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(MailText, null, "text/html");

                    var fileName = stringPath;
                    if (IsBodyHtml == true)
                    {
                        msg.AlternateViews.Add(plainView);
                    }

                    msg.IsBodyHtml = IsBodyHtml;
                    obj.Host = emailServer;
                    obj.Send(msg);
                    Result = "Successful";
                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        EmailNotify em = new EmailNotify()
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailFrom"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = true,
                            EmailSent = true,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Success"
                        };
                        db.EmailNotifies.Add(em);
                        db.SaveChanges();

                    }
                    //Logger.Info("The mail is: " + Result);
                }
                catch (Exception ex)
                {
                    Result = "failed" + ex.Message;
                    using (DataConnectorContext db = new DataConnectorContext())
                    {
                        ErrorLog err = new ErrorLog
                        {
                            ErrorName = "Email failed!!",
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            ErrorDate = DateTime.Now
                        };
                        db.ErrorLogs.Add(err);
                        db.SaveChanges();
                        EmailNotify emp = new EmailNotify
                        {
                            EmailAddress = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateSent = DateTime.Now,
                            EmailReceived = false,
                            EmailSent = false,
                            EmailRecipient = ConfigurationManager.AppSettings["EmailRecepient"].ToString(),
                            EmailDateReceived = DateTime.Now,
                            Status = "Failed"

                        };
                        db.EmailNotifies.Add(emp);
                        db.SaveChanges();
                    }
                }
            }
            return null;
        }
        public static bool ValidateEmail(string email)
        {
            bool emailValidated = false;
            if (Regex.IsMatch(email, "^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$"))
            {
                emailValidated = true;
            }
            return emailValidated;
        }

        // Send mail Term Loan Method
        public string GetTermLoan()
        {
           // var result = SendTermLoanEmail(database.GetTermLoanCustomerDetail());
            var result = SendTermLoanEmail(database.GetTermLoanTestData());
            logger.Info("Connect email service to finacle!!");
            return result;
        }
        // Send mail Leae Finance Loan Method
        public  string GetLeaseLoan()
        {
            var result2 = SendLeaseFinanceLoanEmail(database.GetLeaseLoanCustomerDetail());
            return result2;
        }
        // Send mail Overdraft Loan Method
        public string GetOverdraftLoan()
        {
            var result3 = SendOverdraftLoanEmail(database.GetOverdraftLoanCustomerDetail());
            return result3;
        }

        //// Get information from finacle and log customers details on 103 Db.
        //public TermLoanList GetCustomersDetailsLogIntoTestServerTermLoan()
        //{
        //    CustomerDetails list = new CustomerDetails();
        //    LoanCustomerDB data = new LoanCustomerDB();
        //    var FromFinacleDetails = data.GetTermLoanCustomerDetail();
        //    foreach (var detail in FromFinacleDetails)
        //    {
        //        var query = @"insert into ";
        //    }
            
        //    return null;
        //}
    }
}
