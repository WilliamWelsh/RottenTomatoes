namespace RottenTomatoes
{
    public class SearchResultItem
    {
        public Movie Movie { get; }

        public SearchResultItem(Movie Movie)
        {
            this.Movie = Movie;
        }
    }
}