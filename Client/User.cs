using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class User
    {
        public string userName { get; private set; }
        public int balance { get; set; }

        public User(string _userName)
        {
            this.userName = _userName;
            this.balance = 0;
        }
    }
}