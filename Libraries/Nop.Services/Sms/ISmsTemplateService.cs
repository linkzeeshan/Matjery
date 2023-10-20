using Nop.Core.Domain.Sms;
using System.Collections.Generic;


namespace Nop.Services
{
    public interface ISmsTemplateService
    {
        /// <summary>
        /// Delete a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        void DeleteSmsTemplate(SmsTemplate smsTemplate);

        /// <summary>
        /// Inserts a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        void InsertSmsTemplate(SmsTemplate smsTemplate);

        /// <summary>
        /// Updates a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        void UpdateSmsTemplate(SmsTemplate smsTemplate);

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateId">Sms template identifier</param>
        /// <returns>Sms template</returns>
        SmsTemplate GetSmsTemplateById(int smsTemplateId);

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateName">Sms template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Sms template</returns>
        SmsTemplate GetSmsTemplateByName(string smsTemplateName, int storeId);

        /// <summary>
        /// Gets all sms templates
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Sms template list</returns>
        IList<SmsTemplate> GetAllSmsTemplates(int storeId);
    }
}