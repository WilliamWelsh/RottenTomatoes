using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RottenTomatoes.Data
{
    // Data of a season of a show
    public class TVSeason : IEquatable<TVSeason>
    {
        public string Name { get; }
        public string URL { get; }

        public TVSeason(TVSeasonItem seasonChosen)
        {
            Name = seasonChosen.Name;
            URL = seasonChosen.URL;
        }

        public bool Equals(TVSeason other) => URL == other.URL;
        public override bool Equals(object obj) => Equals(obj as TVSeason);
        public override int GetHashCode() => 0; // idk
    }

    // For results on a series page
    public class TVSeasonItem : IEquatable<TVSeasonItem>
    {
        public string Name { get; }
        public string URL { get; }

        public TVSeasonItem(string name, string url)
        {
            Name = name;
            URL = url;
        }

        public bool Equals(TVSeasonItem other) => URL == other.URL;
        public override bool Equals(object obj) => Equals(obj as TVSeasonItem);
        public override int GetHashCode() => 0; // idk
    }
}
