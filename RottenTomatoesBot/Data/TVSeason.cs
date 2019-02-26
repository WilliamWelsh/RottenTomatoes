using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RottenTomatoes.Data
{
    public class TVSeason : IEquatable<TVSeason>
    {
        public string Name { get; }
        public string URL { get; }

        public TVSeason(string name, string url)
        {
            Name = name;
            URL = url;
        }

        public bool Equals(TVSeason other) => URL == other.URL;
        public override bool Equals(object obj) => Equals(obj as TVSeason);
        public override int GetHashCode() => 0; // idk
    }
}
