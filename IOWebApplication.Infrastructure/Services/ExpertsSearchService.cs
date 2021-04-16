using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Integrations.Experts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    /// <summary>
    /// Интеграция с регистъра на вещите лица
    /// </summary>
    public class ExpertsSearchService : IExpertsSearchService
    {
        private readonly IRepository repo;

        private readonly IHttpRequester requester;

        private readonly string baseUrl;

        private readonly ILogger logger;

        public ExpertsSearchService(
            IRepository _repo,
            IHttpRequester _requester,
            IConfiguration _config,
            ILogger<ExpertsSearchService> _logger)
        {
            repo = _repo;
            requester = _requester;
            baseUrl = _config.GetValue<string>("Experts:BaseUrl") ?? "http://experts.mjs.bg/experts/";
            logger = _logger;
        }

        /// <summary>
        /// Търсене на вещи лица в регистъра
        /// </summary>
        /// <param name="competenceCode">Код на специалност (по номенклатура на регистъра)</param>
        /// <param name="keyword">Име или входящ номер</param>
        /// <param name="start">Брой пропуснати вещи лица</param>
        /// <param name="length">Брой върнати вещи лица</param>
        /// <param name="region">Код на Съдебен район (по номенклатура на регистъра)</param>
        /// <returns></returns>
        public async Task<List<ExpertIntegrationModel>> SearchExperts(string competenceCode = "any", string keyword = null, int start = 0, int length = 10, string region = "any")
        {
            string url = $"{baseUrl}public/expert-witness-search/list?start={start}&length={length}&keyword={keyword}&competence={competenceCode}&pool-membership={region}";

            ExpertsInegrationResult result = await requester.GetAsync<ExpertsInegrationResult>(url);

            return GetExpertsFromResult(result);
        }

        /// <summary>
        /// Импортира данните от рефгистъра на вещите лица в нашата база
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            bool res = false;

            try
            {
                int length = 1;
                string url = $"{baseUrl}public/expert-witness-search/list?start=0&length={length}&competence=any&pool-membership=any";

                ExpertsInegrationResult result = await requester.GetAsync<ExpertsInegrationResult>(url);
                length = result?.recordsTotal ?? 0;
                url = $"{baseUrl}public/expert-witness-search/list?start=0&length={length}&competence=any&pool-membership=any";
                result = await requester.GetAsync<ExpertsInegrationResult>(url);
                SaveToDb(result);
                res = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating Experts register");
            }

            return res;
        }

        private List<ExpertIntegrationModel> GetExpertsFromResult(ExpertsInegrationResult result)
        {
            List<ExpertIntegrationModel> experts = new List<ExpertIntegrationModel>();

            foreach (var item in result.data)
            {
                ExpertIntegrationModel expert = new ExpertIntegrationModel()
                {
                    FullName = item.fullname,
                    Competences = GetCompetences(item.competence),
                    CourtRegions = GetRegions(item.poolmembership)
                };

                experts.Add(expert);
            }

            return experts;
        }

        private List<string> GetRegions(string poolmembership)
        {
            Dictionary<string, string> regionMapper = GetRegionMapper();

            string[] regions = poolmembership
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Split(",", StringSplitOptions.RemoveEmptyEntries);

            return regions
                .Select(r => regionMapper.ContainsKey(r.Trim()) ? regionMapper[r.Trim()] : "Непознат район")
                .ToList();
        }

        private List<CompetenceIntegrationModel> GetCompetences(string competence)
        {
            List<CompetenceIntegrationModel> competences = new List<CompetenceIntegrationModel>();
            DateTime today = DateTime.Today;
            Dictionary<string, string> competenceMapper = repo.All<Speciality>()
                .Where(s => s.IsActive)
                .Where(s => s.Code != null)
                .Where(s => s.DateStart <= today)
                .Where(s => s.DateEnd == null || s.DateEnd >= today)
                .GroupBy(s => s.Code)
                .ToDictionary(k => k.Key, v => v.FirstOrDefault().Label);

            if (competence != null)
            {
                string[] competenceCodes = competence
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Split(",", StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in competenceCodes)
                {
                    string code = item.Trim();

                    competences.Add(new CompetenceIntegrationModel()
                    {
                        Code = code,
                        Name = competenceMapper.ContainsKey(code) ? competenceMapper[code] : "Непозната специалност"
                    });
                }
            }

            return competences;
        }

        private void SaveToDb(ExpertsInegrationResult result)
        {
            // ToDo Когато решим къде да се записват, да се имплементира метода
            throw new NotImplementedException();
        }

        private Dictionary<string, string> GetRegionMapper()
        {
            // ToDo  Когато решим къде са да се замени с четене от базата
            return new Dictionary<string, string>()
            {
                { "1", "Съдебен район София град"},
                { "2", "Съдебен район София област" },
                { "3", "Съдебен район Благоевград" },
                { "4", "Съдебен район Бургас" },
                { "5", "Съдебен район Варна" },
                { "6", "Съдебен район Велико Търново" },
                { "7", "Съдебен район Видин" },
                { "8", "Съдебен район Враца" },
                { "9", "Съдебен район Габрово" },
                { "10", "Съдебен район Добрич" },
                { "11", "Съдебен район Кърджали" },
                { "12", "Съдебен район Кюстендил" },
                { "13", "Съдебен район Ловеч" },
                { "14", "Съдебен район Монтана" },
                { "15", "Съдебен район Пазарджик" },
                { "16", "Съдебен район Перник" },
                { "17", "Съдебен район Плевен" },
                { "18", "Съдебен район Пловдив" },
                { "19", "Съдебен район Разград" },
                { "20", "Съдебен район Русе" },
                { "21", "Съдебен район Силистра" },
                { "22", "Съдебен район Сливен" },
                { "23", "Съдебен район Смолян" },
                { "24", "Съдебен район Стара Загора" },
                { "25", "Съдебен район Търговище" },
                { "26", "Съдебен район Хасково" },
                { "27", "Съдебен район Шумен" },
                { "28", "Съдебен район Ямбол" },
                { "29", "Специализиран Наказателен Съд" },
                { "30", "Агенция по Вписванията" },
                { "31", "Върховен Административен Съд" },
                { "32", "Върховна Касационна Прокуратура" },
                { "33", "Национална Следствена Служба" },
            };
        }
    }
}
