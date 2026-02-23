using Agrojob.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Применяем миграции, если их нет
            context.Database.Migrate();

            // Очищаем существующие данные (опционально - для тестов)
            // ClearExistingData(context);

            // 1. СОЗДАЕМ СПРАВОЧНИКИ
            CreateCategories(context);
            CreateLocations(context);
            CreateTags(context);

            context.SaveChanges();

            //// 2. СОЗДАЕМ ТЕСТОВЫЕ КОМПАНИИ
            //var companies = CreateCompanies(context);
            //context.SaveChanges();

            //// 3. СОЗДАЕМ ТЕСТОВЫЕ ВАКАНСИИ
            //CreateVacancies(context, companies);
            //context.SaveChanges();
        }
        
        private static void ClearExistingData(ApplicationDbContext context)
        {
            // Очищаем в правильном порядке (с учетом внешних ключей)
            context.Offers.RemoveRange(context.Offers);
            context.Requirements.RemoveRange(context.Requirements);
            context.VacancyTags.RemoveRange(context.VacancyTags);
            context.Vacancies.RemoveRange(context.Vacancies);
            context.Companies.RemoveRange(context.Companies);
            context.Tags.RemoveRange(context.Tags);
            context.Locations.RemoveRange(context.Locations);
            context.Categories.RemoveRange(context.Categories);

            context.SaveChanges();
        }

        private static void CreateCategories(ApplicationDbContext context)
        {
            if (context.Categories.Any()) return;

            var categories = new Category[]
            {
                new Category { Name = "Агроном", Description = "Специалисты в области растениеводства" },
                new Category { Name = "Ветеринар", Description = "Специалисты по здоровью животных" },
                new Category { Name = "Механизатор", Description = "Операторы сельхозтехники" },
                new Category { Name = "Технолог", Description = "Специалисты по переработке" },
                new Category { Name = "Инженер", Description = "Инженерно-технические специалисты" }
            };

            context.Categories.AddRange(categories);
        }

        private static void CreateLocations(ApplicationDbContext context)
        {
            if (context.Locations.Any()) return;

            var locations = new Location[]
            {
                new Location { Name = "Москва", Region = "Московская обл." },
                new Location { Name = "Краснодар", Region = "Краснодарский край" },
                new Location { Name = "Ростов-на-Дону", Region = "Ростовская обл." },
                new Location { Name = "Воронеж", Region = "Воронежская обл." },
                new Location { Name = "Белгород", Region = "Белгородская обл." },
                new Location { Name = "Ставрополь", Region = "Ставропольский край" },
                new Location { Name = "Казань", Region = "Татарстан" },
                new Location { Name = "Саратов", Region = "Саратовская обл." }
            };

            context.Locations.AddRange(locations);
        }

        private static void CreateTags(ApplicationDbContext context)
        {
            if (context.Tags.Any()) return;

            var tags = new Tag[]
            {
                new Tag { Name = "сезонная работа" },
                new Tag { Name = "постоянная работа" },
                new Tag { Name = "без опыта" },
                new Tag { Name = "опыт от 1 года" },
                new Tag { Name = "опыт от 3 лет" },
                new Tag { Name = "опыт от 5 лет" },
                new Tag { Name = "жилье" },
                new Tag { Name = "питание" },
                new Tag { Name = "вахта" },
                new Tag { Name = "полный день" },
                new Tag { Name = "гибкий график" },
                new Tag { Name = "удаленная работа" },
                new Tag { Name = "официальное трудоустройство" },
                new Tag { Name = "ДМС" },
                new Tag { Name = "обучение" },
                new Tag { Name = "карьерный рост" },
                new Tag { Name = "премии" },
                new Tag { Name = "спецтехника" },
                new Tag { Name = "растениеводство" },
                new Tag { Name = "животноводство" },
                new Tag { Name = "птицеводство" }
            };

            context.Tags.AddRange(tags);
        }

        private static List<Company> CreateCompanies(ApplicationDbContext context)
        {
            if (context.Companies.Any())
                return context.Companies.ToList();

            var companies = new List<Company>
            {
                new Company
                {
                    Name = "Агрохолдинг \"Зеленые поля\"",
                    Description = "Крупнейший агрохолдинг юга России. Выращивание зерновых, масличных культур.",
                    ContactPerson = "Иван Петрович",
                    ContactPhone = "+7 (999) 123-45-67",
                    ContactEmail = "hr@zeleniepolya.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "МТС \"АгроФерма\"",
                    Description = "Современный животноводческий комплекс. Разведение КРС, производство молока.",
                    ContactPerson = "Елена Викторовна",
                    ContactPhone = "+7 (473) 555-33-22",
                    ContactEmail = "hr@agrofarma.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Фермерское хозяйство \"Ягодное\"",
                    Description = "Выращивание ягодных культур (клубника, малина, смородина).",
                    ContactPerson = "Мария Александровна",
                    ContactPhone = "+7 (812) 456-78-90",
                    ContactEmail = "rabota@yagodnoe.ru",
                    IsVerified = false
                },
                new Company
                {
                    Name = "АгроПродукт",
                    Description = "Переработка сельхозпродукции. Производство круп, муки, комбикормов.",
                    ContactPerson = "Дмитрий Сергеевич",
                    ContactPhone = "+7 (4722) 33-44-55",
                    ContactEmail = "job@agroproduct.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Агрохолдинг \"Нива\"",
                    Description = "Растениеводство, животноводство, переработка. Крупнейший работодатель региона.",
                    ContactPerson = "Николай Федорович",
                    ContactPhone = "+7 (863) 555-66-77",
                    ContactEmail = "hr@niva.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Птицефабрика \"Дружба\"",
                    Description = "Производство куриного яйца и мяса птицы. Современное оборудование.",
                    ContactPerson = "Анна Ивановна",
                    ContactPhone = "+7 (813) 777-88-99",
                    ContactEmail = "hr@druzhba.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Агрохим-Центр",
                    Description = "Поставка удобрений, средств защиты растений. Консультационные услуги.",
                    ContactPerson = "Сергей Викторович",
                    ContactPhone = "+7 (8652) 44-55-66",
                    ContactEmail = "hr@agrohim.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Тепличный комплекс \"Цветы\"",
                    Description = "Выращивание цветов и овощей в защищенном грунте. Современные теплицы.",
                    ContactPerson = "Екатерина Владимировна",
                    ContactPhone = "+7 (495) 888-99-00",
                    ContactEmail = "job@cveti.ru",
                    IsVerified = false
                },
                new Company
                {
                    Name = "Элеватор \"Южный\"",
                    Description = "Хранение и перевалка зерна. Современный элеваторный комплекс.",
                    ContactPerson = "Андрей Владимирович",
                    ContactPhone = "+7 (861) 222-33-44",
                    ContactEmail = "hr@yuzhniy.ru",
                    IsVerified = true
                },
                new Company
                {
                    Name = "Племенной завод",
                    Description = "Разведение племенного скота. Селекционная работа.",
                    ContactPerson = "Татьяна Николаевна",
                    ContactPhone = "+7 (473) 111-22-33",
                    ContactEmail = "hr@plemzavod.ru",
                    IsVerified = true
                }
            };

            context.Companies.AddRange(companies);
            return companies;
        }

        private static void CreateVacancies(ApplicationDbContext context, List<Company> companies)
        {
            if (context.Vacancies.Any()) return;

            var categories = context.Categories.ToList();
            var locations = context.Locations.ToList();
            var tags = context.Tags.ToList();

            var random = new Random();
            var vacancies = new List<Vacancy>();

            // Вакансия 1: Агроном-полевод (сезонный)
            var vacancy1 = new Vacancy
            {
                Title = "Агроном-полевод (сезонный)",
                Description = "Крупный агрохолдинг приглашает агронома-полевода на сезонные работы. Участие в посевной кампании, контроль роста растений, организация уборочных работ. Работа в дружном коллективе, современная техника.",
                Salary = "150 000 - 180 000 ₽",
                ViewsCount = 245,
                PostedDate = DateTime.UtcNow.AddDays(-2),
                IsSeasonal = true,
                IsActive = true,
                CompanyId = companies[0].Id,
                LocationId = locations.First(l => l.Name == "Краснодар").Id,
                CategoryId = categories.First(c => c.Name == "Агроном").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Высшее или среднее специальное образование" },
                    new Requirement { Text = "Опыт работы от 2 лет" },
                    new Requirement { Text = "Знание современных технологий возделывания культур" },
                    new Requirement { Text = "Наличие прав категории B" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Официальное трудоустройство" },
                    new Offer { Text = "Предоставляется жилье" },
                    new Offer { Text = "Питание за счет компании" },
                    new Offer { Text = "Своевременная оплата труда" },
                    new Offer { Text = "СезвЂ“нные бонусы по итогам уборки" }
                }
            };
            vacancy1.VacancyTags = GetTagsForVacancy(tags, new[] { "сезонная работа", "растениеводство", "жилье", "питание", "опыт от 3 лет" });
            vacancies.Add(vacancy1);

            // Вакансия 2: Главный ветеринарный врач
            var vacancy2 = new Vacancy
            {
                Title = "Главный ветеринарный врач",
                Description = "Требуется главный ветеринарный врач на крупный животноводческий комплекс. Руководство ветслужбой, планирование лечебно-профилактических мероприятий, контроль эпизоотической обстановки.",
                Salary = "150 000 - 200 000 ₽",
                ViewsCount = 189,
                PostedDate = DateTime.UtcNow.AddDays(-5),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[1].Id,
                LocationId = locations.First(l => l.Name == "Воронеж").Id,
                CategoryId = categories.First(c => c.Name == "Ветеринар").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Высшее ветеринарное образование" },
                    new Requirement { Text = "Опыт работы от 5 лет" },
                    new Requirement { Text = "Опыт руководства коллективом" },
                    new Requirement { Text = "Знание современных методов лечения" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Стабильная заработная плата" },
                    new Offer { Text = "Служебный автомобиль" },
                    new Offer { Text = "ДМС" },
                    new Offer { Text = "Возможность профессионального роста" }
                }
            };
            vacancy2.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "животноводство", "опыт от 5 лет", "ДМС", "полный день" });
            vacancies.Add(vacancy2);

            // Вакансия 3: Сборщик урожая (сезонный)
            var vacancy3 = new Vacancy
            {
                Title = "Сборщик урожая (сезонный)",
                Description = "На сезон сбора ягод требуются сборщики. Работа на свежем воздухе, дружный коллектив. Обучение на месте. Возможна сдельная оплата.",
                Salary = "60 000 - 90 000 ₽",
                ViewsCount = 567,
                PostedDate = DateTime.UtcNow.AddHours(-5),
                IsSeasonal = true,
                IsActive = true,
                CompanyId = companies[2].Id,
                LocationId = locations.First(l => l.Name == "Саратов").Id,
                CategoryId = categories.First(c => c.Name == "Агроном").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Без опыта работы" },
                    new Requirement { Text = "Ответственность" },
                    new Requirement { Text = "Желание работать" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Сдельная оплата" },
                    new Offer { Text = "Гибкий график" },
                    new Offer { Text = "Питание" },
                    new Offer { Text = "Трансфер от места сбора" }
                }
            };
            vacancy3.VacancyTags = GetTagsForVacancy(tags, new[] { "сезонная работа", "без опыта", "гибкий график", "питание" });
            vacancies.Add(vacancy3);

            // Вакансия 4: Технолог пищевого производства
            var vacancy4 = new Vacancy
            {
                Title = "Технолог пищевого производства",
                Description = "Разработка и контроль технологических процессов производства пищевых продуктов. Внедрение новых рецептур, оптимизация существующих процессов. Работа с документацией.",
                Salary = "110 000 - 140 000 ₽",
                ViewsCount = 134,
                PostedDate = DateTime.UtcNow.AddDays(-3),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[3].Id,
                LocationId = locations.First(l => l.Name == "Белгород").Id,
                CategoryId = categories.First(c => c.Name == "Технолог").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Высшее профильное образование" },
                    new Requirement { Text = "Знание систем качества" },
                    new Requirement { Text = "Опыт работы от 3 лет" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Конкурентоспособная зарплата" },
                    new Offer { Text = "Социальный пакет" },
                    new Offer { Text = "Профессиональное развитие" },
                    new Offer { Text = "Корпоративное обучение" }
                }
            };
            vacancy4.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "опыт от 3 лет", "официальное трудоустройство", "обучение" });
            vacancies.Add(vacancy4);

            // Вакансия 5: Механизатор на уборочную (сезонный)
            var vacancy5 = new Vacancy
            {
                Title = "Механизатор на уборочную (сезонный)",
                Description = "Требуются механизаторы на уборочную кампанию. Работа на современной технике (John Deere, Claas). Уборка зерновых, подработка на току.",
                Salary = "120 000 - 150 000 ₽",
                ViewsCount = 312,
                PostedDate = DateTime.UtcNow.AddHours(-12),
                IsSeasonal = true,
                IsActive = true,
                CompanyId = companies[4].Id,
                LocationId = locations.First(l => l.Name == "Ростов-на-Дону").Id,
                CategoryId = categories.First(c => c.Name == "Механизатор").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Наличие удостоверения тракториста" },
                    new Requirement { Text = "Опыт работы на комбайнах" },
                    new Requirement { Text = "Категории C, D" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Достойная оплата" },
                    new Offer { Text = "Проживание" },
                    new Offer { Text = "Питание" },
                    new Offer { Text = "Сезонная работа с возможностью продолжения" }
                }
            };
            vacancy5.VacancyTags = GetTagsForVacancy(tags, new[] { "сезонная работа", "спецтехника", "жилье", "питание", "опыт от 3 лет" });
            vacancies.Add(vacancy5);

            // Вакансия 6: Ветеринарный фельдшер
            var vacancy6 = new Vacancy
            {
                Title = "Ветеринарный фельдшер",
                Description = "Ветеринарное сопровождение птицеводческого комплекса. Проведение вакцинации, лечебных мероприятий, контроль здоровья поголовья.",
                Salary = "70 000 - 90 000 ₽",
                ViewsCount = 98,
                PostedDate = DateTime.UtcNow.AddDays(-1),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[5].Id,
                LocationId = locations.First(l => l.Name == "Казань").Id,
                CategoryId = categories.First(c => c.Name == "Ветеринар").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Среднее или высшее ветеринарное образование" },
                    new Requirement { Text = "Опыт работы в птицеводстве" },
                    new Requirement { Text = "Ответственность" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Официальное трудоустройство" },
                    new Offer { Text = "Социальные гарантии" },
                    new Offer { Text = "Возможность обучения" }
                }
            };
            vacancy6.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "птицеводство", "опыт от 1 года", "официальное трудоустройство" });
            vacancies.Add(vacancy6);

            // Вакансия 7: Агроном по защите растений
            var vacancy7 = new Vacancy
            {
                Title = "Агроном по защите растений",
                Description = "Планирование и проведение мероприятий по защите растений. Мониторинг фитосанитарной обстановки, составление схем обработок. Работа с пестицидами и агрохимикатами.",
                Salary = "100 000 - 130 000 ₽",
                ViewsCount = 156,
                PostedDate = DateTime.UtcNow.AddDays(-4),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[6].Id,
                LocationId = locations.First(l => l.Name == "Ставрополь").Id,
                CategoryId = categories.First(c => c.Name == "Агроном").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Высшее образование" },
                    new Requirement { Text = "Опыт работы от 3 лет" },
                    new Requirement { Text = "Знание пестицидов и агрохимикатов" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Стабильная зарплата" },
                    new Offer { Text = "Бонусы по результатам" },
                    new Offer { Text = "Служебный автомобиль" }
                }
            };
            vacancy7.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "растениеводство", "опыт от 3 лет", "премии", "ДМС" });
            vacancies.Add(vacancy7);

            // Вакансия 8: Рабочий в теплицу (сезонный)
            var vacancy8 = new Vacancy
            {
                Title = "Рабочий в теплицу (сезонный)",
                Description = "Работа в современных теплицах. Уход за растениями, сбор продукции. Комфортные условия труда, дружный коллектив.",
                Salary = "50 000 - 70 000 ₽",
                ViewsCount = 423,
                PostedDate = DateTime.UtcNow.AddHours(-2),
                IsSeasonal = true,
                IsActive = true,
                CompanyId = companies[7].Id,
                LocationId = locations.First(l => l.Name == "Москва").Id,
                CategoryId = categories.First(c => c.Name == "Агроном").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Без опыта" },
                    new Requirement { Text = "Аккуратность" },
                    new Requirement { Text = "Желание работать" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "График 5/2" },
                    new Offer { Text = "Трансфер" },
                    new Offer { Text = "Питание" },
                    new Offer { Text = "Дружный коллектив" }
                }
            };
            vacancy8.VacancyTags = GetTagsForVacancy(tags, new[] { "сезонная работа", "без опыта", "питание", "полный день" });
            vacancies.Add(vacancy8);

            // Вакансия 9: Инженер КИПиА
            var vacancy9 = new Vacancy
            {
                Title = "Инженер КИПиА",
                Description = "Обслуживание и ремонт контрольно-измерительных приборов и автоматики на элеваторе. Планово-предупредительные ремонты, модернизация оборудования.",
                Salary = "130 000 - 160 000 ₽",
                ViewsCount = 87,
                PostedDate = DateTime.UtcNow.AddDays(-6),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[8].Id,
                LocationId = locations.First(l => l.Name == "Краснодар").Id,
                CategoryId = categories.First(c => c.Name == "Инженер").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Профильное образование" },
                    new Requirement { Text = "Опыт работы от 3 лет" },
                    new Requirement { Text = "Знание современных систем автоматизации" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Высокая зарплата" },
                    new Offer { Text = "Полный соцпакет" },
                    new Offer { Text = "Перспективы роста" }
                }
            };
            vacancy9.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "опыт от 3 лет", "официальное трудоустройство", "карьерный рост" });
            vacancies.Add(vacancy9);

            // Вакансия 10: Зоотехник-селекционер
            var vacancy10 = new Vacancy
            {
                Title = "Зоотехник-селекционер",
                Description = "Племенная работа, селекция, отбор животных, ведение документации. Участие в выставках и аукционах. Работа с современным ПО.",
                Salary = "90 000 - 120 000 ₽",
                ViewsCount = 112,
                PostedDate = DateTime.UtcNow.AddDays(-3),
                IsSeasonal = false,
                IsActive = true,
                CompanyId = companies[9].Id,
                LocationId = locations.First(l => l.Name == "Воронеж").Id,
                CategoryId = categories.First(c => c.Name == "Ветеринар").Id,
                Requirements = new List<Requirement>
                {
                    new Requirement { Text = "Высшее образование" },
                    new Requirement { Text = "Опыт работы в животноводстве" },
                    new Requirement { Text = "Знание селекционных программ" }
                },
                Offers = new List<Offer>
                {
                    new Offer { Text = "Стабильная работа" },
                    new Offer { Text = "Социальные гарантии" },
                    new Offer { Text = "Жилье для иногородних" }
                }
            };
            vacancy10.VacancyTags = GetTagsForVacancy(tags, new[] { "постоянная работа", "животноводство", "опыт от 3 лет", "жилье", "обучение" });
            vacancies.Add(vacancy10);

            context.Vacancies.AddRange(vacancies);
        }

        private static List<VacancyTag> GetTagsForVacancy(List<Tag> allTags, string[] tagNames)
        {
            var vacancyTags = new List<VacancyTag>();

            foreach (var tagName in tagNames)
            {
                var tag = allTags.FirstOrDefault(t => t.Name == tagName);
                if (tag != null)
                {
                    vacancyTags.Add(new VacancyTag { Tag = tag });
                }
            }

            return vacancyTags;
        }
    }
}