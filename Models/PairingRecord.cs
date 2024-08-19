using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Models{
    public class PairingRecord{
        public int Id {get; set;}
        public int MentorID{get; set;}
        public string MentorName{get; set;}
        public string MentorEmail{get;set;}
        public int MenteeID{get; set;}
        public string MenteeName{get; set;}
        public string MenteeEmail{get;set;}
        public double SimilarityScore{get; set;}
        public int IterationID {get; set;}
    }
}