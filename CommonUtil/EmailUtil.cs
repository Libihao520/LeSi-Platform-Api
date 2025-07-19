using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Options;

namespace CommonUtil
{
    /// <summary>
    /// 邮件发送工具类，用于方便地发送邮件，支持设置收件人、抄送、密送以及添加附件等功能
    /// </summary>
    public class EmailUtil
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailUtil> _logger;

        /// <summary>
        /// 构造函数，初始化邮件配置选项和日志记录器
        /// </summary>
        /// <param name="emailOptions">邮件配置选项的监视器，用于获取当前的邮件配置值</param>
        /// <param name="logger">用于记录邮件发送相关操作的日志记录器</param>
        public EmailUtil(IOptionsMonitor< EmailOptions> emailOptions, ILogger<EmailUtil> logger)
        {
            _logger = logger;
            _emailOptions = emailOptions.CurrentValue;
        }

        /// <summary>
        /// 发送邮件，支持以分号分隔的收件人、抄送、密送字符串形式传入地址
        /// </summary>
        /// <param name="strMailText">邮件内容</param>
        /// <param name="strTitle">标题</param>
        /// <param name="to">收件人(多人用;隔开)</param>
        /// <param name="cc">抄送(多人用;隔开)</param>
        /// <param name="bcc">密抄(多人用;隔开)</param>
        /// <returns>发送结果信息，成功返回"发送成功！"，失败返回相应错误提示</returns>
        public string NetSendEmail(string strMailText, string strTitle, string to, string cc = "", string bcc = "")
        {
            try
            {
                List<string> toList = to.Split(';').ToList();
                List<string> ccList = cc.Split(';').ToList();
                List<string> bccList = bcc.Split(';').ToList();
                return NetSendEmail(strMailText, strTitle, toList, ccList, bccList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "将以字符串形式传入的邮件地址解析为列表时出错");
                throw new Exception($"邮件发送失败，解析邮件地址出错：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送邮件，以列表形式传入收件人、抄送、密送的邮件地址
        /// </summary>
        /// <param name="strMailText">邮件内容</param>
        /// <param name="strTitle">标题</param>
        /// <param name="to">收件人列表</param>
        /// <param name="cc">抄送列表</param>
        /// <param name="bcc">密送列表（可选，可为null）</param>
        /// <param name="path">附件路径（可选，为空字符串时不添加附件）</param>
        /// <returns>发送结果信息，成功返回"发送成功！"，失败返回相应错误提示</returns>
        public string NetSendEmail(string strMailText, string strTitle, List<string> to, List<string> cc,
            List<string> bcc = null, string path = "")
        {
            try
            {
                // 获取邮件账号配置相关信息
                var (emailOptionsEmailAndKey, randomNumber) = GetEmailAccountInfo();
                // 创建邮件消息对象
                MailMessage mailMessage =
                    CreateMailMessage(strMailText, strTitle, to, cc, bcc, path, emailOptionsEmailAndKey);
                // 创建并配置SMTP客户端对象
                SmtpClient smtpClient = ConfigureSmtpClient(emailOptionsEmailAndKey, randomNumber);
                // 发送邮件
                smtpClient.Send(mailMessage);
                return "发送成功！";
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "邮件发送过程中SMTP相关错误");
                throw new Exception($"邮件发送失败，SMTP错误：{smtpEx.Message}", smtpEx);
            }
            catch (InvalidOperationException invalidOpEx)
            {
                _logger.LogError(invalidOpEx, "邮件配置相关的操作无效错误");
                throw new Exception($"邮件发送失败，配置错误：{invalidOpEx.Message}", invalidOpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件发送出现未知错误");
                throw new Exception($"邮件发送失败，未知错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取邮件账号配置信息，包括随机选择的邮件账号配置以及对应的随机数
        /// </summary>
        /// <returns>包含邮件账号配置和随机数的元组</returns>
        private (EmailAndKeys, int) GetEmailAccountInfo()
        {
            var emailAndKeys = _emailOptions.EmailAndKeys;
            int emailCount = emailAndKeys.Count;
            if (emailCount == 0)
            {
                throw new InvalidOperationException("未配置任何邮件账户信息。");
            }

            Random random = new Random();
            int randomNumber = random.Next(0, emailCount);
            var emailOptionsEmailAndKey = emailAndKeys[randomNumber];
            return (emailOptionsEmailAndKey, randomNumber);
        }

        /// <summary>
        /// 创建邮件消息对象，设置邮件的各项属性，如主题、正文、发件人、收件人、抄送、密送以及附件等
        /// </summary>
        /// <param name="strMailText">邮件内容</param>
        /// <param name="strTitle">标题</param>
        /// <param name="to">收件人列表</param>
        /// <param name="cc">抄送列表</param>
        /// <param name="bcc">密送列表（可选，可为null）</param>
        /// <param name="path">附件路径（可选，为空字符串时不添加附件）</param>
        /// <param name="emailOptionsEmailAndKey">邮件账号配置信息</param>
        /// <returns>配置好的邮件消息对象</returns>
        private MailMessage CreateMailMessage(string strMailText, string strTitle, List<string> to, List<string> cc,
            List<string> bcc, string path, EmailAndKeys emailOptionsEmailAndKey)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.Subject = strTitle;
            mailMessage.Body = strMailText;
            mailMessage.From = new MailAddress(emailOptionsEmailAndKey.MyEmail);

            AddRecipients(mailMessage.To, to);
            AddRecipients(mailMessage.CC, cc);
            if (bcc != null)
            {
                AddRecipients(mailMessage.Bcc, bcc);
            }

            mailMessage.Priority = MailPriority.Normal;
            mailMessage.IsBodyHtml = true;

            if (!string.IsNullOrEmpty(path))
            {
                mailMessage.Attachments.Add(new Attachment(path));
            }

            return mailMessage;
        }

        /// <summary>
        /// 将邮件地址列表添加到邮件消息对象的对应收件人集合中（如To、CC、Bcc）
        /// </summary>
        /// <param name="recipientCollection">邮件消息对象的收件人集合（如To、CC、Bcc）</param>
        /// <param name="recipients">要添加的邮件地址列表</param>
        private void AddRecipients(MailAddressCollection recipientCollection, List<string> recipients)
        {
            if (recipients == null || recipients.Count == 0)
            {
                return;
            }

            foreach (string recipient in recipients)
            {
                if (!string.IsNullOrEmpty(recipient))
                {
                    recipientCollection.Add(recipient);
                }
            }
        }

        /// <summary>
        /// 创建并配置SMTP客户端对象，设置端口、SSL加密、认证凭据以及主机等信息
        /// </summary>
        /// <param name="emailOptionsEmailAndKey">邮件账号配置信息</param>
        /// <param name="randomNumber">对应的随机数（此处未使用，但为了保持统一逻辑传入）</param>
        /// <returns>配置好的SMTP客户端对象</returns>
        private SmtpClient ConfigureSmtpClient(EmailAndKeys emailOptionsEmailAndKey, int randomNumber)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Port = 587; // 使用587端口，通常用于SMTP over TLS
            smtpClient.EnableSsl = true; // 启用SSL加密
            smtpClient.Credentials =
                new NetworkCredential(emailOptionsEmailAndKey.MyEmail, emailOptionsEmailAndKey.MyKey);
            smtpClient.Host = "smtp.qq.com";
            return smtpClient;
        }
    }
}