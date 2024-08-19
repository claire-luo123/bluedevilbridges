using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class PastMatchesRecord{
        public int Id{get; set;}
        public String Name { get; set; }
        public int NumOfPairs{get; set;}
        public int NumOfMentors{get; set;}
        public int NumOfMentees{get; set;}
        public double AverageSimScore{get; set;}
        public double MaxSimScore{get; set;}
        public double MinSimScore{get; set;}
        public DateTime MatchDate { get; set; }
    }
}