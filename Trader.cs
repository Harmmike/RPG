﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    public class Trader : LivingEntity
    {
        public int ID { get; }

        public Trader(int id, string name)
            : base(name, 9999, 9999, 9999) //we don't need hp on traders, and right now traders have *infinite amount of gold.
        {
            ID = id;
        }
    }
}
