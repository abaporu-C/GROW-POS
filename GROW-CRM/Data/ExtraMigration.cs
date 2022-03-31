using Microsoft.EntityFrameworkCore.Migrations;

namespace GROW_CRM.Data
{
    public class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetMemberTimestampOnUpdate
                    AFTER UPDATE ON Households
                    BEGIN
                        UPDATE Households
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetHouseHoldTimestampOnInsert
                    AFTER INSERT ON Households
                    BEGIN
                        UPDATE Households
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

            //migrationBuilder.Sql(
            //    @"
            //        Create View PlacementSummaries as
            //        Select 'A' || a.AthleteCode as ID, a.LastName || ', ' || a.FirstName as Athlete, c.Code as Contingent, 
            //            a.MediaInfo as Media_Info, Avg(p.Place) as Average,
            //        Min(p.Place) as Highest, Max(p.Place) as Lowest, Count(p.ID) as Total_Events, Count(Distinct s.ID) as Number_of_Sports
            //        From Sports s Join Events e on s.ID = e.SportID
            //          Join Placements p on e.ID = p.EventID
            //          Join Athletes a on p.AthleteID = a.ID 
            //          Join Contingents c on a.ContingentID = c.ID
            //        Group By a.AthleteCode, a.LastName, a.FirstName, a.MediaInfo, c.Code;
            //    ");
        }
    }
}
