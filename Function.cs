using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using AlexaAPI;
using AlexaAPI.Request;
using AlexaAPI.Response;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MoviePicker
{

    public static class Globals
    {
        static Globals()
        {
            genre = "Default";
            year = "Default";
            director = "Default";
        }
        public static string genre { get; set; }
        public static string year { get; set; }
        public static string director { get; set; }

        public static void Reset()
        {
            genre = "Default";
            year = "Default";
            director = "Default";
        }
    }

    public class Function
    {
        private SkillResponse response = null;
        private ILambdaContext context = null;


        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext ctx)
        {
            context = ctx;
            try
            {
                response = new SkillResponse();
                response.Response = new ResponseBody();
                response.Response.ShouldEndSession = false;
                response.Version = AlexaConstants.AlexaVersion;

                if (input.Request.Type.Equals(AlexaConstants.LaunchRequest))
                {



                    ProcessLaunchRequest(response.Response);

                }
                else
                {
                    if (input.Request.Type.Equals(AlexaConstants.IntentRequest))
                    {


                       if (IsDialogIntentRequest(input))
                       {
                            if (!IsDialogSequenceComplete(input))
                            { 
                                CreateDelegateResponse();
                                return response;
                            }
                       }

                       if (!ProcessDialogRequest(input, response))
                       {
                           response.Response.OutputSpeech = ProcessIntentRequest(input);
                       }
                    }
                }
                
                return response;
            }
            catch (Exception ex)
            {
                
            }
            return null; 
        }


        private void ProcessLaunchRequest(ResponseBody response)
        {

                IOutputSpeech innerResponse = new SsmlOutputSpeech();
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate("Lets get you something to watch! Tell me a genre,year or director to help narrow it down. Once your done say spin the movie wheel to get your result");
            Globals.Reset();
            response.OutputSpeech = innerResponse;
                IOutputSpeech prompt = new PlainTextOutputSpeech();
                (prompt as PlainTextOutputSpeech).Text = "";
                response.Reprompt = new Reprompt()
                {
                    OutputSpeech = prompt
                };
            
        }


        private bool IsDialogIntentRequest(SkillRequest input)
        {
            if (string.IsNullOrEmpty(input.Request.DialogState))
                return false;
            return true;
        }


        private bool IsDialogSequenceComplete(SkillRequest input)
        {
            if (input.Request.DialogState.Equals(AlexaConstants.DialogStarted)
               || input.Request.DialogState.Equals(AlexaConstants.DialogInProgress))
            { 
                return false ;
            }
            else
            {
                if (input.Request.DialogState.Equals(AlexaConstants.DialogCompleted))
                {
                    return true;
                }
            }
            return false;
        }


        private bool ProcessDialogRequest(SkillRequest input, SkillResponse response)
        {
            var intentRequest = input.Request;
            string speech_message = string.Empty;
            bool processed = false;

            switch (intentRequest.Intent.Name)
            {
                case "GetWeather":

                    if (!string.IsNullOrEmpty(speech_message))
                    {
                        response.Response.OutputSpeech = new SsmlOutputSpeech();
                        (response.Response.OutputSpeech as SsmlOutputSpeech).Ssml = SsmlDecorate(speech_message);
                    }
                    processed = true;
                    break;

            }

            return processed;
        }


        private string SsmlDecorate(string speech)
        {
            return "<speak>" + speech + "</speak>";
        }


        private IOutputSpeech ProcessIntentRequest(SkillRequest input)
        {
            var intentRequest = input.Request;
            IOutputSpeech innerResponse = new PlainTextOutputSpeech();
            
            switch (intentRequest.Intent.Name)
            {
                case "GetGenre":
                    Slot genrelot;

                    input.Request.Intent.Slots.TryGetValue("genre", out genrelot);
                    String genre = genrelot.Value;
                    innerResponse = new SsmlOutputSpeech();
                    if(Globals.genre.Equals("Default"))
                    {
                        Globals.genre = genre.ToString();
                        (innerResponse as SsmlOutputSpeech).Ssml = "We will find a " + Globals.genre + " movie to watch";
                    }
                    else
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "You already picked the " + Globals.genre + " genre";
                    }

                    break;
                case "GetYear":
                    Slot yearlot;

                    input.Request.Intent.Slots.TryGetValue("year", out yearlot);
                    String year = yearlot.Value.ToString();
                    innerResponse = new SsmlOutputSpeech();


                    if (Globals.year.Equals("Default"))
                    {
                        Globals.year = year.ToString();
                        (innerResponse as SsmlOutputSpeech).Ssml = "We will find a movie from the year " + Globals.year;
                    }
                    else
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "You already told me the year " + Globals.year;
                    }

                    break;
                case "GetDirector":
                    Slot directorlot;

                    input.Request.Intent.Slots.TryGetValue("director", out directorlot);
                    String director = directorlot.Value.ToString();
                    innerResponse = new SsmlOutputSpeech();


                    if (Globals.director.Equals("Default"))
                    {
                        Globals.director = director.ToString();
                        (innerResponse as SsmlOutputSpeech).Ssml =   Globals.director + " is a good choice! I will use that.";
                    }
                    else
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "You already told me " + Globals.director;
                    }

                    break;
                case "GetSpin":

                    innerResponse = new SsmlOutputSpeech();
                    Random rnd = new Random();
                    List<Movie> a = LoadMovies();

                    List<Movie> genres = new List<Movie>();
                    List<Movie> years = new List<Movie>();
                    List<Movie> directors = new List<Movie>();

                    if (!(Globals.genre.Equals("Default")))
                    {
                        foreach (Movie b in a)
                        {
                            if (b.genre.ToUpper() == Globals.genre.ToUpper())
                            {
                                genres.Add(b);
                            }
                        }



                        //(innerResponse as SsmlOutputSpeech).Ssml = "Your movie chosen for you is  " + c[rnd.Next(0, c.Count)].title + ". I hope its a good one!";


                    }

                    if (!(Globals.year.Equals("Default")))
                    {
                        foreach (Movie b in a)
                        {
                            if (b.year.ToUpper() == Globals.year.ToUpper())
                            {
                                years.Add(b);
                            }
                        }

                        //(innerResponse as SsmlOutputSpeech).Ssml = "Your movie chosen for you is  " + c[rnd.Next(0, c.Count)].title + ". I hope its a good one!";

                    }
                    if (!Globals.director.Equals("Default"))
                    {
                        foreach (Movie b in a)
                        {
                            if (b.director.ToUpper() == Globals.director.ToUpper())
                            {
                                directors.Add(b);
                            }
                        }

                         //(innerResponse as SsmlOutputSpeech).Ssml = "Your movie chosen for you is  " + c[rnd.Next(0, c.Count)].title + ". I hope its a good one!";

                    }

                    if((Globals.year.Equals("Default")) && (Globals.genre.Equals("Default")) && (Globals.director.Equals("Default")))
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "Your wild card movie is  " + a[rnd.Next(0, a.Count)].title + ". I hope its a good one!";
                        response.Response.ShouldEndSession = true;
                        break;
                    }


                    genres.AddRange(directors);
                    genres.AddRange(years);
                    genres = genres.Distinct().ToList<Movie>();

                    if(genres.Count != 0)
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "Your movie chosen for you is  " + genres[rnd.Next(0, genres.Count)].title + ". I hope its a good one!";
                    }
                    else
                    {
                        (innerResponse as SsmlOutputSpeech).Ssml = "I could not find any movies with your prefrences! You could watch " + a[rnd.Next(0, a.Count)].title + " instead";
                    }
                     



                    response.Response.ShouldEndSession = true;


                    break;
                case AlexaConstants.CancelIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = "Your intent was canceled";
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.StopIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = "";
                    response.Response.ShouldEndSession = true;                    
                    break;

                case AlexaConstants.HelpIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = "Tell me what genere you like or a year for movies that was good."; 
                    break;

                default:
                    (innerResponse as PlainTextOutputSpeech).Text = ""; 
                    break;
            }
            if (innerResponse.Type == AlexaConstants.SSMLSpeech)
            {
                BuildCard("WSIW", (innerResponse as SsmlOutputSpeech).Ssml);
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate((innerResponse as SsmlOutputSpeech).Ssml);
            }  
            return innerResponse;
        }

        /// <summary>
        /// Build a simple card, setting its title and content field 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns>void</returns>
        private void BuildCard(string title, string output)
        {
            if (!string.IsNullOrEmpty(output))
            {                
                output = Regex.Replace(output, @"<.*?>", "");
                response.Response.Card = new SimpleCard()
                {
                    Title = title,
                    Content = output,
                };  
            }
        }

     


        private void CreateDelegateResponse()
        {
            DialogDirective dld = new DialogDirective()
            {
                Type = AlexaConstants.DialogDelegate
            };
            response.Response.Directives.Add(dld);
        }

        public List<Movie> LoadMovies()
        {


            List<Movie> movies = new List<Movie>();
            movies.Add(new Movie("The Shawshank Redemption", "Drama", "1994", "Frank Darabont"));
            movies.Add(new Movie("The Godfather", "Crime", "1972", "Francis Ford Coppola"));
            movies.Add(new Movie("The Godfather: Part II", "Crime", "1974", "Francis Ford Coppola"));
            movies.Add(new Movie("The Dark Knight", "Action", "2008", "Christopher Nolan"));
            movies.Add(new Movie("12 Angry Men", "Crime", "1957", "Sidney Lumet"));
            movies.Add(new Movie("Schindler's List", "Biography", "1993", "Steven Spielberg"));
            movies.Add(new Movie("The Lord of the Rings: The Return of the King", "Action", "2003", "Peter Jackson"));
            movies.Add(new Movie("Pulp Fiction", "Crime", "1994", "Quentin Tarantino"));
            movies.Add(new Movie("The Good, the Bad and the Ugly", "Western", "1966", "Sergio Leone"));
            movies.Add(new Movie("Fight Club", "Drama", "1999", "David Fincher"));
            movies.Add(new Movie("The Lord of the Rings: The Fellowship of the Ring", "Adventure", "2001", "Peter Jackson"));
            movies.Add(new Movie("Forrest Gump", "Drama", "1994", "Robert Zemeckis"));
            movies.Add(new Movie("Star Wars: Episode V - The Empire Strikes Back", "Action", "1980", "Irvin Kershner"));
            movies.Add(new Movie("Inception", "Action", "2010", "Christopher Nolan"));
            movies.Add(new Movie("The Lord of the Rings: The Two Towers", "Adventure", "2002", "Peter Jackson"));
            movies.Add(new Movie("One Flew Over the Cuckoo's Nest", "Drama", "1975", "Milos Forman"));
            movies.Add(new Movie("Goodfellas", "Crime", "1990", "Martin Scorsese"));
            movies.Add(new Movie("The Matrix", "Action", "1999", "Lana Wachowski"));
            movies.Add(new Movie("Seven Samurai", "Adventure", "1954", "Akira Kurosawa"));
            movies.Add(new Movie("City of God", "Crime", "2002", "Fernando Meirelles"));
            movies.Add(new Movie("Se7en", "Crime", "1995", "David Fincher"));
            movies.Add(new Movie("Star Wars: Episode IV - A New Hope", "Action", "1977", "George Lucas"));
            movies.Add(new Movie("The Silence of the Lambs", "Crime", "1991", "Jonathan Demme"));
            movies.Add(new Movie("It's a Wonderful Life", "Drama", "1946", "Frank Capra"));
            movies.Add(new Movie("Life Is Beautiful", "Comedy", "1997", "Roberto Benigni"));
            movies.Add(new Movie("The Usual Suspects", "Crime", "1995", "Bryan Singer"));
            movies.Add(new Movie("Spirited Away", "Animation", "2001", "Hayao Miyazaki"));
            movies.Add(new Movie("Saving Private Ryan", "Drama", "1998", "Steven Spielberg"));
            movies.Add(new Movie("Léon: The Professional", "Crime", "1994", "Luc Besson"));
            movies.Add(new Movie("The Green Mile", "Crime", "1999", "Frank Darabont"));
            movies.Add(new Movie("Interstellar", "Adventure", "2014", "Christopher Nolan"));
            movies.Add(new Movie("American History X", "Crime", "1998", "Tony Kaye"));
            movies.Add(new Movie("Psycho", "Horror", "1960", "Alfred Hitchcock"));
            movies.Add(new Movie("City Lights", "Comedy", "1931", "Charles Chaplin"));
            movies.Add(new Movie("Once Upon a Time in the West", "Western", "1968", "Sergio Leone"));
            movies.Add(new Movie("Casablanca", "Drama", "1942", "Michael Curtiz"));
            movies.Add(new Movie("Modern Times", "Comedy", "1936", "Charles Chaplin"));
            movies.Add(new Movie("The Intouchables", "Biography", "2011", "Olivier Nakache"));
            movies.Add(new Movie("The Pianist", "Biography", "2002", "Roman Polanski"));
            movies.Add(new Movie("The Departed", "Crime", "2006", "Martin Scorsese"));
            movies.Add(new Movie("Terminator 2", "Action", "1991", "James Cameron"));
            movies.Add(new Movie("Back to the Future", "Adventure", "1985", "Robert Zemeckis"));
            movies.Add(new Movie("Whiplash", "Drama", "2014", "Damien Chazelle"));
            movies.Add(new Movie("Rear Window", "Mystery", "1954", "Alfred Hitchcock"));
            movies.Add(new Movie("Raiders of the Lost Ark", "Action", "1981", "Steven Spielberg"));
            movies.Add(new Movie("Gladiator", "Action", "2000", "Ridley Scott"));
            movies.Add(new Movie("The Lion King", "Animation", "1994", "Roger Allers"));
            movies.Add(new Movie("The Prestige", "Drama", "2006", "Christopher Nolan"));
            movies.Add(new Movie("Avengers: Infinity War", "Action", "2018", "Anthony Russo"));
            movies.Add(new Movie("Memento", "Mystery", "2000", "Christopher Nolan"));
            movies.Add(new Movie("Apocalypse Now", "Drama", "1979", "Francis Ford Coppola"));
            movies.Add(new Movie("Alien", "Horror", "1979", "Ridley Scott"));
            movies.Add(new Movie("The Great Dictator", "Comedy", "1940", "Charles Chaplin"));
            movies.Add(new Movie("Cinema Paradiso", "Drama", "1988", "Giuseppe Tornatore"));
            movies.Add(new Movie("Grave of the Fireflies", "Animation", "1988", "Isao Takahata"));
            movies.Add(new Movie("Sunset Boulevard", "Drama", "1950", "Billy Wilder"));
            movies.Add(new Movie("The Lives of Others", "Drama", "2006", "Florian Henckel von Donnersmarck"));
            movies.Add(new Movie("Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb", "Comedy", "1964", "Stanley Kubrick"));
            movies.Add(new Movie("Paths of Glory", "Drama", "1957", "Stanley Kubrick"));
            movies.Add(new Movie("The Shining", "Drama", "1980", "Stanley Kubrick"));
            movies.Add(new Movie("Django Unchained", "Drama", "2012", "Quentin Tarantino"));
            movies.Add(new Movie("WALL·E", "Animation", "2008", "Andrew Stanton"));
            movies.Add(new Movie("Princess Mononoke", "Animation", "1997", "Hayao Miyazaki"));
            movies.Add(new Movie("Witness for the Prosecution", "Crime", "1957", "Billy Wilder"));
            movies.Add(new Movie("American Beauty", "Drama", "1999", "Sam Mendes"));
            movies.Add(new Movie("Coco", "Animation", "2017", "Lee Unkrich"));
            movies.Add(new Movie("The Dark Knight Rises", "Action", "2012", "Christopher Nolan"));
            movies.Add(new Movie("Oldboy", "Action", "2003", "Chan-wook Park"));
            movies.Add(new Movie("Aliens", "Action", "1986", "James Cameron"));
            movies.Add(new Movie("Once Upon a Time in America", "Crime", "1984", "Sergio Leone"));
            movies.Add(new Movie("Das Boot", "Adventure", "1981", "Wolfgang Petersen"));
            movies.Add(new Movie("Citizen Kane", "Drama", "1941", "Orson Welles"));
            movies.Add(new Movie("Braveheart", "Biography", "1995", "Mel Gibson"));
            movies.Add(new Movie("Vertigo", "Mystery", "1958", "Alfred Hitchcock"));
            movies.Add(new Movie("North by Northwest", "Adventure", "1959", "Alfred Hitchcock"));
            movies.Add(new Movie("Reservoir Dogs", "Crime", "1992", "Quentin Tarantino"));
            movies.Add(new Movie("Your Name.", "Animation", "2016", "Makoto Shinkai"));
            movies.Add(new Movie("Star Wars: Episode VI - Return of the Jedi", "Action", "1983", "Richard Marquand"));
            movies.Add(new Movie("M", "Crime", "1931", "Fritz Lang"));
            movies.Add(new Movie("Dangal", "Action", "2016", "Nitesh Tiwari"));
            movies.Add(new Movie("Requiem for a Dream", "Drama", "2000", "Darren Aronofsky"));
            movies.Add(new Movie("Amadeus", "Biography", "1984", "Milos Forman"));
            movies.Add(new Movie("Like Stars on Earth", "Drama", "2007", "Aamir Khan"));
            movies.Add(new Movie("Lawrence of Arabia", "Adventure", "1962", "David Lean"));
            movies.Add(new Movie("Eternal Sunshine of the Spotless Mind", "Drama", "2004", "Michel Gondry"));
            movies.Add(new Movie("A Clockwork Orange", "Crime", "1971", "Stanley Kubrick"));
            movies.Add(new Movie("Amélie", "Comedy", "2001", "Jean-Pierre Jeunet"));
            movies.Add(new Movie("Double Indemnity", "Crime", "1944", "Billy Wilder"));
            movies.Add(new Movie("3 Idiots", "Comedy", "2009", "Rajkumar Hirani"));
            movies.Add(new Movie("2001: A Space Odyssey", "Adventure", "1968", "Stanley Kubrick"));
            movies.Add(new Movie("Toy Story", "Animation", "1995", "John Lasseter"));
            movies.Add(new Movie("Taxi Driver", "Crime", "1976", "Martin Scorsese"));
            movies.Add(new Movie("Singin' in the Rain", "Comedy", "1952", "Stanley Donen"));
            movies.Add(new Movie("Full Metal Jacket", "Drama", "1987", "Stanley Kubrick"));
            movies.Add(new Movie("Inglourious Basterds", "Adventure", "2009", "Quentin Tarantino"));
            movies.Add(new Movie("To Kill a Mockingbird", "Crime", "1962", "Robert Mulligan"));
            movies.Add(new Movie("Bicycle Thieves", "Drama", "1948", "Vittorio De Sica"));
            movies.Add(new Movie("The Kid", "Comedy", "1921", "Charles Chaplin"));
            movies.Add(new Movie("The Sting", "Comedy", "1973", "George Roy Hill"));
            movies.Add(new Movie("Toy Story 3", "Animation", "2010", "Lee Unkrich"));
            movies.Add(new Movie("Good Will Hunting", "Drama", "1997", "Gus Van Sant"));
            movies.Add(new Movie("The Hunt", "Drama", "2012", "Thomas Vinterberg"));
            movies.Add(new Movie("Snatch", "Comedy", "2000", "Guy Ritchie"));
            movies.Add(new Movie("Monty Python and the Holy Grail", "Adventure", "1975", "Terry Gilliam"));
            movies.Add(new Movie("Scarface", "Crime", "1983", "Brian De Palma"));
            movies.Add(new Movie("For a Few Dollars More", "Western", "1965", "Sergio Leone"));
            movies.Add(new Movie("The Apartment", "Comedy", "1960", "Billy Wilder"));
            movies.Add(new Movie("L.A. Confidential", "Crime", "1997", "Curtis Hanson"));
            movies.Add(new Movie("Metropolis", "Drama", "1927", "Fritz Lang"));
            movies.Add(new Movie("A Separation", "Crime", "2011", "Asghar Farhadi"));
            movies.Add(new Movie("Indiana Jones and the Last Crusade", "Action", "1989", "Steven Spielberg"));
            movies.Add(new Movie("Rashomon", "Crime", "1950", "Akira Kurosawa"));
            movies.Add(new Movie("Up", "Animation", "2009", "Pete Docter"));
            movies.Add(new Movie("All About Eve", "Drama", "1950", "Joseph L. Mankiewicz"));
            movies.Add(new Movie("Yojimbo", "Action", "1961", "Akira Kurosawa"));
            movies.Add(new Movie("Batman Begins", "Action", "2005", "Christopher Nolan"));
            movies.Add(new Movie("Some Like It Hot", "Comedy", "1959", "Billy Wilder"));
            movies.Add(new Movie("Unforgiven", "Drama", "1992", "Clint Eastwood"));
            movies.Add(new Movie("Downfall", "Biography", "2004", "Oliver Hirschbiegel"));
            movies.Add(new Movie("The Treasure of the Sierra Madre", "Adventure", "1948", "John Huston"));
            movies.Add(new Movie("Die Hard", "Action", "1988", "John McTiernan"));
            movies.Add(new Movie("Heat", "Crime", "1995", "Michael Mann"));
            movies.Add(new Movie("Raging Bull", "Biography", "1980", "Martin Scorsese"));
            movies.Add(new Movie("Ikiru", "Drama", "1952", "Akira Kurosawa"));
            movies.Add(new Movie("Incendies", "Drama", "2010", "Denis Villeneuve"));
            movies.Add(new Movie("The Great Escape", "Adventure", "1963", "John Sturges"));
            movies.Add(new Movie("My Father and My Son", "Drama", "2005", "Çagan Irmak"));
            movies.Add(new Movie("Children of Heaven", "Drama", "1997", "Majid Majidi"));
            movies.Add(new Movie("Pan's Labyrinth", "Drama", "2006", "Guillermo del Toro"));
            movies.Add(new Movie("The Third Man", "Film-Noir", "1949", "Carol Reed"));
            movies.Add(new Movie("Chinatown", "Drama", "1974", "Roman Polanski"));
            movies.Add(new Movie("My Neighbor Totoro", "Animation", "1988", "Hayao Miyazaki"));
            movies.Add(new Movie("Ran", "Action", "1985", "Akira Kurosawa"));
            movies.Add(new Movie("Howl's Moving Castle", "Animation", "2004", "Hayao Miyazaki"));
            movies.Add(new Movie("The Secret in Their Eyes", "Drama", "2009", "Juan José Campanella"));
            movies.Add(new Movie("The Gold Rush", "Adventure", "1925", "Charles Chaplin"));
            movies.Add(new Movie("The Bridge on the River Kwai", "Adventure", "1957", "David Lean"));
            movies.Add(new Movie("Three Billboards Outside Ebbing, Missouri", "Comedy", "2017", "Martin McDonagh"));
            movies.Add(new Movie("On the Waterfront", "Crime", "1954", "Elia Kazan"));
            movies.Add(new Movie("A Beautiful Mind", "Biography", "2001", "Ron Howard"));
            movies.Add(new Movie("Lock, Stock and Two Smoking Barrels", "Comedy", "1998", "Guy Ritchie"));
            movies.Add(new Movie("Casino", "Crime", "1995", "Martin Scorsese"));
            movies.Add(new Movie("The Seventh Seal", "Drama", "1957", "Ingmar Bergman"));
            movies.Add(new Movie("Inside Out", "Animation", "2015", "Pete Docter"));
            movies.Add(new Movie("Room", "Drama", "2015", "Lenny Abrahamson"));
            movies.Add(new Movie("The Elephant Man", "Biography", "1980", "David Lynch"));
            movies.Add(new Movie("Mr. Smith Goes to Washington", "Comedy", "1939", "Frank Capra"));
            movies.Add(new Movie("The Wolf of Wall Street", "Biography", "2013", "Martin Scorsese"));
            movies.Add(new Movie("V for Vendetta", "Action", "2005", "James McTeigue"));
            movies.Add(new Movie("Warrior", "Drama", "2011", "Gavin O'Connor"));
            movies.Add(new Movie("Blade Runner", "Sci-Fi", "1982", "Ridley Scott"));
            movies.Add(new Movie("The General", "Action", "1926", "Clyde Bruckman"));
            movies.Add(new Movie("Wild Strawberries", "Drama", "1957", "Ingmar Bergman"));
            movies.Add(new Movie("Dial M for Murder", "Crime", "1954", "Alfred Hitchcock"));
            movies.Add(new Movie("Trainspotting", "Drama", "1996", "Danny Boyle"));
            movies.Add(new Movie("No Country for Old Men", "Crime", "2007", "Ethan Coen"));
            movies.Add(new Movie("There Will Be Blood", "Drama", "2007", "Paul Thomas Anderson"));
            movies.Add(new Movie("The Sixth Sense", "Drama", "1999", "M. Night Shyamalan"));
            movies.Add(new Movie("Gone with the Wind", "Drama", "1939", "Victor Fleming"));
            movies.Add(new Movie("Fargo", "Crime", "1996", "Joel Coen"));
            movies.Add(new Movie("The Thing", "Horror", "1982", "John Carpenter"));
            movies.Add(new Movie("Gran Torino", "Drama", "2008", "Clint Eastwood"));
            movies.Add(new Movie("The Deer Hunter", "Drama", "1978", "Michael Cimino"));
            movies.Add(new Movie("Finding Nemo", "Animation", "2003", "Andrew Stanton"));
            movies.Add(new Movie("Sherlock Jr.", "Action", "1924", "Buster Keaton"));
            movies.Add(new Movie("Come and See", "Drama", "1985", "Elem Klimov"));
            movies.Add(new Movie("The Big Lebowski", "Comedy", "1998", "Joel Coen"));
            movies.Add(new Movie("Kill Bill: Vol. 1", "Action", "2003", "Quentin Tarantino"));
            movies.Add(new Movie("Shutter Island", "Mystery", "2010", "Martin Scorsese"));
            movies.Add(new Movie("Cool Hand Luke", "Crime", "1967", "Stuart Rosenberg"));
            movies.Add(new Movie("Rebecca", "Drama", "1940", "Alfred Hitchcock"));
            movies.Add(new Movie("Tokyo Story", "Drama", "1953", "Yasujirô Ozu"));
            movies.Add(new Movie("Hacksaw Ridge", "Biography", "2016", "Mel Gibson"));
            movies.Add(new Movie("Mary and Max", "Animation", "2009", "Adam Elliot"));
            movies.Add(new Movie("Sunrise", "Drama", "1927", "F.W. Murnau"));
            movies.Add(new Movie("A Star Is Born", "Drama", "2018", "Bradley Cooper"));
            movies.Add(new Movie("How to Train Your Dragon", "Animation", "2010", "Dean DeBlois"));
            movies.Add(new Movie("Gone Girl", "Crime", "2014", "David Fincher"));
            movies.Add(new Movie("Wild Tales", "Comedy", "2014", "Damián Szifron"));
            movies.Add(new Movie("Jurassic Park", "Adventure", "1993", "Steven Spielberg"));
            movies.Add(new Movie("Into the Wild", "Adventure", "2007", "Sean Penn"));
            movies.Add(new Movie("Life of Brian", "Comedy", "1979", "Terry Jones"));
            movies.Add(new Movie("The Bandit", "Crime", "1996", "Yavuz Turgul"));
            movies.Add(new Movie("It Happened One Night", "Comedy", "1934", "Frank Capra"));
            movies.Add(new Movie("In the Name of the Father", "Biography", "1993", "Jim Sheridan"));
            movies.Add(new Movie("Platoon", "Drama", "1986", "Oliver Stone"));
            movies.Add(new Movie("The Grand Budapest Hotel", "Adventure", "2014", "Wes Anderson"));
            movies.Add(new Movie("Stand by Me", "Adventure", "1986", "Rob Reiner"));
            movies.Add(new Movie("Network", "Drama", "1976", "Sidney Lumet"));
            movies.Add(new Movie("The Truman Show", "Comedy", "1998", "Peter Weir"));
            movies.Add(new Movie("Stalker", "Drama", "1979", "Andrei Tarkovsky"));
            movies.Add(new Movie("Hotel Rwanda", "Biography", "2004", "Terry George"));
            movies.Add(new Movie("Andrei Rublev", "Biography", "1966", "Andrei Tarkovsky"));
            movies.Add(new Movie("Ben-Hur", "Adventure", "1959", "William Wyler"));
            movies.Add(new Movie("Persona", "Drama", "1966", "Ingmar Bergman"));
            movies.Add(new Movie("Memories of Murder", "Action", "2003", "Joon-ho Bong"));
            movies.Add(new Movie("12 Years a Slave", "Biography", "2013", "Steve McQueen"));
            movies.Add(new Movie("The Wages of Fear", "Adventure", "1953", "Henri-Georges Clouzot"));
            movies.Add(new Movie("Million Dollar Baby", "Drama", "2004", "Clint Eastwood"));
            movies.Add(new Movie("Rang De Basanti", "Comedy", "2006", "Rakeysh Omprakash Mehra"));
            movies.Add(new Movie("Rush", "Action", "2013", "Ron Howard"));
            movies.Add(new Movie("The Passion of Joan of Arc", "Biography", "1928", "Carl Theodor Dreyer"));
            movies.Add(new Movie("Mad Max: Fury Road", "Action", "2015", "George Miller"));
            movies.Add(new Movie("Before Sunrise", "Drama", "1995", "Richard Linklater"));
            movies.Add(new Movie("The 400 Blows", "Crime", "1959", "François Truffaut"));
            movies.Add(new Movie("Spotlight", "Crime", "2015", "Tom McCarthy"));
            movies.Add(new Movie("Logan", "Action", "2017", "James Mangold"));
            movies.Add(new Movie("Amores Perros", "Drama", "2000", "Alejandro G. Iñárritu"));
            movies.Add(new Movie("Prisoners", "Crime", "2013", "Denis Villeneuve"));
            movies.Add(new Movie("The Princess Bride", "Adventure", "1987", "Rob Reiner"));
            movies.Add(new Movie("Nausicaä of the Valley of the Wind", "Animation", "1984", "Hayao Miyazaki"));
            movies.Add(new Movie("Butch Cassidy and the Sundance Kid", "Biography", "1969", "George Roy Hill"));
            movies.Add(new Movie("Catch Me If You Can", "Biography", "2002", "Steven Spielberg"));
            movies.Add(new Movie("Harry Potter and the Deathly Hallows: Part 2", "Adventure", "2011", "David Yates"));
            movies.Add(new Movie("Rocky", "Drama", "1976", "John G. Avildsen"));
            movies.Add(new Movie("Barry Lyndon", "Adventure", "1975", "Stanley Kubrick"));
            movies.Add(new Movie("Monsters, Inc.", "Animation", "2001", "Pete Docter"));
            movies.Add(new Movie("The Maltese Falcon", "Film-Noir", "1941", "John Huston"));
            movies.Add(new Movie("The Grapes of Wrath", "Drama", "1940", "John Ford"));
            movies.Add(new Movie("Donnie Darko", "Drama", "2001", "Richard Kelly"));
            movies.Add(new Movie("Diabolique", "Crime", "1955", "Henri-Georges Clouzot"));
            movies.Add(new Movie("The Terminator", "Action", "1984", "James Cameron"));
            movies.Add(new Movie("Gandhi", "Biography", "1982", "Richard Attenborough"));
            movies.Add(new Movie("Dead Poets Society", "Comedy", "1989", "Peter Weir"));
            movies.Add(new Movie("Blade Runner 2049", "Drama", "2017", "Denis Villeneuve"));
            movies.Add(new Movie("La La Land", "Comedy", "2016", "Damien Chazelle"));
            movies.Add(new Movie("The Nights of Cabiria", "Drama", "1957", "Federico Fellini"));
            movies.Add(new Movie("La Haine", "Crime", "1995", "Mathieu Kassovitz"));
            movies.Add(new Movie("Groundhog Day", "Comedy", "1993", "Harold Ramis"));
            movies.Add(new Movie("The Wizard of Oz", "Adventure", "1939", "Victor Fleming"));
            movies.Add(new Movie("Jaws", "Adventure", "1975", "Steven Spielberg"));
            movies.Add(new Movie("The Help", "Drama", "2011", "Tate Taylor"));
            movies.Add(new Movie("Paper Moon", "Comedy", "1973", "Peter Bogdanovich"));
            movies.Add(new Movie("In the Mood for Love", "Drama", "2000", "Kar-Wai Wong"));
            movies.Add(new Movie("Gangs of Wasseypur", "Action", "2012", "Anurag Kashyap"));
            movies.Add(new Movie("Before Sunset", "Drama", "2004", "Richard Linklater"));
            movies.Add(new Movie("A Wednesday", "Crime", "2008", "Neeraj Pandey"));
            movies.Add(new Movie("Paris, Texas", "Drama", "1984", "Wim Wenders"));
            movies.Add(new Movie("The Bourne Ultimatum", "Action", "2007", "Paul Greengrass"));
            movies.Add(new Movie("The Handmaiden", "Crime", "2016", "Chan-wook Park"));
            movies.Add(new Movie("Guardians of the Galaxy", "Action", "2014", "James Gunn"));
            movies.Add(new Movie("Tangerines", "Drama", "2013", "Zaza Urushadze"));
            movies.Add(new Movie("Castle in the Sky", "Animation", "1986", "Hayao Miyazaki"));
            movies.Add(new Movie("Winter Sleep", "Drama", "2014", "Nuri Bilge Ceylan"));
            movies.Add(new Movie("PK", "Comedy", "2014", "Rajkumar Hirani"));
            movies.Add(new Movie("Pirates of the Caribbean: The Curse of the Black Pearl", "Action", "2003", "Gore Verbinski"));
            movies.Add(new Movie("Drishyam", "Crime", "2015", "Nishikant Kamat"));
            movies.Add(new Movie("Dog Day Afternoon", "Biography", "1975", "Sidney Lumet"));
            return movies;
        }



    }
}
