using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    public class World
    {
        private List<Location> _locations = new List<Location>(); //will store a list of locations.

        internal void AddLocation(Location location)
        {
            _locations.Add(location);
        }

        public Location LocationAt(int xCoordinate, int yCoordinate)//returns our location.
        {
            foreach (Location loc in _locations)
            {
                if(loc.XCoordinate == xCoordinate && loc.YCoordinate == yCoordinate) //checks if the input coords match any coords in our list.
                {
                    return loc;
                }
            }
            return null;
        }
    }
}
