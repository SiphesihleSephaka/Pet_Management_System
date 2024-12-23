using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pet_Management_System.Models
{
    public class Meths
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        public DateTime GetWorkingDay(DateTime startDate, int daysToAdd)
        {
            int count = 0;
            DateTime result = startDate;
            while (count < daysToAdd)
            {
                result = result.AddDays(1);
                if (result.DayOfWeek != DayOfWeek.Sunday && result.DayOfWeek != DayOfWeek.Saturday)
                {
                    count++;
                }
            }
            return result;
        }
        private static readonly Random random = new Random();

        public static int GenerateRandomCode()
        {
             
            
            // Generate a random 4-digit code
            int code = random.Next(1000, 10000);
           
            return code;
        }
    }
}