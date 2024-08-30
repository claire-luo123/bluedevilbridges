# Blue Devil Bridges
## This repository contains code from the Blue Devil Bridges project that I worked on as software engineer intern through Duke University Office of Information Technology's Code+ Program during the summer of 2024. I'd like to acknowledge the other contributors of this project: Jason Okoro, Catherine Yang, Latham Hall, Eric Welborn

This guide provides necessary instructions on how to use the Blue Devil Bridges website and format the CSV file used in the mentor-mentee matching algorithm.

## Table of Contents

- [Sign In](#sign-in)
- [CSV File Format](#csv-file-format)
- [Start the Pairing Process](#start-the-pairing-process)
- [Viewing Results](#viewing-results)

## Sign In

Go to [bdb.dev.duke.edu](https://bdb.dev.duke.edu) and sign in with your NetID. Once you reach the home page, use the 'Upload Mentor Survey' and 'Upload Mentee Survey' buttons to upload the CSV files from the mentor and mentee Qualtrics surveys, respectively.

**IMPORTANT:** Before uploading, change all commas in the CSV files to semicolons. This ensures that the file structure does not impact the algorithm's ability to process multiple-select question responses. Note that both files must also have exactly 23 columns.

## CSV File Format

Each row in the CSV represents either a mentor or a mentee, with specific columns representing various attributes and preferences. The CSV file should have the following columns in the specified order:

1. **Name**: The name of the mentor/mentee.
2. **Email**: The email address of the mentor/mentee.
3. **School**: A semicolon-separated list of schools associated with the mentor/mentee.
4. **Year**: The year (e.g., freshman, sophomore, etc.) for the mentee and the year of graduation (e.g., Class of 2000) for the mentor.
5. **Ethnicity**: A semicolon-separated list of ethnicities associated with the mentor/mentee.
6. **Identity**: A semicolon-separated list of identities associated with the mentor/mentee.
7. **Major**: A semicolon-separated list of majors associated with the mentor/mentee.
8. **Minor**: A semicolon-separated list of minors associated with the mentor/mentee.
9. **Certificate**: A semicolon-separated list of certificates associated with the mentor/mentee.
10. **GradArea**: A semicolon-separated list of graduate areas associated with the mentor/mentee.
11. **Location**: A semicolon-separated list of preferred locations associated with the mentor/mentee.
12. **Industry**: A semicolon-separated list of industries associated with the mentor/mentee.
13. **Goal**: A semicolon-separated list of goals associated with the mentor/mentee.
14. **Hobby**: A semicolon-separated list of hobbies associated with the mentor/mentee.
15. **EthnicityWeighting**: An integer representing the weighting for ethnicity matching.
16. **IdentityWeighting**: An integer representing the weighting for identity matching.
17. **AcademicWeighting**: An integer representing the weighting for academic (major/minor/certificate) matching.
18. **GraduateWeighting**: An integer representing the weighting for graduate area matching.
19. **IndustryWeighting**: An integer representing the weighting for industry matching.
20. **LocationWeighting**: An integer representing the weighting for location matching.
21. **GoalWeighting**: An integer representing the weighting for goal matching.
22. **HobbyWeighting**: An integer representing the weighting for hobby matching.
23. **NumStudent**: An integer representing the number of students a mentor is willing to take on (For mentee CSV file, please leave all of them as 0).

### Notes on Specific Data Types

- **Semicolon-separated lists**: Columns such as School, Ethnicity, Identity, Major, Minor, Certificate, GradArea, Location, Industry, Goal, and Hobby should contain lists of values separated by semicolons (`;`).
- **Weighting Columns**: The weighting columns should contain integer values. These integers represent the importance of each attribute when calculating the match score.
- **NumStudent**: This column specifies how many mentees a mentor is willing to take on and should contain an integer value.

## Start the Pairing Process

Click 'Start Pairing' once both files are successfully uploaded. Note that you must leave your computer on while the algorithm is running. You can navigate away from the website tab, but it needs to be kept open.

Depending on the number of mentors and mentees, the algorithm may take a while to run. You'll see a loading symbol when it's running. Here is a breakdown of some timestamps we tested for reference:
- 100 mentors and mentees: 41 seconds
- 300 mentors and mentees: 30 minutes
- 1200 mentors and mentees: 5 hours

If the matching process is successfully completed, the words "Pairing done" will appear under the 'Start Pairing' button, and some initial results will automatically pop up on the right-hand side. Click "Download Pairings" to get a CSV file containing the match pairings.

If you see a different error message, then the matching was not successful. In this case, please click 'Rerun Algorithm', which will automatically refresh the page. Then, upload both files again.

## Viewing Results

Once the matches are successfully generated, the histogram displays the distribution of similarity scores of the matches made. The "At a Glance" table gives details about the number of mentors, mentees, and pairings, as well as the average, minimum, and maximum similarity scores.

Open the Statistics tab to view more detailed visualizations breaking down student years and alumni years. The Past Pairings tab will allow you to view previous iterations of the algorithm. Here, you can download past matches and their corresponding statistics. You'll see a past pairing entry titled "Placeholder", which is there to ensure the working functionality of the website. Please do not remove this entry. With all other past pairings, you can rename, delete, and view statistics for the entries.

---

This guide should help you effectively use the Blue Devil Bridges website and ensure the CSV files are properly formatted for the mentor-mentee matching algorithm. If you have any questions or encounter issues, please contact the support team.
