using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Model.Interfaces
{
    public interface ICanFight
    {
        event Action OnDeath;
        void Die();
        bool IsAlive { get; }
        int GetDamage();
        void DealDamage(int damage);
    }
}
