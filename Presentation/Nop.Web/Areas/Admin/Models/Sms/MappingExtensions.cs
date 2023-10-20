using Nop.Core.Domain.Sms;
using Nop.Core.Infrastructure.Mapper;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        #region Sms templates

        public static SmsTemplateModel ToModel(this SmsTemplate entity)
        {
            return entity.MapTo<SmsTemplate, SmsTemplateModel>();
        }

        public static SmsTemplate ToEntity(this SmsTemplateModel model)
        {
            return model.MapTo<SmsTemplateModel, SmsTemplate>();
        }

        public static SmsTemplate ToEntity(this SmsTemplateModel model, SmsTemplate destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Queued email

        public static QueuedSmsModel ToModel(this QueuedSms entity)
        {
            return entity.MapTo<QueuedSms, QueuedSmsModel>();
        }

        public static QueuedSms ToEntity(this QueuedSmsModel model)
        {
            return model.MapTo<QueuedSmsModel, QueuedSms>();
        }

        public static QueuedSms ToEntity(this QueuedSmsModel model, QueuedSms destination)
        {
            return model.MapTo(destination);
        }

        #endregion


    }
}