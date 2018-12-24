namespace PressCenters.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Data.Models;

    public class SourcesSeeder : ISeeder
    {
        public void Seed(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var sources = new List<(string TypeName, string ShortName, string Name, string Description, string Url)>
                          {
                              ("PressCenters.Services.Sources.BgInstitutions.MvrBgSource", "МВР",
                                  "Министерство на вътрешните работи",
                                  "Министерството на вътрешните работи (МВР) е българска държавна институция с ранг на министерство, която се грижи за защитата на националната сигурност, борбата с престъпността, опазването на обществения ред и други.",
                                  "https://www.mvr.bg/default.htm"),
                              ("PressCenters.Services.Sources.BgInstitutions.NsiBgNewsSource",
                                  "НСИ", "Национален статистически институт - Новини",
                                  "Националният статистически институт на Република България (НСИ) е държавна агенция, пряко подчинена на Министерския съвет, която се занимава с набиране, обработване и предоставяне на точна информация за цялостното социално и икономическо състояние и развитие на България.",
                                  "http://www.nsi.bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.NsiBgPressSource",
                                  "НСИ", "Национален статистически институт - Прессъобщения",
                                  "Националният статистически институт на Република България (НСИ) е държавна агенция, пряко подчинена на Министерския съвет, която се занимава с набиране, обработване и предоставяне на точна информация за цялостното социално и икономическо състояние и развитие на България.",
                                  "http://www.nsi.bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.GovernmentBgSource", "МС", "Министерски съвет",
                                  "Министерският съвет (Правителството) е основен орган на изпълнителната власт в Република България.",
                                  "http://www.government.bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.FscBgSource", "КФН", "Комисия за финансов надзор",
                                  "Комисията за финансов надзор на Република България е специализиран държавен регулаторен орган за контролиране на финансовата система, извън банковия сектор, надзорът над който се осъществява от Българската народна банка.",
                                  "http://www.fsc.bg/bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.ApiBgSource", "АПИ",
                                  "Агенция \"Пътна инфраструктура\"",
                                  "Агенция „Пътна инфраструктура“ е специализирана агенция към Министерството на регионалното развитие и благоустройството, отговаряща за републиканската пътна мрежа.",
                                  "http://www.api.bg/index.php/bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.MhGovernmentBgNewsSource",
                                  "МЗ", "Министерство на здравеопазването - Новини",
                                  "Чрез Министерството на здравеопазването българската държава гарантира опазването здравето на гражданите и прилага принципи на равнопоставеност при ползване на здравни услуги.",
                                  "http://www.mh.government.bg/bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.MhGovernmentBgEpidemicSource",
                                  "МЗ", "Министерство на здравеопазването - Епидемична обстановка",
                                  "Чрез Министерството на здравеопазването българската държава гарантира опазването здравето на гражданите и прилага принципи на равнопоставеност при ползване на здравни услуги.",
                                  "http://www.mh.government.bg/bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.PrbBgSource", "Прокуратурата",
                                  "Прокуратура на Република България",
                                  "Прокуратурата на Република България (ПРБ) е структура на държавното обвинение в системата на съдебната власт на Република България. ПРБ е независима част от съдебната система, отделна от съда.",
                                  "http://www.prb.bg/bg/"),
                              ("PressCenters.Services.Sources.BgInstitutions.PresidentBgSource", "Президентството",
                                  "Президентство на Република България",
                                  "Президентът на България е държавният глава на Република България, който е сред органите на държавната власт.",
                                  "http://www.president.bg/"),
                              ("PressCenters.Services.Sources.BgStateCompanies.ToploBgSource", "Топлофикация",
                                  "Топлофикация София ЕАД",
                                  "„Топлофикация София” EАД е най-старата топлофикационна система в България. По мащабите на производство и периметъра на обслужване „Топлофикация София” EАД е най-голямото дружество в страната и на Балканския полуостров.",
                                  "http://toplo.bg/"),
                              ("PressCenters.Services.Sources.BgPoliticalParties.GerbBgSource", "ГЕРБ", "ПП ГЕРБ",
                                  "ПП ГЕРБ е дясноцентристка консервативна политическа партия в България. Тя е основана на 3 декември 2006 година по инициатива на тогавашния кмет на София Бойко Борисов на основата на създаденото по-рано през същата година гражданско сдружение с име „Граждани за европейско развитие на България“ и абревиатура „ГЕРБ“.",
                                  "http://gerb.bg/"),
                              ("PressCenters.Services.Sources.BgPoliticalParties.BspBgSource", "БСП",
                                  "Българска социалистическа партия (БСП)",
                                  "Българската социалистическа партия (БСП) е лявоцентристка социалдемократическа политическа партия в България. Член на Социалистическия интернационал от октомври 2003 г. и на Партията на европейските социалисти (ПЕС). Дейността ѝ се ръководи от Национален съвет (НС) начело с председател. Наследник на Българската комунистическа партия.",
                                  "http://bsp.bg/"),
                          };

            foreach (var source in sources)
            {
                if (!dbContext.MainNewsSources.Any(x => x.TypeName == source.TypeName))
                {
                    dbContext.Sources.Add(
                        new Source
                        {
                            TypeName = source.TypeName,
                            ShortName = source.ShortName,
                            Name = source.Name,
                            Description = source.Description,
                            Url = source.Url,
                        });
                }
            }
        }
    }
}
