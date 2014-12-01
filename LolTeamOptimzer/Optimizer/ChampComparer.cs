using System;
using System.Collections.Generic;

namespace LolTeamOptimizer.Optimizer
{
    public class ChampComparer : IEqualityComparer<Champion>
    {
        public bool Equals(Champion x, Champion y)
        {
            //Check whether the objects are the same object. 
            if (ReferenceEquals(x, y)) return true;

            //Check whether the products' properties are equal. 
            return x != null && y != null && x.Id.Equals(y.Id);
        }

        public int GetHashCode(Champion obj)
        {
            //Get hash code for the Name field if it is not null. 
            int hashChampId = obj.Id.GetHashCode();

            //Get hash code for the Code field. 
            int hashChampName = obj.Name.GetHashCode();

            //Calculate the hash code for the product. 
            return hashChampId ^ hashChampName;
        }
    }
}