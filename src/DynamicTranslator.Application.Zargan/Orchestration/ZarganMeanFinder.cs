using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abp.Dependency;

using DynamicTranslator.Application.Orchestrators;
using DynamicTranslator.Application.Orchestrators.Finders;
using DynamicTranslator.Application.Orchestrators.Organizers;
using DynamicTranslator.Application.Requests;
using DynamicTranslator.Application.Zargan.Configuration;
using DynamicTranslator.Constants;
using DynamicTranslator.Domain.Model;
using DynamicTranslator.Extensions;

using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace DynamicTranslator.Application.Zargan.Orchestration
{
    public class ZarganMeanFinder : IMeanFinder, IMustHaveTranslatorType, ITransientDependency
    {
        private readonly IMeanOrganizerFactory _meanOrganizerFactory;
        private readonly IZarganTranslatorConfiguration _zarganConfiguration;

        public ZarganMeanFinder(IZarganTranslatorConfiguration zarganConfiguration, IMeanOrganizerFactory meanOrganizerFactory)
        {
            _zarganConfiguration = zarganConfiguration;
            _meanOrganizerFactory = meanOrganizerFactory;
        }

        public async Task<TranslateResult> Find(TranslateRequest translateRequest)
        {
            if (!_zarganConfiguration.IsActive() || !_zarganConfiguration.CanSupport())
            {
                return new TranslateResult(false, new Maybe<string>());
            }

            var uri = string.Format(_zarganConfiguration.Url,
                HttpUtility.UrlEncode(translateRequest.CurrentText, Encoding.UTF8));

            var response = await new RestClient(uri) { Encoding = Encoding.UTF8 }
                .ExecuteGetTaskAsync(
                    new RestRequest(Method.GET)
                        .AddHeader("Accept-Language", "en-US,en;q=0.8,tr;q=0.6")
                        .AddHeader("Accept-Encoding", "gzip, deflate, sdch")
                        .AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36")
                        .AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"));

            var mean = new Maybe<string>();

            if (response.Ok())
            {
                var organizer = _meanOrganizerFactory.GetMeanOrganizers().First(x => x.TranslatorType == TranslatorType);
                mean = await organizer.OrganizeMean(response.Content, translateRequest.FromLanguageExtension);
            }

            return new TranslateResult(true, mean);
        }

        public TranslatorType TranslatorType => TranslatorType.Zargan;
    }
}
