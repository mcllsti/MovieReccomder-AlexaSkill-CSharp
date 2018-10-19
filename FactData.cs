using System.Collections.Generic;

namespace MoviePicker
{
    public class Movie
    {

        public Movie(string Title,string Genre,string Year,string Director)
        {
            title = Title;
            genre = Genre;
            year = Year;
            director = Director;
        }


        public string title { get; set; }
        public string genre { get; set; }
        public string year { get; set; }
        public string director { get; set; }



    }



}




