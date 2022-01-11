using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot.Database
{
    public class BoardroomDatum
    {
        public int BoardroomDataID { get; set; }
        public int Epoch { get; set; }
        public Decimal TWAP { get; set; }
        public Decimal? PEG { get; set; }
        public DateTime Created { get; set; }

        public static BoardroomDatum RecordBoardRoomData(int epoch, Decimal twap, Decimal? peg)
        {
            // check if previous epoch was recorded
            // If not record previous epoch data
            BoardroomDatum newRecord = null;
            using var dbContext = new SQLiteDbContext();
            dbContext.Database.EnsureCreated();

            if (!dbContext.BoardroomData.Where(x => x.Epoch == epoch).Any())
            {
                newRecord = new BoardroomDatum()
                {
                    Epoch = epoch,
                    TWAP = (Decimal)twap,
                    PEG = peg,
                    Created = DateTime.Now,
                };
                dbContext.BoardroomData.Add(newRecord);

                dbContext.SaveChanges();
                Logging.WriteToConsole($"Saved record to DB:");
                dbContext.BoardroomData?.ToList().ForEach(bd =>
                {
                    Logging.WriteToConsole($"{bd.Epoch} TWAP: {bd.TWAP} PEG: {bd.PEG} Recorded: {bd.Created}");
                });
            }

            return newRecord;
        }
    }
}
