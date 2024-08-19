using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class AlumniRecord{
        // General Info
        public int Id {get; set;}
        public int MentorID {get; set;}
        public String Name {get; set;}
        public String IPAddress {get; set;}
        public String Email {get; set;}
        public List<string> School {get; set;}
        public String Year {get; set;}
        public String Gender {get; set;}
        
        // Ethnicity Info
        public List<string> Ethnicity {get; set;}
        
        // Identity Info
        public List<string> Identity {get; set;}
        
        // Academic Info
        public List<string> Major {get; set;}
        public List<string> Minor {get; set;}
        public List<string> Certificate {get; set;}

        // Graduate School Info
        public List<string> GradArea {get; set;}
        
        // Location (State) Info
        public List<string> Location {get; set;}

        // Industry Info
        public List<string> Industry {get; set;}

        // Goal Info
        public List<string> Goal {get; set;}

        // Hobby Info
        public List<string> Hobby {get; set;}

        // Scale Info
        public int EthnicityWeighting {get; set;}
        public int IdentityWeighting {get; set;}
        public int AcademicWeighting {get; set;}
        public int GraduateWeighting {get; set;}
        public int IndustryWeighting {get; set;}
        public int LocationWeighting {get; set;}
        public int GoalWeighting {get; set;}
        public int HobbyWeighting {get; set;}
        
        
        // Willingness to take multiple mentees
        public int NumStudent {get; set;}

        public int IterationID {get; set;}
    }
}
