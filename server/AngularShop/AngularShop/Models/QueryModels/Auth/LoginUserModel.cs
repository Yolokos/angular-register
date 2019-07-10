using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularShop.Models.QueryModels
{
    //Model for authorization user
    public class LoginUserModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
