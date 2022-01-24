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


                //Look For Dietary Restrictions
                if (!context.DietaryRestrictions.Any())
                {
                    //Restrictions Array
                    string[] restrictions = new string[] { "Diabetes", "Obesity", "Lactose Intolerant", "Gluten Intolerance/Sensitivity", 
                                                            "Cancer", "Heart Disease", "Osteoporosis", "Digestive Disorders", "Food Allergies"};

                    //List of new Dietary Restriction Objects
                    List<DietaryRestriction> dietaryRestrictions = new List<DietaryRestriction>();

                    //Add DietaryRestrictions to dietaryRestrictions List
                    for (int i = 0; i < restrictions.Count(); i++)
                        dietaryRestrictions.Add(new DietaryRestriction { Restriction = restrictions[i] });

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

                //Look for Income Situation
                if (!context.IncomeSituations.Any())
                {
                    //array of situations
                    string[] situations = new string[] { "ODSP", "Ontario Works", "CPP-Disability", "EI", "GAINS", "Post. Sec. Student",
                                                         "Other", "Volunteer"};

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
                
                //Looks for Items
                if (!context.Items.Any())
                {
                    //array of items fields
                    string[] names = new string[] { "Carrot", "Flour", "Pork Chops", "Olive Oil", "White Rice", "Avocado"};                    
                    decimal[] prices = new decimal[] { 3.99M, 1.50M, 5.99M, 3.50M };

                    //List of Item Objects
                    List<Item> items = new List<Item>();                    

                    //add items to list
                    for (int i = 0; i < names.Count(); i++)
                        items.Add(new Item { Name = names[i], Price = prices[rnd.Next(0, prices.Length)]});

                    //add list to context
                    context.Items.AddRange(items);

                    //save changes
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

                //Look for Provinces
                if (!context.Provinces.Any())
                {
                    var provinces = new List<Province>
                    {
                        new Province { Code = "ON", Name = "Ontario"},
                        new Province { Code = "PE", Name = "Prince Edward Island"},
                        new Province { Code = "NB", Name = "New Brunswick"},
                        new Province { Code = "BC", Name = "British Columbia"},
                        new Province { Code = "NL", Name = "Newfoundland and Labrador"},
                        new Province { Code = "SK", Name = "Saskatchewan"},
                        new Province { Code = "NS", Name = "Nova Scotia"},
                        new Province { Code = "MB", Name = "Manitoba"},
                        new Province { Code = "QC", Name = "Quebec"},
                        new Province { Code = "YT", Name = "Yukon"},
                        new Province { Code = "NU", Name = "Nunavut"},
                        new Province { Code = "NT", Name = "Northwest Territories"},
                        new Province { Code = "AB", Name = "Alberta"}
                    };
                    context.Provinces.AddRange(provinces);
                    context.SaveChanges();
                }

                //Look for Households
                if (!context.Households.Any())
                {
                    //Foreign Keys
                    int[] provincesIDs = context.Provinces.Select(p => p.ID).ToArray();
                    int provinceCount = provincesIDs.Count();

                    //Add Households to context
                    context.Households.AddRange(
                        new Household
                        {
                            StreetNumber = 65,
                            StreetName = "Church St.",
                            AptNumber = 201,
                            City = "St. Catherines",
                            PostalCode = "R3E9C8",
                            YearlyIncome = 25000M,
                            NumberOfMembers = 2,
                            LICOVerified = true,
                            ProvinceID = provincesIDs[rnd.Next(provinceCount)]
                        },
                        new Household
                        {
                            StreetNumber = 38,
                            StreetName = "Church St.",                            
                            City = "St. Catherines",
                            PostalCode = "R3E 9R8",
                            YearlyIncome = 25000M,
                            NumberOfMembers = 3,
                            LICOVerified = true,
                            ProvinceID = provincesIDs[rnd.Next(provinceCount)]
                        }
                    );

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

                    int[] incomeSituationIDs = context.IncomeSituations.Select(i => i.ID).ToArray();
                    int incomeSituationCount = incomeSituationIDs.Count();

                    //Name Generator
                    string[] firstNames = new string[] { "Mark", "Pedro", "Roger", "Hitome", "Lin", "Brendon", "Michelle", "Leticia", "Love", "Jennifer", "Shadwick" };
                    string[] middleNames = new string[] { "M", "L", "K", "P", "A", "T", "R", "G"};
                    string[] lastNames = new string[] { "Pereira", "Martin", "Scott", "McTavish", "Smith", "Castro", "Lee", "Zhang", "Cruise"};

                    //Start DOB
                    DateTime startDOB = Convert.ToDateTime("1992-08-22");

                    //Loop over wach household and assign family members to it
                    for (int i = 0; i < householdCount; i++)
                    {
                        string lastName = lastNames[rnd.Next(lastNames.Count())];
                        int[] numberOfMembers = context.Households.Where(h => h.ID == householdIDs[i]).Select(h => h.NumberOfMembers).ToArray();

                        for(int j = 0; j < numberOfMembers[0]; j++)
                        {
                            context.Members.Add(
                                new Member
                                {
                                    FirstName = firstNames[rnd.Next(firstNames.Count())],
                                    MiddleName = middleNames[rnd.Next(middleNames.Count())],
                                    LastName = lastName,
                                    DOB = startDOB.AddDays(rnd.Next(60, 6500)),
                                    PhoneNumber = "0000000000",
                                    Email = "mail@mail.com",
                                    Notes = baconNotes[rnd.Next(5)],
                                    GenderID = genderIDs[rnd.Next(genderCount)],
                                    HouseholdID = householdIDs[i],
                                    IncomeSituationID = incomeSituationIDs[rnd.Next(incomeSituationCount)]
                                }    
                            );

                            context.SaveChanges();
                        }
                    }
                }                
            }
        }
    }
}