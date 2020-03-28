using Business.Core.Annotations;
using Business.Core.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Annotations
{
    public class TokenCheck : ArgumentAttribute
    {
        public TokenCheck(int state = -80, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Token check";
            this.Message = "Token is null";
        }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var key = value.Key as string;

            //..1: check token key

            if (string.IsNullOrWhiteSpace(key))
            {
                //return this.ResultCreate(this.State, this.Message);
            }

            return this.ResultCreate(); //ok
        }
    }

    public class SessionCheck : ArgumentAttribute
    {
        public SessionCheck(int state = -81, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Session check";
        }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            var key = value.Key as string;

            //..1: check 2: convert

            var session = new Session { Account = key };

            return this.ResultCreate(session);//return out session
        }
    }
}
