/*
    The logic behind our similarity calculations:
        1. For the ones that we use Similarity Matrix
        Example:
            In a n*n similarity matrix, row number shows the index of mentor major, col number shows the index of mentee major
            Suppose mentor has n majors and mentee has m majors
            This creates n*m combinations of possible major similarity scores
            Let x be the highest number in n or m
            We arrange the n*m combinations of similarity scores in descending orders and only take the top x ones and add them all together
            We then take the sum from last step and divide it over x to make sure it's weighted
        
            This goes the same for similarity sections that requires similarity matrix - Major, Minor, Industry, Location
            We compare not only perfect matches, but also near matches

        2. For the ones we compare using .Equals()
        Example:
            If the mentor hobby is ever the same as the mentee hobby, the hobby similarity score +1
            Let x be the number of mentor hobby number times mentee hobby number
            We then take the hobby similarity score from last step and divide it over x to make sure it's weighted

        Then we time each similarity score with mentor's weighting + mentee's weighting for that section
        And we add them all together and divide by the total weighting

        Example:
        Academic similarity score: 1.0
        Hobby similarity score: 0.8
        Mentor Academic weighting (on a scale of 0 to 10): 4
        Mentor Hobby weighting (on a scale of 0 to 10): 10
        Mentee Academic weighting (on a scale of 0 to 10): 8
        Mentee Hobby weighting (on a scale of 0 to 10): 2
        Overall Score = (1.0 * (4 + 8) + 0.8 * (10 + 2))/(4 + 10 + 8 + 2)
*/
        
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using BlueDevilBridges.Data;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Data_analytics
{
    public class PairScore
    {
        private readonly IJSRuntime _js;

        private readonly BlueDevilBridgesContext _context;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public PairScore(BlueDevilBridgesContext context, IJSRuntime js, IWebHostEnvironment hostingEnvironment) {
            _context = context;
            _js = js;
            _hostingEnvironment = hostingEnvironment;
        }
        public int thresh = 0;
        public int Major_num = 58;
        public int Minor_num = 61;
        public int Industry_num = 44;
        public int Location_num = 21;
        public int ID{get;set;}
        public PastMatchesRecord current{get;set;}
  
        public string[] Majors = {
        "African and African American Studies","Ancient Religion and Society","Art History",
        "Art History and Visual Arts","Asian and Middle Eastern Studies","Biology","Biomedical Engineering",
        "Biophysics","Brazilian and Global Portuguese Studies","Chemistry","Civil Engineering",
        "Classical Civilization","Classical Languages","Computational Media","Computer Science",
        "Cultural Anthropology","Dance","Earth and Climate Sciences","Economics","Electrical and Computer Engineering",
        "English","Environmental Engineering","Environmental Sciences","Environmental Sciences and Policy",
        "Evolutionary Anthropology","French Studies","Gender Sexuality and Feminist Studies","German",
        "Global Cultural Studies","Global Health","History","Interdisciplinary Engineering and Applied Science (IDEAS)",
        "International Comparative Studies","Italian and European Studies","Linguistics","Linguistics and Computer Science",
        "Marine Science and Conservation","Mathematics","Mechanical Engineering","Medieval and Renaissance Studies",
        "Music","Neuroscience","Philosophy","Physics","Political Science","Psychology","Public Policy Studies",
        "Religious Studies","Romance Studies","Russian","Slavic and Eurasian Studies","Sociology",
        "Spanish Latin American and Latinx Studies","Statistical Science","Theater Studies","Visual Arts",
        "Visual and Media Studies","Undecided","None"
        };
  
        public string[] Minors = {
        "African and African American Studies","Art History","Asian American and Diaspora Studies",
        "Asian and Middle Eastern Studies","Biology","Brazilian and Global Portuguese Studies",
        "Chemistry","Cinematic Arts","Classical Archaeology","Classical Civilization","Computational Biology and Bioinformatics",
        "Computational Media","Computer Science","Creative Writing","Cultural Anthropology","Dance",
        "Earth and Climate Sciences","Economics","Education","Electrical and Computer Engineering",
        "Energy Engineering","English","Environmental Sciences and Policy","Evolutionary Anthropology",
        "Finance","French Studies","Gender Sexuality and Feminist Studies","German","Global Cultural Studies",
        "Global Health","Greek","History","Inequality Studies","Italian Studies","Latin","Linguistics",
        "Machine Learning and Artificial Intelligence","Marine Science and Conservation","Mathematics","Medical Sociology",
        "Medieval and Renaissance Studies","Music","Musical Theater","Neuroscience","Philosophy",
        "Photography","Physics","Polish Culture and Language","Political Science","Psychology","Religious Studies",
        "Russian and East European Literatures in Translation","Russian Culture and Language",
        "Sexuality Studies","Sociology","Spanish Studies","Statistical Science","Theater Studies",
        "Visual Arts","Visual and Media Studies","None"
       };
  
        public string[] Certificates = {
        "Aerospace Engineering","Architectural Engineering","Child Policy Research","Decision Sciences",
        "Digital Intelligence","Documentary Studies","East Asian Studies","Energy and the Environment",
        "Ethics and Society","Global Development Engineering","Health Policy","Human Rights",
        "Information Science and Information Studies","Innovation and Entrepreneurship","Islamic Studies",
        "Jewish Studies","Latin American Studies","Latinx Studies in the Global South","Markets and Management",
        "Materials Science and Engineering","Philosophy Politics and Economics","Policy Journalism and Media Studies",
        "Robotics and Automation","Science and the Public","Sustainability Engagement","None"
        };
  
        public string[] Industries = {
        "Technology / Software / IT", "Finance / Investment / Banking", "Medical",
        "Consulting and professional services", "Healthcare", "Nursing", "Education",
        "Journalism", "Marketing / Advertising", "Arts / Creative Industries",
        "Non-Profit and Social Services", "Law / Legal Services", "Hospitality and Tourism",
        "Manufacturing and engineering", "Retail and e-commerce", "Energy and Utilities",
        "Politics / Government and public sector", "Real estate", "Construction", "Food and beverage",
        "Media Entertainment and Sports", "Transportation and logistics", "Fashion and Apparel",
        "STEM", "Artificial Intelligence / Machine Learning / Data Science",
        "Pharmaceutical", "Biotechnology", "Research and Development", "Telecommunications",
        "Aerospace and Defense", "Agricultural and Food Science", "Environmental Science",
        "Product / Product Management / UX Design", "Veterinary Medicine", "Venture Capital",
        "Accounting", "Private Equity", "Architecture and Design", "Military", "International Relations",
        "Academia / Higher Ed", "Religion", "Physical Therapy", "Dentistry","None"
        };
  
        public string[] Locations = {
        "Raleigh/Durham/Chapel Hill",
        "New York City/Tri-State area",
        "Washington DC",
        "Northern California",
        "Boston MA",
        "Atlanta GA",
        "Southern California",
        "Charlotte NC",
        "Philadelphia and mid/southern NJ",
        "Chicago IL",
        "Pacific Northwest/Seattle",
        "Houston/Dallas/Austin",
        "Nashville TN",
        "Denver CO",
        "Miami/Central Florida/Tampa Bay",
        "Baltimore MD",
        "Richmond VA",
        "UK",
        "EU",
        "China",
        "India",
        "None"
        };

        public Dictionary<int, Info> mentorD;
        public Dictionary<int, Info> menteeD;
        public decimal[,] scores;
        public int columnNum = 25;

        public decimal[,] Major_Sim;
        public decimal[,] Minor_Sim;
        public decimal[,] Industry_Sim;
        public decimal[,] Location_Sim;

        public decimal avgScore;
        public decimal min;
        public decimal max;

        public List<int> mentorReId = new List<int>();
        public List<int> menteeReId = new List<int>();
        public int mentorNum;
        public int menteeNum;
        public int[] mentorID;
        public int[] menteeID;
        public int iteration;
        private int z0_row;
        private int z0_col;

        public async Task<string[,]> CalculateScores(IBrowserFile mentor, IBrowserFile mentee)
        {
            string logFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "logs.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            // Loading the similarity matrix for Major, Minor, Industry and Location
            try
            {
                string webRootPath1 = _hostingEnvironment.WebRootPath;
                string filePath1 = Path.Combine(webRootPath1, "Major_Similarity.csv");
                Major_Sim = await LoadSimMatrix(filePath1, Major_num);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading major similarity matrix: {ex.Message}", ex);
            }
            try
            {
                string webRootPath2 = _hostingEnvironment.WebRootPath;
                string filePath2 = Path.Combine(webRootPath2, "Minor_Similarity.csv");
                Minor_Sim = await LoadSimMatrix(filePath2, Minor_num);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading minor similarity matrix: {ex.Message}", ex);
            }            
            try
            {
                string webRootPath3 = _hostingEnvironment.WebRootPath;
                string filePath3 = Path.Combine(webRootPath3, "Industry_Similarity.csv");
                Industry_Sim = await LoadSimMatrix(filePath3, Industry_num);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading industry similarity matrix: {ex.Message}", ex);
            }
            try
            {
                string webRootPath4 = _hostingEnvironment.WebRootPath;
                string filePath4 = Path.Combine(webRootPath4, "Location_Similarity.csv");
                Location_Sim = await LoadSimMatrix(filePath4, Location_num);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading location similarity matrix: {ex.Message}", ex);
            }
            // Record update
            var mostRecentRecord = _context.PastMatchesRecords
            .OrderByDescending(record => record.Id)
            .FirstOrDefault();
            current = new PastMatchesRecord(){Id=mostRecentRecord.Id+1,MatchDate=DateTime.UtcNow};
            ID=current.Id;
            // Initialization of mentor and mentee Dictionary<int,Info>
            mentorD = await SaveFile(mentor, false);
            menteeD = await SaveFile(mentee, true);
            mentorNum = mentorD.Count;
            menteeNum = menteeD.Count;
            
            // Get the scoring matrix
            decimal[,] scores = await GetScoreMatrix(mentorD, menteeD);

            // Matches the pairs using the scoring matrix
            string[,] pairingRecords = await StartMatching(scores, ID);

            // Statistics update
            avgScore = 0;
            min = 100;
            max = 0;
            for(int i = 1; i < pairingRecords.GetLength(0) - 1; i++)
            {
                decimal s1 = decimal.Parse(pairingRecords[i, 7]); 
                avgScore += s1;
                if (s1 < min)
                {
                    min = s1;
                }
                if (s1 > max)
                {
                    max = s1;
                }
            }
            if (pairingRecords.GetLength(0) > 1)
            {
                avgScore = avgScore / pairingRecords.GetLength(0) - 1;
            }
            current.AverageSimScore = (double)avgScore;
            current.MaxSimScore = (double)max;
            current.MinSimScore = (double)min;
            current.NumOfMentors = mentorNum;
            current.NumOfMentees = menteeNum;

            current.NumOfPairs = pairingRecords.GetLength(0) - 1;
            _context.Add(current);
            await _context.SaveChangesAsync();
            await LogAsync($"Finished CalculateScores");
            return pairingRecords;
        }
        
        // Save the file of mentor/mentee information into the Database
        // Return a Dictionary with count num being the key and Info being the value
        public async Task<Dictionary<int, Info>> SaveFile(IBrowserFile file, Boolean mentee)
        {
            var D = new Dictionary<int, Info>();
            using (var reader = new StreamReader(file.OpenReadStream(1000000)))
            {
                string headerLine = await reader.ReadLineAsync();
                string line;
                int key = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var columns = line.Split(',');
                    if(columns.Length == 23){
                        key += 1;
                        var info = new Info
                        {
                            Count = key,
                            Name = columns[0],
                            Email = columns[1],
                            School = new List<string>(columns[2].Split(";")),
                            Year = columns[3],
                            Ethnicity = new List<string>(columns[4].Split(";")),
                            Identity = new List<string>(columns[5].Split(";")),
                            Major = new List<string>(columns[6].Split(";")),
                            Minor = new List<string>(columns[7].Split(";")),
                            Certificate = new List<string>(columns[8].Split(";")),
                            GradArea = new List<string>(columns[9].Split(";")),
                            Location = new List<string>(columns[10].Split(";")),
                            Industry = new List<string>(columns[11].Split(";")),
                            Goal = new List<string>(columns[12].Split(";")),
                            Hobby = new List<string>(columns[13].Split(";")),
                            EthnicityWeighting = Int32.Parse(columns[14]),
                            IdentityWeighting = Int32.Parse(columns[15]),
                            AcademicWeighting = Int32.Parse(columns[16]),
                            GraduateWeighting = Int32.Parse(columns[17]),
                            IndustryWeighting = Int32.Parse(columns[18]),
                            LocationWeighting = Int32.Parse(columns[19]),
                            GoalWeighting = Int32.Parse(columns[20]),
                            HobbyWeighting = Int32.Parse(columns[21]),
                            NumStudent = Int32.Parse(columns[22])
                        };
                        D.Add(key, info);

                        if(!mentee) {
                            var alumniRecord = new AlumniRecord() {MentorID = info.Count, Name = info.Name, IPAddress = info.IPAddress, 
                            Email = info.Email, School = info.School, Year = info.Year, Gender = info.Gender, Ethnicity = info.Ethnicity,
                            Identity = info.Identity, Major = info.Major, Minor = info.Minor, Certificate = info.Certificate, 
                            GradArea = info.GradArea, Location = info.Location, Industry = info.Industry, Goal = info.Goal,
                            Hobby = info.Hobby, EthnicityWeighting = info.EthnicityWeighting, IdentityWeighting = info.IdentityWeighting,
                            AcademicWeighting = info.AcademicWeighting, GraduateWeighting = info.GraduateWeighting, 
                            IndustryWeighting = info.IndustryWeighting, LocationWeighting = info.LocationWeighting, 
                            GoalWeighting = info.GoalWeighting, HobbyWeighting = info.HobbyWeighting, NumStudent = info.NumStudent, IterationID=ID};

                            _context.Add(alumniRecord);
		                    await _context.SaveChangesAsync();

                        }
                        else { //mentee
                            var studentRecord = new StudentRecord() {MenteeID=info.Count, Name = info.Name, IPAddress = info.IPAddress, 
                            Email = info.Email, School = info.School, Year = info.Year, Gender = info.Gender, Ethnicity = info.Ethnicity,
                            Identity = info.Identity, Major = info.Major, Minor = info.Minor, Certificate = info.Certificate, 
                            GradArea = info.GradArea, Location = info.Location, Industry = info.Industry, Goal = info.Goal,
                            Hobby = info.Hobby, EthnicityWeighting = info.EthnicityWeighting, IdentityWeighting = info.IdentityWeighting,
                            AcademicWeighting = info.AcademicWeighting, GraduateWeighting = info.GraduateWeighting, 
                            IndustryWeighting = info.IndustryWeighting, LocationWeighting = info.LocationWeighting, 
                            GoalWeighting = info.GoalWeighting, HobbyWeighting = info.HobbyWeighting, NumStudent = info.NumStudent,IterationID=ID};

                            _context.Add(studentRecord);
		                    await _context.SaveChangesAsync();

                        }
                                            
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid number of columns in data.");
                    }
                }
            }
            return D;
        }

        // Load the similarity matrix as decimal 2D array
        public async Task<decimal[,]> LoadSimMatrix(string filePath, int num)
        {
            var simMatrix = new decimal[num, num];
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                });

                // Skip the header row
                await csv.ReadAsync();

                for (int rowIndex = 0; await csv.ReadAsync() && rowIndex < num; rowIndex++)
                {

                    for (int colIndex = 0; colIndex < num; colIndex++)
                    {
                        if (csv.TryGetField<decimal>(colIndex + 1, out var value))
                        {
                            try
                            {
                                simMatrix[rowIndex, colIndex] = value;
                            }
                            catch (IndexOutOfRangeException ex)
                            {
                                await LogAsync($"Index error at rowIndex: {rowIndex}, colIndex: {colIndex}");
                                await LogAsync($"num: {num}");
                                await LogAsync($"filepath: {filePath}");
                                throw;
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException($"CSV file not found at {filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when loading the similarity matrix.", ex);
            }
            return simMatrix;
        }

        public async Task<decimal> AcademicSim(Info mentor, Info mentee)
        {   
            // Calculate the similarity score for major similarity (major: 10, minor: 3, certificate: 3)
            decimal score = (mentor.AcademicWeighting + mentee.AcademicWeighting) * (1m * await MajorSim(mentor,mentee) + 0.3m * await MinorSim(mentor,mentee) + 0.3m * await CertSim(mentor,mentee)) / 1.6m;
            return score; // Return a score between 0 and 1
        }

        public async Task<decimal> MajorSim(Info mentor, Info mentee)
        {
            // Calculate the Similarity of Major
            if (mentor.Major == null || mentee.Major == null) return 0;
            if (mentor.Major.Contains("Undecided") || mentee.Major.Contains("Undecided")) return 0;
            if (mentor.Major.Contains("None") || mentee.Major.Contains("None")) return 0;
            if (mentor.Major.Contains("") || mentee.Major.Contains("")) return 0;
            if (mentor.Major.Count == mentee.Major.Count)
            {
                if (mentor.Major.SequenceEqual(mentee.Major)) return 1;
            }
            int MatchCount = 0;
            decimal MajorScore = 0;
            var MajorScores = new List<decimal>();
            int ScoreCount = Math.Max(mentor.Major.Count, mentee.Major.Count);
            foreach (string mentor_major in mentor.Major)
            {
                string trimmedMentorMajor = mentor_major.Trim().ToLower();
                if (Majors.Any(major => major.Trim().ToLower() == trimmedMentorMajor))
                {
                    int index_mentor_major = Array.FindIndex(Majors, major => major.Trim().ToLower() == trimmedMentorMajor);
                    foreach (string mentee_major in mentee.Major)
                    {
                        string trimmedMenteeMajor = mentee_major.Trim().ToLower();
                        if (Majors.Any(major => major.Trim().ToLower() == trimmedMenteeMajor))
                        {
                            int index_mentee_major = Array.FindIndex(Majors, major => major.Trim().ToLower() == trimmedMenteeMajor);
                            MajorScores.Add(Major_Sim[index_mentor_major, index_mentee_major]);
                            MatchCount ++;
                        }
                        else
                        {
                            await LogAsync($"Mentee major not in list: {mentee_major}");
                        }
                    }
                }
                else
                {
                    await LogAsync($"Mentor major not in list: {mentor_major}");
                }
            }

            if(MatchCount > 0)
            {
                var groupedScores = MajorScores
                .GroupBy(n => n)
                .OrderByDescending(m => m.Count())
                .ThenByDescending(m => m.Key)
                .SelectMany(m => Enumerable.Repeat(m.Key, m.Count()))
                .ToList();
                var TheMostOccurring = groupedScores.Take(Math.Min(MatchCount, ScoreCount)).ToList();
                foreach(decimal s in TheMostOccurring)
                {
                    MajorScore += s;
                }
                MajorScore = MajorScore / Math.Min(MatchCount, ScoreCount);
            }
            return MajorScore;
        }

        public async Task<decimal> MinorSim(Info mentor, Info mentee)
        {
            if(mentor.Minor == null || mentee.Minor == null) return 0;
            if (mentor.Minor.Contains("None") || mentee.Minor.Contains("None")) return 0;
            if (mentor.Minor.Contains("") || mentee.Minor.Contains("")) return 0;
            if (mentor.Minor.Count == mentee.Minor.Count)
            {
                if (mentor.Minor.SequenceEqual(mentee.Minor)) return 1;
            }
            int MatchCount = 0;
            decimal MinorScore = 0;
            var MinorScores = new List<decimal>();
            int ScoreCount = Math.Max(mentor.Minor.Count, mentee.Minor.Count);
            foreach(string mentor_minor in mentor.Minor)
            {
                string trimmedMentorMinor = mentor_minor.Trim().ToLower();
                if(Minors.Any(minor => minor.Trim().ToLower() == trimmedMentorMinor))
                {
                    int index_mentor_minor = Array.FindIndex(Minors, minor => minor.Trim().ToLower() == trimmedMentorMinor);
                    foreach (string mentee_minor in mentee.Minor)
                    {
                        string trimmedMenteeMinor = mentee_minor.Trim().ToLower();
                        if (Minors.Any(minor => minor.Trim().ToLower() == trimmedMenteeMinor))
                        {
                            int index_mentee_minor = Array.FindIndex(Minors, minor => minor.Trim().ToLower() == trimmedMenteeMinor);
                            MinorScores.Add(Minor_Sim[index_mentor_minor, index_mentee_minor]);
                            MatchCount ++;
                        }
                        else
                        {
                            await LogAsync($"Mentee minor not in list: {mentee_minor}");
                        }
                    }
                }
                else
                {
                    await LogAsync($"Mentor minor not in list: {mentor_minor}");
                }
            }
            if(MatchCount > 0)
            {
                var groupedScores = MinorScores
                .GroupBy(n => n)
                .OrderByDescending(m => m.Count())
                .ThenByDescending(m => m.Key)
                .SelectMany(m => Enumerable.Repeat(m.Key, m.Count()))
                .ToList();
                var TheMostOccurring = groupedScores.Take(Math.Min(MatchCount, ScoreCount)).ToList();
                foreach(decimal s in TheMostOccurring)
                {
                    MinorScore += s;
                }
                MinorScore = MinorScore / Math.Min(MatchCount, ScoreCount);
            }
            return MinorScore;
        }

        public async Task<decimal> IndustrySim(Info mentor, Info mentee)
        {
            if (mentor.Industry.Contains("") || mentee.Industry.Contains("")) return 0;
            if(mentor.Industry == null || mentee.Industry == null) return 0;
            if (mentor.Industry.Count == mentee.Industry.Count)
            {
                if (mentor.Industry.SequenceEqual(mentee.Industry)) return 1;
            }
            int MatchCount = 0;
            decimal IndustryScore = 0;
            var IndustryScores = new List<decimal>();
            int ScoreCount = Math.Max(mentor.Industry.Count, mentee.Industry.Count);
            foreach(string mentor_industry in mentor.Industry) {
                string trimmedMentorIndustry = mentor_industry.Trim().ToLower();
                if(Industries.Any(industry => industry.Trim().ToLower() == trimmedMentorIndustry))
                {
                    int index_mentor_industry = Array.FindIndex(Industries, industry => industry.Trim().ToLower() == trimmedMentorIndustry);
                    foreach(string mentee_industry in mentee.Industry)
                    {
                        string trimmedMenteeIndustry = mentee_industry.Trim().ToLower();
                        if (Industries.Any(industry => industry.Trim().ToLower() == trimmedMenteeIndustry))
                        {
                            int index_mentee_industry = Array.FindIndex(Industries, industry => industry.Trim().ToLower() == trimmedMenteeIndustry);
                            IndustryScores.Add(Industry_Sim[index_mentor_industry, index_mentee_industry]);
                            MatchCount ++;
                        }
                        else
                        {
                            await LogAsync($"Mentee industry not in list: {mentee_industry}");
                        }
                    }
                }
                else
                {
                    await LogAsync($"Mentor industry not in list: {mentor_industry}");
                }
            }
    
            if(MatchCount > 0)
            {
                var groupedScores = IndustryScores
                .GroupBy(n => n)
                .OrderByDescending(m => m.Count())
                .ThenByDescending(m => m.Key)
                .SelectMany(m => Enumerable.Repeat(m.Key, m.Count()))
                .ToList();
                var TheMostOccurring = groupedScores.Take(Math.Min(MatchCount, ScoreCount)).ToList();
                foreach(decimal s in TheMostOccurring)
                {
                    IndustryScore += s;
                }
                IndustryScore = ((mentor.IndustryWeighting + mentee.IndustryWeighting) * IndustryScore) / Math.Min(MatchCount, ScoreCount);
            }
            return IndustryScore;
        }

        public async Task<decimal> LocationSim(Info mentor, Info mentee)
        {
            if (mentor.Location == null || mentee.Location == null) return 0;
            if (mentor.Location.Contains("") || mentee.Location.Contains("")) return 0;
            if (mentor.Location.Count == mentee.Location.Count)
            {
                if (mentor.Location.SequenceEqual(mentee.Location)) return 1;
            }
            int MatchCount = 0;
            decimal LocationScore = 0;
            var LocationScores = new List<decimal>();
            int ScoreCount = Math.Max(mentor.Location.Count, mentee.Location.Count);
            foreach(string mentor_location in mentor.Location)
            {
                string trimmedMentorLocation = mentor_location.Trim().ToLower();
                if (Locations.Any(location => location.Trim().ToLower() == trimmedMentorLocation))
                {
                    int index_mentor_location = Array.FindIndex(Locations, location => location.Trim().ToLower() == trimmedMentorLocation);
                    foreach(string mentee_location in mentee.Location)
                    {
                        string trimmedMenteeLocation = mentee_location.Trim().ToLower();
                        if (Locations.Any(location => location.Trim().ToLower() == trimmedMenteeLocation))
                        {
                            int index_mentee_location = Array.FindIndex(Locations, location => location.Trim().ToLower() == trimmedMenteeLocation);
                            LocationScores.Add(Location_Sim[index_mentor_location, index_mentee_location]);
                            MatchCount ++;
                        }
                        else
                        {
                            await LogAsync($"Mentee location not in list: {mentee_location}");
                        }
                    }
                }
                else
                {
                    await LogAsync($"Mentor location not in list: {mentor_location}");
                }         
            }

            if (MatchCount > 0)
            {
                var groupedScores = LocationScores
                .GroupBy(n => n)
                .OrderByDescending(m => m.Count())
                .ThenByDescending(m => m.Key)
                .SelectMany(m => Enumerable.Repeat(m.Key, m.Count()))
                .ToList();
                var TheMostOccurring = groupedScores.Take(Math.Min(MatchCount, ScoreCount)).ToList();
                foreach(decimal s in TheMostOccurring)
                {
                    LocationScore += s;
                }
                LocationScore = ((mentor.LocationWeighting + mentee.LocationWeighting) * LocationScore) / Math.Min(MatchCount, ScoreCount);
            }
            return LocationScore;
        }

        public async Task<decimal> CertSim(Info mentor, Info mentee)
        {
            // Certificate Similarity Score
            // For certificate we will just put 1 for the exact same certificate
            if(mentor.Certificate == null || mentee.Certificate == null) return 0;
            decimal CertScore = 0;
            foreach(string mentor_cert in mentor.Certificate)
            {
                foreach(string mentee_cert in mentee.Certificate)
                {
                    if(mentor_cert.Equals("None") || mentee_cert.Equals("None")){
                        return 0;
                    }
                    if(mentor_cert.Equals(mentee_cert)) {
                        CertScore += 1;
                    }
                }
            }
            CertScore = CertScore / (mentor.Certificate.Count * mentee.Certificate.Count);
            return CertScore;
        }
        public async Task<decimal> GraduateSim(Info mentor, Info mentee)
        {
            // Calculate Graduate Interest Similarity
            if(mentor.GradArea == null || mentee.GradArea == null) return 0;
            decimal GraduateScore = 0;
            foreach(string mentor_grad in mentor.GradArea) {
                foreach(string mentee_grad in mentee.GradArea) {
                    if(mentor_grad.Equals("None") || mentee_grad.Equals("None")){
                        return 0;
                    }
                    if(mentor_grad.Equals(mentee_grad)) {
                        GraduateScore += 1;
                    }
                }
            }
            GraduateScore = ((mentor.GraduateWeighting + mentee.GraduateWeighting) * GraduateScore) / (mentor.GradArea.Count * mentee.GradArea.Count);
            return GraduateScore;
        }

        public async Task<decimal> GoalsSim(Info mentor, Info mentee)
        {
            // Calculate Goal Similarity
            if(mentor.Goal == null || mentee.Goal == null) return 0;
            decimal GoalScore = 0;
            foreach(string mentor_goal in mentor.Goal) {
                foreach(string mentee_goal in mentee.Goal) {
                    if(mentor_goal.Equals(mentee_goal)) {
                        GoalScore += 1;
                    }
                }
            }
            GoalScore = ((mentor.GoalWeighting + mentee.GoalWeighting) * GoalScore) / (mentor.Goal.Count * mentee.Goal.Count);
            return GoalScore;
        }

        public async Task<decimal> IdentitySim(Info mentor, Info mentee)
        {
            // Calculate the similarity score for identity
            // Idenitiy includes first gen/LGBTQ+/International/etc

            if(mentor.Identity == null || mentee.Identity == null) return 0;
            decimal IdentityScore = 0;
            foreach(string mentor_identity in mentor.Identity) {
                foreach(string mentee_identity in mentee.Identity) {
                    if(mentor_identity.Equals(mentee_identity)) {
                        IdentityScore += 1;
                    }
                }
            }
            IdentityScore = ((mentor.IdentityWeighting + mentee.IdentityWeighting) * IdentityScore) / (mentor.Identity.Count * mentee.Identity.Count);
            return IdentityScore;
        }

        public async Task<decimal> EthnicitySim(Info mentor, Info mentee)
        {
            // Calculate the similarity score for ethnicity
            if(mentor.Ethnicity == null || mentee.Ethnicity == null) return 0;
            decimal EthnicityScore = 0;
            foreach(string mentor_ethnicity in mentor.Ethnicity) {
                foreach(string mentee_ethnicity in mentee.Ethnicity) {
                    if(mentor_ethnicity.Equals(mentee_ethnicity)) {
                        EthnicityScore += 1;
                    }
                }
            }
            EthnicityScore = ((mentor.EthnicityWeighting + mentee.EthnicityWeighting) * EthnicityScore) / (mentor.Ethnicity.Count * mentee.Ethnicity.Count);
            return EthnicityScore;
        }

        public async Task<decimal> HobbiesSim(Info mentor, Info mentee)
        {
            //need to somehow get rid of all of the "other" responses from last year's data cause we plan on geting rid of that answer option
            // Calculate the similarity score for hobbies
            if(mentor.Hobby == null || mentee.Hobby == null) return 0;
            decimal HobbyScore = 0;
            foreach(string mentor_hobby in mentor.Hobby) {
                foreach(string mentee_hobby in mentee.Hobby) {
                    if(mentor_hobby.Equals(mentee_hobby)) {
                        HobbyScore += 1;
                    }
                }
            }
            HobbyScore = ((mentor.HobbyWeighting + mentee.HobbyWeighting) * HobbyScore) / (mentor.Hobby.Count * mentee.Hobby.Count);
            return HobbyScore;
        }

        public async Task<decimal> OverallSim(Info mentor, Info mentee)
        {
            // Calculate the overall similarity based on marginal info
            // Probably will revise after stakeholder meeting
            // The whole idea is to trying the best to avoid absolute 0 for any pair and to add variations to the score
            decimal MarginalScore = 0;
            /*if(mentor.Name.Substring(1).Equals(mentee.Name.Substring(1)))
            {
                MarginalScore += 0.01m;
            }
            if(mentor.Name.Substring(3).Equals(mentee.Name.Substring(3)))
            {
                MarginalScore += 0.03m;
            }
            if(mentor.Gender.Equals(mentee.Gender))
            {
                MarginalScore += 0.02m;
            }*/
            return MarginalScore;
        }

        public async Task<decimal> GetScore(Info mentor, Info mentee)
        {
            // Calculate the scores for each mentor and mentee
            if(mentor == null || mentee == null) return 0;
            try{
                var score = await OverallSim(mentor, mentee);
                score += (await AcademicSim(mentor, mentee) + await IndustrySim(mentor, mentee) + await LocationSim(mentor, mentee) + await GraduateSim(mentor, mentee) + await GoalsSim(mentor, mentee) + await IdentitySim(mentor, mentee) + await EthnicitySim(mentor, mentee) + await HobbiesSim(mentor, mentee));
                score /= (mentor.AcademicWeighting + mentee.AcademicWeighting + mentor.IndustryWeighting + mentee.IndustryWeighting + mentor.LocationWeighting + mentee.LocationWeighting + mentor.GraduateWeighting + mentee.GraduateWeighting + mentor.GoalWeighting + mentee.GoalWeighting + mentor.IdentityWeighting + mentee.IdentityWeighting + mentor.EthnicityWeighting + mentee.EthnicityWeighting + mentor.HobbyWeighting + mentee.HobbyWeighting);
                return score * 100m;
            }
            catch(Exception ex){
                await LogAsync($"Inside GetScore: {ex.Message}");
            }
            return 0;
        }

        public async Task LogAsync(string message)
        {
            // Log to the console
            await _js.InvokeVoidAsync("console.log", message);

            // Log to a text file
            string logFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "logs.txt");
            await AppendTextToFileAsync(logFilePath, message);
        }
        private async Task AppendTextToFileAsync(string filePath, string message)
        {
            // Ensure the log directory exists
            

            // Append the message to the file with a timestamp
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                await writer.WriteLineAsync($"{DateTime.Now}: {message}");
            }
        }

        public async Task<decimal[,]> GetScoreMatrix(Dictionary<int,Info> mentorD1, Dictionary<int,Info> menteeD1)
        {
            if (mentorD1 == null || menteeD1 == null)
            {
                throw new InvalidOperationException("mentorD or menteeD is not initialized.");
            }

            // Store the score for each pair
            decimal[,] scores = await DummyMatrix(mentorD1.Count, menteeD1.Count);
            int mentor_id = 1;
            decimal max = 0;
            int ind1 = 0;
            int ind2 = 0;
            foreach (var tor in mentorD1)
            {
                int mentee_id = 1;               
                foreach (var tee in menteeD1)
                {                    
                    Info mentor_info = tor.Value;
                    Info mentee_info = tee.Value;
                    try
                    {
                        scores[mentor_id - 1, mentee_id - 1] = await GetScore(mentor_info, mentee_info);
                        await LogAsync($"Score is {scores[mentor_id - 1, mentee_id - 1]} at {mentor_id - 1} {mentee_id - 1}");
                        if(scores[mentor_id -1, mentee_id -1] > max)
                        {
                            ind1 = mentor_id - 1;
                            ind2 = mentee_id - 1;
                            max = scores[mentor_id - 1, mentee_id - 1];
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogAsync($"{ex.Message}");
                        await LogAsync($"Error at mentor_id: {mentor_id}, mentee_id: {mentee_id}");
                        await LogAsync($"mentorD.Count: {mentorD1.Count}, menteeD.Count: {menteeD1.Count}");
                    }
                    mentee_id++;
                }
                mentor_id++;
            }
            await LogAsync($"Max Score is :{scores[ind1, ind2]} at {ind1}, {ind2}");
            return scores;
        }

        public async Task<decimal[,]> DummyMatrix(int n1, int n2)
        {
            decimal[,] newM = new decimal[n1, n2];
            if(n1 > n2){
                return new decimal[n1, n1];
            }
            if(n2 > n1)
            {
                return new decimal[n2, n2];
            }
            return newM;
        }
        
        public async Task<string[,]> StartMatching(decimal[,] scores, int iter)
        {
            /*
            Matching:
            Match mentors to mentees using the FindAssignments() method by passing into the 'scores' 2D decimal array
            FindAssignments() method uses Hungarian algorithm to return an int[] array called 'matrix' containing the index of the optimal matches
            Example: ideal matches are mentor 1 (index 0) with mentee 7 (index 6), mentor 2 (index 1) with mentee 3 (index 2)...
            The returned 1D array will look like this: [6, 2, ...]
            For each pair, we now know the index of the mentor and the index of the mentee
            We could find their similarity score by searching their index in the scores array: scores[ideal mentor id, ideal mentee id],
            where the ideal mentee id for the specific mentor id is matrix[mentor id]
            We then use this score to determine if they have passed the threshold value or not
            If so, index increments by 1 and we write the pair into a list 'pairingRecords' with variable type 'PairingRecord' from the Models and into the database
            If not, we store the mentor id and mentee id into lists 'mentorReId' and 'menteeReId' in case a rematch will be needed
            After iterating through the matrix, we now have all the pairs that passed the threshold value stored in 'pairingRecords'
            */
            iteration = iter;
            int[] matrix = FindAssignments(scores);
            var pairingRecords = new List<PairingRecord>();
            int index = 0;
            for(int i = 0; i < matrix.Length; i++)
            {
                await LogAsync($"Matched Mentor {i+1} with Mentee {matrix[i]+1} with score {scores[i,matrix[i]]}");
                foreach(var D1 in mentorD){
                    if (D1.Key == i+1)
                    {
                        foreach(var D2 in menteeD)
                        {
                            if (D2.Key == matrix[i] + 1)
                            {
                                double ss = (double)scores[i,matrix[i]];
                                if(ss > thresh)
                                {
                                    var pairingRecord = new PairingRecord() { MentorID = D1.Key, MentorName = D1.Value.Name, MentorEmail = D1.Value.Email, MenteeID = D2.Key, MenteeName = D2.Value.Name, MenteeEmail = D2.Value.Email, SimilarityScore = ss, IterationID = iteration};
                                    _context.Add(pairingRecord);
		                            await _context.SaveChangesAsync();
                                    index ++;
                                    pairingRecords.Add(pairingRecord);
                                }
                                else
                                {
                                    mentorReId.Add(D1.Key);
                                    menteeReId.Add(D2.Key);
                                }
                            }
                        }
                    }
                }
            }
            await LogAsync($"Index is: {index}");

            /*
            Rematching:
            Initiated if index is lower than the total number of mentees
            Now the lists 'mentorReId' and 'menteeReId' should contain all the mentor id and mentee id for matches that are lower than the threshold value
            We also add all id of the mentors who are willing to take multiple students
            We use the lists of id to get a list of mentor and mentee information:
                'newTor' is a Dictionary<int, Info> that stores the index and Info of the mentor that needs rematching
                'newTee' is a Dictionary<int, Info> that stores the index and Info of the mentee that needs rematching
            We pass the new Dictionaries into the GetScoreMatrix() method to get a new scores matrix called 'newScores'
            And obtain new index matrix 'newMatrix' using the FindAssignments() method
            We then add all into the pairingRecords and into the database
            And we convert the list pairingRecords into a 2D string array 'pairingArray' and return it back to the CalculateScores() method
            */
            if(index < menteeNum)
            {
                foreach(var m in mentorD)
                {
                    if(m.Value.NumStudent > 1)
                    {
                        mentorReId.Add(m.Key);
                    }
                }

                if (mentorNum < menteeNum)
                {
                    foreach (var record in menteeD)
                    {
                        bool isPaired = pairingRecords.Any(pr => pr.MenteeID == record.Key);
                        if (!isPaired && !menteeReId.Contains(record.Key))
                        {
                            menteeReId.Add(record.Key);
                        }
                    }
                }
                else if (mentorNum > menteeNum)
                {
                    foreach (var record in mentorD)
                    {
                        bool isPaired = pairingRecords.Any(pr => pr.MentorID == record.Key);
                        if (!isPaired && !mentorReId.Contains(record.Key))
                        {
                            mentorReId.Add(record.Key);
                        }
                    }
                }

                mentorReId = new HashSet<int>(mentorReId).ToList();
                menteeReId = new HashSet<int>(menteeReId).ToList();
                mentorReId.Sort();
                menteeReId.Sort();

                foreach(var x in mentorReId)
                {
                    await LogAsync($"Rematch mentor Id: {x}");
                }
                foreach(var y in menteeReId)
                {
                    await LogAsync($"Rematch mentee Id: {y}");
                }
                var newTor = new Dictionary<int, Info>();
                var newTee = new Dictionary<int, Info>();
                foreach(int x in mentorReId)
                {
                    if(mentorD.ContainsKey(x) && !newTor.ContainsKey(x))
                    {
                        newTor.Add(x,mentorD.GetValueOrDefault(x));
                        await LogAsync($"mentorD contains mentor {x}");
                    }
                }
                foreach(int y in menteeReId)
                {
                    if(menteeD.ContainsKey(y) && !newTee.ContainsKey(y))
                    {
                        newTee.Add(y,menteeD.GetValueOrDefault(y));
                        await LogAsync($"menteeD contains mentee {y}");
                    }
                }

                decimal[,] newScores = await GetScoreMatrix(newTee, newTor);
                int[] newMatrix = FindAssignments(newScores);
                int[] mentorReIdArray = mentorReId.ToArray();
                int[] menteeReIdArray = menteeReId.ToArray();

                for(int x = 0; x < newMatrix.Length; x++)
                {
                    await LogAsync($"mentee: {x}, mentor: {newMatrix[x]}, score : {newScores[x,newMatrix[x]]}");
                }

                for(int y = 0; y < menteeReIdArray.Length; y++){
                    for(int z = 0; z < mentorReIdArray.Length; z++){
                        await LogAsync($"newScores at {y}, {z} is {newScores[y,z]}");
                    }
                }

                for(int x = 0; x < newMatrix.Length; x++)
                {
                    if(newScores[x,newMatrix[x]] > 0)
                    {
                        try{
                            int mentee_index = x;
                            int mentor_index = newMatrix[x];
                            int mentee_id = menteeReIdArray[mentee_index];
                            int mentor_id = mentorReIdArray[mentor_index];
                            await LogAsync($"mentee: {mentee_id}, mentor: {mentor_id}.");
                            Info tor = mentorD.GetValueOrDefault(mentor_id);
                            Info tee = menteeD.GetValueOrDefault(mentee_id);
                            double ss = (double)newScores[x,newMatrix[x]];
                            var pairingRecord = new PairingRecord() { MentorID = mentor_id, MentorName = tor.Name, MentorEmail = tor.Email, MenteeID = mentee_id, MenteeName = tee.Name, MenteeEmail = tee.Email, SimilarityScore = ss, IterationID = iteration};
                            _context.Add(pairingRecord);
    		                await _context.SaveChangesAsync();
                            index ++;
                            pairingRecords.Add(pairingRecord);
                        }
                        catch(Exception ex)
                        {
                            await LogAsync($"Error at rematching: {ex.Message} at mentee: {x} and mentor: {newMatrix[x]}");
                        }
                    }
                }
            }
            await LogAsync($"Index is: {index}");
            // Rematching finished
            // Writing all records into the pairingArray to return to CalculateScores() method
            string[,] pairingArray = new string[pairingRecords.Count+1, 9];
            pairingArray[0, 0] = "Id";
            pairingArray[0, 1] = "Mentor ID";
            pairingArray[0, 2] = "Mentor Name";
            pairingArray[0, 3] = "Mentor Email";
            pairingArray[0, 4] = "Mentee ID";
            pairingArray[0, 5] = "Mentee Name";
            pairingArray[0, 6] = "Mentee Email";
            pairingArray[0, 7] = "Similarity Score";
            pairingArray[0, 8] = "Iteration Number"; 
            for (int i = 0; i < pairingRecords.Count; i++)
            {
                var record = pairingRecords[i];
                pairingArray[i + 1, 0] = record.Id.ToString();
                pairingArray[i + 1, 1] = record.MentorID.ToString();
                pairingArray[i + 1, 2] = record.MentorName ?? string.Empty;
                pairingArray[i + 1, 3] = record.MentorEmail ?? string.Empty;
                pairingArray[i + 1, 4] = record.MenteeID.ToString();
                pairingArray[i + 1, 5] = record.MenteeName ?? string.Empty;
                pairingArray[i + 1, 6] = record.MenteeEmail ?? string.Empty;
                pairingArray[i + 1, 7] = record.SimilarityScore.ToString();
                pairingArray[i + 1, 8] = record.IterationID.ToString();
            }
            return pairingArray;
        }

        // Hungarian algorithm starts here
        public static int[] FindAssignments(decimal[,] scores)
        {
            if (scores == null)
            {
                throw new ArgumentNullException(nameof(scores));
            }
            int n = scores.GetLength(0);
            int m = scores.GetLength(1);

            // Converting scores from 2D decimal scores array to 2D double costs array
            double[,] costs = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    costs[i, j] = (double)(100 - scores[i, j]);
                }
            }
            var h = costs.GetLength(0);
            var w = costs.GetLength(1);
            bool rowsGreaterThanCols = h > w;
            
            // If the number of rows is greater than columns, transpose the cost matrix
            if(rowsGreaterThanCols)
            {
                var row = w;
                var col = h;
                var transposeCosts = new double [row, col];
                for(var i = 0; i < row; i++)
                {
                    for (var j = 0; j < col;j++)
                    {
                        transposeCosts[i, j] = costs[j, i];
                    }
                }
                costs = transposeCosts;
                h = row;
                w = col;
            }

            // Find and subtract the smallest value in each row from all elements in that row
            for(var i = 0; i < h; i++)
            {
                var min = double.MaxValue;
                for(var j = 0; j < w; j++)
                {
                    min = Math.Min(min, costs[i, j]);
                }

                for(var j = 0; j < w; j++)
                {
                    costs[i, j] -= min;
                }
            }

            var masks = new byte[h, w]; // Represent the state of the matrix elements: (0) - unmarker, (1) - starred, (2) - primed
            var rowsCovered = new bool[h]; // Boolean array indicating whether each row is covered
            var colsCovered = new bool[w]; // Boolean array indicating whether each column is covered

            // Cover each zero with a star (1) if it does not have any other starred zero in its row or column
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        masks[i, j] = 1;
                        rowsCovered[i] = true;
                        colsCovered[j] = true;
                    }
                }
            }
            ClearCovers(rowsCovered, colsCovered, w, h);
            var path = new Location[w * h]; // Keep track of the sequence of rows and columns that form the alternating path of primed and starred zeros
            var pathStart = default(Location);
            var step = 1;

            // Remaining steps of the Hungarian algorithm begin
            while(step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(masks, colsCovered, w, h),
                    2 => RunStep2(costs, masks, rowsCovered, colsCovered, w, h, ref pathStart),
                    3 => RunStep3(masks, rowsCovered, colsCovered, w, h, path, pathStart),
                    4 => RunStep4(costs, rowsCovered, colsCovered, w, h),
                    _ => step
                };
            }

            // Extract the assignments from the masks array
            var agentsTasks = new int[h]; // Store the final assignments of matching
            // Each index represents an agent (mentor during matching, mentee during rematching), and the value at that index represents the index of the assigned agent (mentee during matching, mentor during rematching).
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 1)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                    else
                    {
                        agentsTasks[i] = -1;
                    }
                }
            }

            // If the cost matrix was transposed, transpose the assignments back
            if(rowsGreaterThanCols)
            {
                var agentsTasksTranspose = new int[w]; // Temporary storage for assignments when the cost matrix was initially transposed
                for(var i = 0; i < w; i++)
                {
                    agentsTasksTranspose[i] = -1;
                }
            
                for(var j = 0; j < h; j++)
                {
                    agentsTasksTranspose[agentsTasks[j]] = j;
                }
                agentsTasks = agentsTasksTranspose;
            }
            return agentsTasks;
        }

        // Step 1: Cover all columns containing a starred zero and count the number of covered columns
        // If all columns are covered, the solution is optimal. Otherwise, proceed to Step 2
        private static int RunStep1(byte[,] masks, bool[] colsCovered, int w, int h)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 1)
                    {
                        colsCovered[j] = true;
                    }
                }
            }
            var colsCoveredCount = 0;
            for(var j = 0; j < w; j++)
            {
                if(colsCovered[j])
                {
                    colsCoveredCount++;
                }
            }
            if(colsCoveredCount == Math.Min(w, h))
            {
                return -1;
            }
            return 2;
        }

        // Step 2: Find a non-covered zero in the cost matrix and prime it
        // If no non-covered zero found, proceed to step 4
        // If there's a starred zero in the same row, cover the row and uncover the column containing the starred zero. Otherwise, proceed to Step 3.
        private static int RunStep2(double[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, ref Location pathStart)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            while(true)
            {
                var loc = FindZero(costs, rowsCovered, colsCovered, w, h);
                if(loc.row == -1)
                {
                    return 4;
                }
                masks[loc.row, loc.column] = 2;

                var starCol = FindStarInRow(masks, w, loc.row);
                if(starCol != -1)
                {
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    pathStart = loc;
                    return 3;
                }
            }
        }

        // Step 3: Construct an alternating path and convert primed and starred zeros
        // Convert primed zeros to starred zeros and starred zeros to unstarred
        private static int RunStep3(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, Location[] path, Location pathStart)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var pathIndex = 0;
            path[0] = pathStart;

            while(true)
            {
                var row = FindStarInColumn(masks, h, path[pathIndex].column);
                if(row == -1)
                {
                    break;
                }
                pathIndex++;
                path[pathIndex] = new Location(row, path[pathIndex - 1].column);
                var col = FindPrimeInRow(masks, w, path[pathIndex].row);
                pathIndex++;
                path[pathIndex] = new Location(path[pathIndex - 1].row, col);
            }
            ConvertPath(masks, path, pathIndex + 1);
            ClearCovers(rowsCovered, colsCovered, w, h);
            ClearPrimes(masks, w, h);
            return 1;
        }

        // Step 4: Adjust the cost matrix by the smallest uncovered value
        // Find the smallest uncovered value in the cost matrix
        // Subtract it from all uncovered elements and add (twice since it's both row and column) it to all covered elements
        private static int RunStep4(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var minValue = FindMinimum(costs, rowsCovered, colsCovered, w, h);
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(rowsCovered[i])
                    {
                        costs[i, j] += minValue;
                    }
                    if(!colsCovered[j])
                    {
                        costs[i, j] -= minValue;
                    }
                }
            }
            return 2;
        }

        // Find the smallest value in the uncovered elements of the cost matrix
        private static double FindMinimum(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var minValue = double.MaxValue;
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(!rowsCovered[i] && !colsCovered[j])
                    {
                        minValue = Math.Min(minValue, costs[i, j]);
                    }
                }
            }
            return minValue;
        }

        // Find the column index of a starred zero in the given row
        private static int FindStarInRow(byte[,] masks, int w, int row)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var j = 0; j < w; j++)
            {
                if(masks[row, j] == 1)
                {
                    return j;
                }
            }
            return -1;
        }

        // Find the row index of a starred zero in the given column
        private static int FindStarInColumn(byte[,] masks, int h, int col)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var i = 0; i < h; i++)
            {
                if(masks[i, col] == 1)
                {
                    return i;
                }
            }
            return -1;
        }

        // Find the column index of a primed zero in the given row
        private static int FindPrimeInRow(byte[,] masks, int w, int row)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var j = 0; j < w; j++)
            {
                if(masks[row, j] == 2)
                {
                    return j;
                }
            }
            return -1;
        }

        // Find a non-covered zero in the cost matrix and return its location (row index and column index)
        private static Location FindZero(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        return new Location(i, j);
                    }
                }
            }
            return new Location(-1, -1);
        }

        // Convert primed and starred zeros along the path
        private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            for(var i = 0; i < pathLength; i++)
            {
                masks[path[i].row, path[i].column] = masks[path[i].row, path[i].column] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[path[i].row, path[i].column]
                };
            }
        }

        // Clear all primed zeros
        private static void ClearPrimes(byte[,] masks, int w, int h)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 2)
                    {
                        masks[i, j] = 0;
                    }
                }
            }
        }

        // Clear all row and column covers
        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                rowsCovered[i] = false;
            }

            for(var j = 0; j < w; j++)
            {
                colsCovered[j] = false;
            }
        }

        private struct Location
        {
            internal readonly int row;
            internal readonly int column;
            internal Location(int row, int col)
            {
                this.row = row;
                this.column = col;
            }
        }
    }
}