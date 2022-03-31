using GROW_CRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GROW_CRM.Data
{
    public class GROWSeedData
    {
        //Initialize Method
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new GROWContext(
                serviceProvider.GetRequiredService<DbContextOptions<GROWContext>>()))
            {
                //Random generator
                Random rnd = new Random();

                //5 random strings
                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                if (!context.HealthIssueTypes.Any())
                {
                    context.HealthIssueTypes.AddRange(new List<HealthIssueType> { 
                        new HealthIssueType{Type = "Illness"},
                        new HealthIssueType{Type = "Concern"}
                    });

                    //Save changes
                    context.SaveChanges();
                }

                //Look For Dietary Restrictions
                if (!context.DietaryRestrictions.Any())
                {
                    //Restrictions Array
                    string[] illnesses = new string[] { "Diabetes", "Obesity", "Cancer", "Heart Disease", "Osteoporosis" };

                    string[] concerns = new string[] { "Lactose Intolerant", "Gluten Intolerance/Sensitivity", "Digestive Disorders", "Food Allergies" };

                    //List of new Dietary Restriction Objects
                    List<DietaryRestriction> dietaryRestrictions = new List<DietaryRestriction>();

                    //Add DietaryRestrictions to dietaryRestrictions List
                    for (int i = 0; i < illnesses.Count(); i++)
                        dietaryRestrictions.Add(new DietaryRestriction { Restriction = illnesses[i], HealthIssueTypeID = 1});

                    for (int i = 0; i < concerns.Count(); i++)
                        dietaryRestrictions.Add(new DietaryRestriction { Restriction = concerns[i], HealthIssueTypeID = 2 });

                    //Add Objects to context
                    context.DietaryRestrictions.AddRange(dietaryRestrictions);

                    //Save changes
                    context.SaveChanges();
                }

                //Look for Document Types
                if (!context.DocumentTypes.Any())
                {
                    //Types Array
                    string[] types = new string[] { "CRA Notice of Assessment", "ODSP", "Ontario Works Statements", "Bank Statement",
                                                    "OW", "OAS", "CPP", "Unemployed"};

                    //List of new Document Type Objects
                    List<DocumentType> documentTypes = new List<DocumentType>();

                    //Add DocumentTypes to documentTypes List
                    for (int i = 0; i < types.Count(); i++)
                        documentTypes.Add(new DocumentType { Type = types[i] });

                    //Add List to context
                    context.DocumentTypes.AddRange(documentTypes);

                    //Save Changes
                    context.SaveChanges();
                }

                //Look for Genders
                if (!context.Genders.Any())
                {
                    //Array of gender names
                    string[] names = new string[] { "Male", "Female", "Non-Binary", "Other", "Prefer Not to Say"};

                    //List of Gender Objects
                    List<Gender> genders = new List<Gender>();

                    //Add Genders to genders List
                    for (int i = 0; i < names.Count(); i++)
                        genders.Add(new Gender { Name = names[i]});

                    //Add list to context
                    context.Genders.AddRange(genders);

                    //Save changes
                    context.SaveChanges();
                }

                //Look for Statuses
                if (!context.HouseholdStatuses.Any())
                {
                    //Array of status names
                    string[] names = new string[] { "Active", "Inactive", "On Hold" };

                    //List of Household Status Objects
                    List<HouseholdStatus> householdStatuses = new List<HouseholdStatus>();

                    //Add Household statuses to the list
                    for (int i = 0; i < names.Count(); i++)
                        householdStatuses.Add(new HouseholdStatus { Name = names[i] });

                    //Add list to the context
                    context.HouseholdStatuses.AddRange(householdStatuses);

                    //Save the changes
                    context.SaveChanges();
                        
                }

                //Look for Income Situation
                if (!context.IncomeSituations.Any())
                {
                    //array of situations
                    string[] situations = new string[] { "ODSP", "Ontario Works", "CPP-Disability", "EI", "GAINS", "Post. Sec. Student",
                                                         "Other", "Volunteer", "Employed", "WSIB"};

                    //List of IncomeSituation Objects
                    List<IncomeSituation> incomeSituations = new List<IncomeSituation>();

                    //add situations to list
                    for (int i = 0; i < situations.Count(); i++)
                        incomeSituations.Add(new IncomeSituation { Situation = situations[i]});

                    //add list to context
                    context.IncomeSituations.AddRange(incomeSituations);

                    //Save changes
                    context.SaveChanges();
                }

                //Look for Categories
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Produce"},
                        new Category { Name = "Freezer"},
                        new Category { Name = "Dairy/Eggs/Bread"},
                        new Category { Name = "Pantry"},
                        new Category { Name = "Specials"}
                    };
                    context.Categories.AddRange(categories);
                    context.SaveChanges();
                }
                
                //Looks for Items
                if (!context.Items.Any())
                {
                    var produceItems = new List<Item> {
                            new Item { Code = "148", Name = "Anise / Fennel", Price = 1.50 },
                            new Item { Code = "101", Name = "Apples", Price = 0.10 },
                            new Item { Code = "102", Name = "Avocado large*", Price = 1.00 },
                            new Item { Code = "103", Name = "Avocado small*", Price = 0.50 },
                            new Item { Code = "104", Name = "Bananas", Price = 0.10 },
                            new Item { Code = "105", Name = "Blueberries / Blackberries", Price = 1.50 },
                            new Item { Code = "106", Name = "Broccoli", Price = 2.00 },
                            new Item { Code = "147", Name = "Brussel Sprouts", Price = 1.00 },
                            new Item { Code = "127", Name = "Cabbage *", Price = 2.00 },
                            new Item { Code = "107", Name = "Cantaloupe", Price = 1.50 },
                            new Item { Code = "108", Name = "Carrots", Price = 0.05 },
                            new Item { Code = "109", Name = "Cauliflower", Price = 2.50 },
                            new Item { Code = "110", Name = "Celery", Price = 1.50 },
                            new Item { Code = "111", Name = "Clementine", Price = 0.10 },
                            new Item { Code = "112", Name = "Corn", Price = 0.25 },
                            new Item { Code = "113", Name = "Cucumber", Price = 1.00 },
                            new Item { Code = "114", Name = "Cucumber Mini", Price = 0.05 },
                            new Item { Code = "115", Name = "Eggplant", Price = 0.10 },
                            new Item { Code = "116", Name = "Garlic *", Price = 0.25 },
                            new Item { Code = "117", Name = "Grapes *", Price = 1.00 },
                            new Item { Code = "118", Name = "Green Onions", Price = 0.25 },
                            new Item { Code = "119", Name = "Kale", Price = 0.50 },
                            new Item { Code = "120", Name = "Kiwi", Price = 0.25 },
                            new Item { Code = "121", Name = "Lemon *", Price = 0.25 },
                            new Item { Code = "122", Name = "Lettuce Romaine Hearts", Price = 0.50 },
                            new Item { Code = "123", Name = "Limes", Price = 0.05 },
                            new Item { Code = "124", Name = "Mango", Price = 1.00 },
                            new Item { Code = "125", Name = "Micro Greens", Price = 0.25 },
                            new Item { Code = "126", Name = "Mushrooms *", Price = 1.50 },
                            new Item { Code = "128", Name = "Onion", Price = 0.05 },
                            new Item { Code = "129", Name = "Oranges", Price = 0.20 },
                            new Item { Code = "130", Name = "Peaches / Nectarines", Price = 0.10 },
                            new Item { Code = "131", Name = "Pear", Price = 0.10 },
                            new Item { Code = "132", Name = "Peppers", Price = 0.50 },
                            new Item { Code = "133", Name = "Peppers Hot 3 / 0.05", Price = 0.05 },
                            new Item { Code = "134", Name = "Peppers Mini", Price = 0.05 },
                            new Item { Code = "135", Name = "Plums", Price = 0.05 },
                            new Item { Code = "136", Name = "Potatoes", Price = 0.05 },
                            //new Item { Code = "136", Name = "Bag of Potatoes", Price = 1.50 },
                            new Item { Code = "137", Name = "Potatoes Baby Basket", Price = 0.50 },
                            new Item { Code = "138", Name = "Potatoes Sweet(Yam)", Price = 0.75 },
                            new Item { Code = "139", Name = "Raspberries", Price = 1.50 },
                            //new Item { Code = "147", Name = "Shallots", Price = 0.05 },
                            new Item { Code = "140", Name = "Squash", Price = 2.50 },
                            new Item { Code = "141", Name = "Strawberries", Price = 1.50 },
                            new Item { Code = "142", Name = "Swiss Chard", Price = 0.50 },
                            new Item { Code = "143", Name = "Tomato Cherry / Grape Basket", Price = 0.50 },
                            new Item { Code = "144", Name = "Tomatoes", Price = 0.10 },
                            new Item { Code = "145", Name = "Watermelon", Price = 2.50 },
                            new Item { Code = "146", Name = "Zucchini", Price = 0.50 }
                            };

                    var freezerItems = new List<Item> {
                            new Item { Code = "201", Name = "Chicken Legs(2)", Price =  1.00 },
                            new Item { Code = "202", Name = "Chicken Drumsticks 4lbs", Price = 3.00 },
                            new Item { Code = "203", Name = "Chicken Thighs 4lbs", Price = 3.00 },
                            new Item { Code = "204", Name = "Chicken Wings 2lbs", Price = 2.00 },
                            new Item { Code = "205", Name = "Ground Beef", Price =  2.75 },
                            new Item { Code = "206", Name = "Veggie Burger 2pc", Price = 2.00 },
                            new Item { Code = "207", Name = "Fish(Haddock / Basa)", Price = 1.00 }
                            };

                    var dairyItems = new List<Item> {
                            new Item { Code = "301", Name = "Almond Milk 2L",   Price = 2.00 },
                            new Item { Code = "323", Name = "Bread Commisso's", Price = 1.00 },
                            new Item { Code = "302", Name = "Bread Costco", Price = 0.50 },
                            new Item { Code = "303", Name = "Butter",   Price = 1.00 },
                            new Item { Code = "304", Name = "Cheese Large", Price = 3.00 },
                            new Item { Code = "305", Name = "Cream Cheese", Price = 2.00 },
                            new Item { Code = "306", Name = "Eggs (12)   ", Price = 2.00 },
                            //new Item { Code = "306", Name = "Eggs (12)   ", Price = 3.00 },
                            new Item { Code = "307", Name = "Goat Milk 1l", Price = 2.00 },
                            new Item { Code = "308", Name = "Hummus",   Price = 2.50 },
                            new Item { Code = "309", Name = "Hummus Mini",  Price = 0.25 },
                            new Item { Code = "310", Name = "Margerine",    Price = 1.50 },
                            new Item { Code = "311", Name = "Milk - 1L",    Price = 1.00 },
                            new Item { Code = "312", Name = "Milk - 4L",    Price = 3.00 },
                            new Item { Code = "313", Name = "Oat Milk 1l",  Price = 2.00 },
                            new Item { Code = "314", Name = "Orange Juice", Price = 2.00 },
                            new Item { Code = "315", Name = "Pizza Dough",  Price = 2.00 },
                            new Item { Code = "316", Name = "Sour Crème",   Price = 2.00 },
                            new Item { Code = "317", Name = "Soy Milk 1l",  Price = 2.00 },
                            new Item { Code = "318", Name = "Tofu", Price = 2.50 },
                            new Item { Code = "319", Name = "Yogurt 4 pack",    Price = 1.00 },
                            new Item { Code = "322", Name = "Yogurt 6 pack",    Price = 1.50 },
                            new Item { Code = "320", Name = "Yogurt Greek", Price = 3.00 },
                            new Item { Code = "321", Name = "Yogurt Tub",   Price = 2.00 },
                            //new Item { Code = "322", Name = "Sliced Cheese",    Price = 2.50 }
                            };

                    var pantryItems = new List<Item> {
                            new Item { Code = "401", Name = " Apple Sauce", Price = 1.00 },
                            new Item { Code = "402", Name = " Baking Powder", Price =   2.00 },
                            new Item { Code = "403", Name = " Bars Cereal. Protein. Cookie", Price =    0.50 },
                            new Item { Code = "404", Name = " BBQ Sauce", Price =   1.00 },
                            new Item { Code = "405", Name = " Bleach", Price =  2.00 },
                            new Item { Code = "406", Name = " Broth", Price =   1.00 },
                            new Item { Code = "407", Name = " Canned Beans. Veggies. and Fruit", Price =    0.75 },
                            new Item { Code = "408", Name = " Canola Oil", Price =  3.00 },
                            new Item { Code = "409", Name = " Cereal all other", Price =    2.00 },
                            new Item { Code = "410", Name = " Cereal Rice Krispies", Price =    3.00 },
                            new Item { Code = "445", Name = " Coconut Milk", Price =    1.00 },
                            //new Item { Code = "445", Name = " Coffee", Price =  4.00 },
                            new Item { Code = "411", Name = " Crackers ", Price =   2.00 },
                            new Item { Code = "412", Name = " Dried Legumes/Beans", Price = 1.50 },
                            new Item { Code = "413", Name = " Flour", Price =   2.00 },
                            new Item { Code = "414", Name = " Garden Cocktail", Price = 0.75 },
                            new Item { Code = "415", Name = " Granola Bars 6 pack", Price = 1.00 },
                            //new Item { Code = "446", Name = " Gummies", Price = 0.10 },
                            new Item { Code = "416", Name = " Jam", Price = 2.00 },
                            new Item { Code = "417", Name = " Kraft Dinner", Price =    1.00 },
                            new Item { Code = "418", Name = " Laundry Soap large", Price =  6.00 },
                            new Item { Code = "419", Name = " Laundry Soap small", Price =  3.00 },
                            new Item { Code = "420", Name = " Miracle Whip", Price =    3.00 },
                            new Item { Code = "421", Name = " Nuts", Price =    2.00 },
                            new Item { Code = "422", Name = " Oats", Price =    2.00 },
                            new Item { Code = "423", Name = " Olive Oil", Price =   6.00 },
                            new Item { Code = "424", Name = " Passata", Price = 0.75 },
                            new Item { Code = "425", Name = " Pasta", Price =   0.75 },
                            new Item { Code = "426", Name = " Pasta Sauce", Price = 0.75 },
                            new Item { Code = "427", Name = " Peanut Butter", Price =   2.50 },
                            new Item { Code = "428", Name = " Polenta", Price = 3.00 },
                            //new Item { Code = "445", Name = " Protein Drink", Price =   0.50 },
                            new Item { Code = "446", Name = " Raisins", Price = 4.00 },
                            new Item { Code = "429", Name = " Rice", Price =    1.50 },
                            new Item { Code = "430", Name = " Salad Dressing", Price =  1.00 },
                            //new Item { Code = "445", Name = " Salsa", Price =   1.50 },
                            new Item { Code = "431", Name = " Soap ", Price =   0.50 },
                            new Item { Code = "432", Name = " Soup Small", Price =  0.50 },
                            new Item { Code = "433", Name = " Spices", Price =  1.00 },
                            new Item { Code = "434", Name = " Sugar White and Brown", Price =   2.00 },
                            new Item { Code = "435", Name = " Tea", Price = 2.00 },
                            new Item { Code = "436", Name = " Tea Green Tea", Price =   4.50 },
                            new Item { Code = "437", Name = " Tea Orange Pekoe", Price =    3.00 },
                            new Item { Code = "438", Name = " Tea Red Rose", Price =    5.00 },
                            new Item { Code = "439", Name = " Toilet Paper", Price =    5.00 },
                            new Item { Code = "440", Name = " Tomato Paste", Price =    0.75 },
                            new Item { Code = "441", Name = " Tooth Paste / Brush / Floss", Price = 0.75 },
                            new Item { Code = "442", Name = " Tuna", Price =    1.00 },
                            new Item { Code = "443", Name = " Wild Rice Blend", Price = 0.25 },
                            new Item { Code = "444", Name = " Yeast", Price =   0.50 },
                            };


                    var specialsItems = new List<Item> {
                            new Item { Code = "501", Name = "Cat Food (wet)", Price = 0.50 },
                            new Item { Code = "502", Name = "Sweets (Cotco)", Price = 2.00 },
                            new Item { Code = "503", Name = "Drinks ", Price = 0.50 },
                            new Item { Code = "504", Name = "GROW Soup", Price = 2.50 },
                            new Item { Code = "505", Name = "Deoderant", Price = 1.00 },
                            new Item { Code = "506", Name = "Polenta", Price = 3.00 },
                            new Item { Code = "507", Name = "Orzo", Price = 0.75 },
                            new Item { Code = "508", Name = "Ramen / Rice Krispies", Price = 0.25 }
                            };

                    foreach (var category in context.Categories)
                    {
                        switch (category.Name)
                        {
                            case "Produce":
                                foreach (var item in produceItems)
                                {
                                    item.CategoryID = category.ID;
                                }

                                context.Items.AddRange(produceItems);
                                break;
                            case "Freezer":
                                foreach (var item in freezerItems)
                                {
                                    item.CategoryID = category.ID;
                                }

                                context.Items.AddRange(freezerItems);
                                break;
                            case "Dairy/Eggs/Bread":
                                foreach (var item in dairyItems)
                                {
                                    item.CategoryID = category.ID;
                                }

                                context.Items.AddRange(dairyItems);
                                break;
                            case "Pantry":
                                foreach (var item in pantryItems)
                                {
                                    item.CategoryID = category.ID;
                                }

                                context.Items.AddRange(pantryItems);
                                break;
                            case "Specials":
                                foreach (var item in specialsItems)
                                {
                                    item.CategoryID = category.ID;
                                }

                                context.Items.AddRange(specialsItems);
                                break;
                            default:
                                break;
                        }
                    }

                    context.SaveChanges();
                }

                //Look for messages
                if (!context.Messages.Any())
                {
                    //List of Messages
                    List<Message> messages = new List<Message>();

                    //Fill message list
                    for (int i = 0; i < 10; i++)
                        messages.Add(new Message { Text = baconNotes[rnd.Next(5)], Date = DateTime.Now});

                    //Add list to context
                    context.Messages.AddRange(messages);

                    //Save Changes
                    context.SaveChanges();
                }

                //Look for NotificationTypes
                if (!context.NotificationTypes.Any())
                {
                    //Add new Notification types to context
                    context.NotificationTypes.AddRange(
                        new NotificationType
                        {
                            Type = "Email"
                        },
                        new NotificationType
                        {
                            Type = "SMS"
                        }
                    );

                    //Save Changes
                    context.SaveChanges();
                }

                //Look for Payment Type
                if (!context.PaymentTypes.Any())
                {
                    //Add Payment Types to context
                    context.PaymentTypes.AddRange(
                        new PaymentType
                        {
                            Type = "Credit Card"
                        },
                        new PaymentType
                        {
                            Type = "Debit Card"
                        },
                        new PaymentType
                        {
                            Type = "Cash"
                        }
                    );

                    //Save Changes
                    context.SaveChanges();
                }

                //Look for Cities
                if (!context.Cities.Any())
                {
                    var cities = new List<City>
                    {
                        new City{Name = "Grimsby"},
                        new City{Name = "Lincoln"},
                        new City{Name = "West Lincoln"},
                        new City{Name = "Wainfleet"},
                        new City{Name = "Pelham"},
                        new City{Name = "St. Catharines"},
                        new City{Name = "Thorold"},
                        new City{Name = "Welland"},
                        new City{Name = "Port Colborne"},
                        new City{Name = "Niagara-on-the-Lake"},
                        new City{Name = "Niagara Falls"},
                        new City{Name = "Fort Erie"}
                    };


                    context.Cities.AddRange(cities);
                    context.SaveChanges();                                        
                }

                //Look for Provinces
                if (!context.Provinces.Any())
                {
                    var provinces = new List<Province>
                    {
                        new Province { Code = "ON", Name = "Ontario"},
                        //new Province { Code = "PE", Name = "Prince Edward Island"},
                        //new Province { Code = "NB", Name = "New Brunswick"},
                        //new Province { Code = "BC", Name = "British Columbia"},
                        //new Province { Code = "NL", Name = "Newfoundland and Labrador"},
                        //new Province { Code = "SK", Name = "Saskatchewan"},
                        //new Province { Code = "NS", Name = "Nova Scotia"},
                        //new Province { Code = "MB", Name = "Manitoba"},
                        //new Province { Code = "QC", Name = "Quebec"},
                        //new Province { Code = "YT", Name = "Yukon"},
                        //new Province { Code = "NU", Name = "Nunavut"},
                        //new Province { Code = "NT", Name = "Northwest Territories"},
                        //new Province { Code = "AB", Name = "Alberta"}
                    };
                    context.Provinces.AddRange(provinces);
                    context.SaveChanges();
                }

                //Look for About
                if (!context.Abouts.Any())
                {
                    //add initial GROW address
                    context.Abouts.AddRange(
                   new About
                   {
                       OrgName = "GROW",
                       StreetNumber = "4377",
                       StreetName = "Fourth Avenue",
                       AptNumber = "",
                       PostalCode = "L2E 4N1",
                       CityID = 11,
                       ProvinceID = 1,
                       PhoneNumber = "9052626812",
                       WebSite = "https://www.growcflc.com/",
                       Email = "info@growflc.com"
                   });
                    //Save changes
                    context.SaveChanges();
                }


                //Look for Households
                if (!context.Households.Any())
                {
                    //Foreign Keys
                    int[] provincesIDs = context.Provinces.Select(p => p.ID).ToArray();
                    int provinceCount = provincesIDs.Count();

                    int[] householdStatusesIDs = context.HouseholdStatuses.Select(p => p.ID).ToArray();
                    int householdStatusesCount = householdStatusesIDs.Count();

                    int[] citiesIDs = context.Cities.Select(c => c.ID).ToArray();
                    int citiesCount = citiesIDs.Count();

                    int hhsa = context.HouseholdStatuses.Where(hhs => hhs.Name == "Active").Select(hhs => hhs.ID).FirstOrDefault();
                    int hhsoh = context.HouseholdStatuses.Where(hhs => hhs.Name == "On Hold").Select(hhs => hhs.ID).FirstOrDefault();

                    //Data
                    string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "X", "W", "Y", "Z" };
                    int alhabetLen = alphabet.Count();                    

                    //Add Households to context
                    for (int i = 0; i < 100; i++)
                    {
                        string lastName = Faker.Name.Last();

                        string postalCode = $"L{Faker.RandomNumber.Next(9)}{alphabet[rnd.Next(alhabetLen)]} {Faker.RandomNumber.Next(9)}{alphabet[rnd.Next(alhabetLen)]}{Faker.RandomNumber.Next(9)}";
                        context.Add(new Household
                        {
                            Name = $"House {lastName}",
                            StreetNumber = Faker.RandomNumber.Next(200).ToString(),
                            StreetName = Faker.Address.StreetName(),
                            AptNumber = Faker.RandomNumber.Next(100).ToString(),
                            PostalCode = postalCode,
                            LICOVerified = true,
                            LastVerification = DateTime.Now,
                            CityID = citiesIDs[rnd.Next(citiesCount)],
                            ProvinceID = 1,
                            HouseholdStatusID = i % 3 == 0? hhsoh : hhsa,
                            AboutID = 1
                        }); ;
                    }
                   //Save changes
                   context.SaveChanges();
                }                




                //Look for members
                if (!context.Members.Any())
                {
                    //Foreign Keys
                    int[] householdIDs = context.Households.Select(h => h.ID).ToArray();
                    int householdCount = householdIDs.Count();                    

                    int[] genderIDs = context.Genders.Select(g => g.ID).ToArray();
                    int genderCount = genderIDs.Count();                    

                    //Name Generator
                    string[] firstNames = new string[] { "Mark", "Pedro", "Roger", "Hitome", "Lin", "Brendon", "Michelle", "Leticia", "Love", "Jennifer", "Shadwick" };
                    string[] middleNames = new string[] { "M", "L", "K", "P", "A", "T", "R", "G"};
                    string[] lastNames = new string[] { "Pereira", "Martin", "Scott", "McTavish", "Smith", "Castro", "Lee", "Zhang", "Cruise"};

                    //Start DOB
                    DateTime startDOB = Convert.ToDateTime("1992-08-22");

                    //Phone Generator
                    string[] areaCodes = new string[] { "905", "289", "365", "742"};
                    int areaCodesCount = areaCodes.Length;

                    //Data
                    string[] notes = new string[] { "Member did not yet submited their documents.",
                                                    "Member is waiting for their CRA documents.",
                                                    "Member address is a temporary one, we are waiting for their update on the situation."};

                    int notesLen = notes.Count();

                    //Loop over wach household and assign family members to it
                    for (int i = 0; i < householdCount; i++)
                    {
                        Household household = context.Households.Include(h => h.HouseholdStatus).Where(h => h.ID == (i+1)).FirstOrDefault();

                        string lastName = household.Name.Substring(6);

                        string hhStatus = household.HouseholdStatus.Name;

                        string phoneNumber = $"{areaCodes[rnd.Next(areaCodesCount)]}";

                        for(int j = 0; j < 7; j++) phoneNumber = phoneNumber + Faker.RandomNumber.Next(9).ToString();

                        for(int k = 0; k < rnd.Next(5) + 1; k++)
                        {
                            context.Members.Add(
                                new Member
                                {
                                    FirstName = firstNames[rnd.Next(firstNames.Count())],
                                    MiddleName = middleNames[rnd.Next(middleNames.Count())],
                                    LastName = lastName,
                                    DOB = startDOB.AddDays(rnd.Next(60, 6500)),
                                    PhoneNumber = phoneNumber,
                                    Email = Faker.Internet.Email(),
                                    Notes = hhStatus == "On Hold" ? notes[rnd.Next(notesLen)] : "",
                                    ConsentGiven = true,
                                    GenderID = genderIDs[rnd.Next(genderCount)],
                                    HouseholdID = household.ID
                                }    
                            );

                            context.SaveChanges();
                        }
                    }
                }

                //Members
                int[] memberIDs = context.Members.Select(m => m.ID).ToArray();
                int memberCount = memberIDs.Count();

                //Look for Dietary Restriction Members
                if (!context.DietaryRestrictionMembers.Any())
                {
                    //Foreign Keys
                    int[] drIDs = context.DietaryRestrictions.Select(dr => dr.ID).ToArray();
                    int drCount = drIDs.Count();                                     

                    foreach(int memberID in memberIDs)
                    {
                        if ((memberID % 3) == 0) continue;

                        context.DietaryRestrictionMembers.Add(
                            new DietaryRestrictionMember
                            {
                                MemberID = memberID,
                                DietaryRestrictionID = drIDs[rnd.Next(drCount)]
                            }
                        );

                        context.SaveChanges();
                    }
                }

                //Look for Member Income Situation
                if (!context.MemberIncomeSituations.Any())
                {
                    int[] incomeSituationIDs = context.IncomeSituations.Select(i => i.ID).ToArray();
                    int incomeSituationCount = incomeSituationIDs.Count();                    

                    List<MemberIncomeSituation> mis = new List<MemberIncomeSituation>();

                    foreach (var memberID in memberIDs)
                    {
                        MemberIncomeSituation s = new MemberIncomeSituation
                        {
                            MemberID = memberID,
                            IncomeSituationID = incomeSituationIDs[rnd.Next(incomeSituationCount - 1)],
                            Income = rnd.Next(10, 10000)
                        };
                        mis.Add(s);
                    }
                    context.MemberIncomeSituations.AddRange(mis);
                    context.SaveChanges();
                }

                //Look for Orders
                if (!context.Orders.Any())
                {
                    //Foreign Keys
                    int[] paymentTypeIDs = context.PaymentTypes.Select(i => i.ID).ToArray();
                    int paymentTypeCount = paymentTypeIDs.Count();

                    List<Order> orders = new List<Order>();

                    foreach (var memberID in memberIDs)
                    {
                        if ((memberID % 3) == 0) continue;

                        Order s = new Order
                        {
                            Date = DateTime.Now,
                            Total = 0,
                            MemberID = memberID,
                            PaymentTypeID = paymentTypeIDs[rnd.Next(paymentTypeCount)],
                        };
                        orders.Add(s);
                    }

                    context.Orders.AddRange(orders);
                    context.SaveChanges();

                    //Foreign Keys for OrderItems
                    int[] itemsIDs = context.Items.Select(i => i.ID).ToArray();
                    int itemsCount = itemsIDs.Count();

                    List<OrderItem> ois = new List<OrderItem>();

                    for(int i = 0; i < orders.Count; i++)
                    {
                        OrderItem oi = new OrderItem
                        {
                            ItemID = itemsIDs[rnd.Next(itemsCount - 1)],
                            OrderID = orders[i].ID,
                            Quantity = Faker.RandomNumber.Next(30)
                        };
                        ois.Add(oi);

                        Item item = context.Items.FirstOrDefault(i => i.ID == oi.ItemID);

                        orders[i].Total = oi.Quantity * item.Price;
                    }
                    context.Orders.UpdateRange(orders);                    
                    context.OrderItems.AddRange(ois);
                    context.SaveChanges();
                }

                // Look for any Employees.  Seed ones to match the seeded Identity accounts.
                if (!context.Employees.Any())
                {
                    context.Employees.AddRange(
                     new Employee
                     {
                         FirstName = "Gregory",
                         LastName = "House",
                         Email = "admin1@outlook.com",
                     },
                     new Employee
                     {
                         FirstName = "Betty",
                         LastName = "Rubble",
                         Email = "super1@outlook.com",
                     },
                     new Employee
                     {
                         FirstName = "George",
                         LastName = "Washington",
                         Email = "user1@outlook.com",
                     });

                    context.SaveChanges();
                }
            }
        }
    }
}
