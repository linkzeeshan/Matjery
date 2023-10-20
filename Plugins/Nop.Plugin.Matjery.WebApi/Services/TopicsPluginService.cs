using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class TopicsPluginService : BasePluginService, ITopicsPluginService
    {
        public List<TopicResult> GetAllTopics()
        {
            var topics = new List<TopicResult>();
            try
            {
               
                //load by store
                IList<Topic> topicList = _topicService.GetAllTopics(_storeContext.CurrentStore.Id);
                foreach (Topic topic in topicList)
                {
                    var model = new TopicResult
                    {
                        Id = topic.Id,
                        SystemName = topic.SystemName,
                        IncludeInSitemap = topic.IncludeInSitemap,
                        IsPasswordProtected = topic.IsPasswordProtected,
                        SeName = _urlRecordService.GetSeName(topic),
                        TopicTemplateId = topic.TopicTemplateId
                    };
                    foreach (Language language in _languageService.GetAllLanguages())
                    {
                        var subModel = new
                        {
                            MetaTitle = _localizationService.GetLocalized(topic, x => x.MetaTitle, language.Id),
                            MetaKeywords = _localizationService.GetLocalized(topic, x => x.MetaKeywords, language.Id),
                            MetaDescription = _localizationService.GetLocalized(topic, x => x.MetaDescription, language.Id),
                            Title = topic.IsPasswordProtected ? "" : _localizationService.GetLocalized(topic, x => x.Title, language.Id),
                            Body = topic.IsPasswordProtected ? "" : _localizationService.GetLocalized(topic, x => x.Body, language.Id),
                        };
                        model.CustomProperties.Add(language.UniqueSeoCode, subModel);
                    }
                    topics.Add(model);
                }

                return topics;
            }
            catch (Exception ex)
            {
                throw;
            }

            return topics;
        }

        public TopicResult GetTopicDetail(string systemName)
        {
            var model = new TopicResult();
            try
            {
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.TopicModelBySystemCacheKey,
                     systemName, _workContext.WorkingLanguage.Id,
                    _storeContext.CurrentStore.Id,
                    string.Join(",", _customerService.GetCustomerRoleIds(_workContext.CurrentCustomer)));

                var cacheModel = _staticCacheManager.Get(cacheKey, () =>
                {
                    //load by store
                    var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                    if (topic == null)
                        return null;
                    if (!topic.Published)
                        return null;
                    //ACL (access control list)
                    if (!_aclService.Authorize(topic))
                        return null;
                    model = new TopicResult
                    {
                        Id = topic.Id,
                        SystemName = topic.SystemName,
                        IncludeInSitemap = topic.IncludeInSitemap,
                        IsPasswordProtected = topic.IsPasswordProtected,
                        Title = topic.IsPasswordProtected ? "" : _localizationService.GetLocalized(topic, x => x.Title),
                        Body = topic.IsPasswordProtected ? "" : _localizationService.GetLocalized(topic, x => x.Body),
                        MetaKeywords = _localizationService.GetLocalized(topic, x => x.MetaKeywords),
                        MetaDescription = _localizationService.GetLocalized(topic, x => x.MetaDescription),
                        MetaTitle = _localizationService.GetLocalized(topic, x => x.MetaTitle),
                        SeName =  _urlRecordService.GetSeName(topic),
                        TopicTemplateId = topic.TopicTemplateId
                    };
                    return model;
                });
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
            return model;
        }
    }
}
