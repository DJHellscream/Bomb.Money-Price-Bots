using BombMoney.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombMoney.Database
{
    public class BoardroomDatum
    {
        public int BoardroomDataID { get; set; }
        public int Epoch { get; set; }
        public decimal TWAP { get; set; }
        public decimal? PEG { get; set; }
        public DateTime Created { get; set; }

        public static BoardroomDatum RecordBoardRoomData(int epoch, decimal twap, decimal? peg)
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
                    TWAP = twap,
                    PEG = peg,
                    Created = DateTime.Now,
                };
                dbContext.BoardroomData.Add(newRecord);

                dbContext.SaveChanges();
                Logging.WriteToConsole($"Saved record to DB: {newRecord}");
            }

            return newRecord;
        }

        public override string ToString()
        {
            return $"EPOCH: {Epoch} TWAP: {TWAP} PEG: {PEG} Recorded: {Created}";
        }
    }
}
