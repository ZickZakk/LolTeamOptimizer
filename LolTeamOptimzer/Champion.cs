//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LolTeamOptimizer
{
    using System;
    using System.Collections.Generic;
    
    public partial class Champion
    {
        public Champion()
        {
            this.IsStrongAgainst = new HashSet<IsStrongAgainst>();
            this.IsWeakAgainst = new HashSet<IsWeakAgainst>();
            this.GoesWellWith = new HashSet<GoesWellWith>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<IsStrongAgainst> IsStrongAgainst { get; set; }
        public virtual ICollection<IsWeakAgainst> IsWeakAgainst { get; set; }
        public virtual ICollection<GoesWellWith> GoesWellWith { get; set; }
    }
}
