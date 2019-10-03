using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Factories;

namespace Engine.Models
{
    public class Location
    {
        public int XCoordinate { get; }
        public int YCoordinate { get; }
        public string Name { get; }
        public string Description { get; }
        public string ImageName { get; }

        public List<Quest> QuestsAvailableHere { get; } = new List<Quest>();

        public Trader TraderHere { get; set; }

        public Location(int xCoordinate, int yCoordinate, string name, string description, string imageName)
        {
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            Name = name;
            Description = description;
            ImageName = imageName;
        }

        public List<MonsterEncounter> MonstersHere { get; set; } =
            new List<MonsterEncounter>();

        public void AddMonster(int monsterID, int chanceOfEncountering)
        {
            if(MonstersHere.Exists(m => m.MonsterID == monsterID))
            {
                //This monster has already been added to this location.
                //So overwrite the ChanceOfEncountering with the new number.
                MonstersHere.First(m => m.MonsterID == monsterID).ChanceOfEncountering = chanceOfEncountering;
            }
            else
            {
                //This monster is not already at tthis location, so add it.
                MonstersHere.Add(new MonsterEncounter(monsterID, chanceOfEncountering));
            }
        }

        public Monster GetMonster()
        {
            if(!MonstersHere.Any()) //breaks early if there are no monsters in the list.
            {
                return null;
            }
            // Total the percentages of all monsters at this location.
            int totalChances = MonstersHere.Sum(m => m.ChanceOfEncountering);

            // Selects a random number between 1 and the total (in case chances is not 100).
            int randomNumber = RandomNumberGenerator.SimpleNumberBetween(1, totalChances);

            //Loop through the monster list,
            //adding the monster's percentage chance of appearing to the runningTotal var.
            //When the random number is lower than the runningTotal,
            //that is the monster to return.
            int runningTotal = 0;

            foreach (MonsterEncounter monsterEncounter in MonstersHere)
            {
                runningTotal += monsterEncounter.ChanceOfEncountering;

                if(randomNumber <= runningTotal)
                {
                    return MonsterFactory.GetMonster(monsterEncounter.MonsterID);
                }
            }
            //If there was a problem, return the last monster in the list. This is for safety to prevent crashing.
            return MonsterFactory.GetMonster(MonstersHere.Last().MonsterID);
        }
    }
}
