﻿using Business.Core.Annotations;
using Business.Core.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAPI50.Annotations
{
    /// <summary>
    /// JsonArgAttribute
    /// </summary>
    public class JsonArgAttribute : Business.AspNet.JsonArgAttribute
    {
        /// <summary>
        /// JsonArgAttribute
        /// </summary>
        public JsonArgAttribute() : base()
        {
            Description = "Json2 format";
        }

        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var opt = this.textJsonOptions;
            var data = await base.Proces<Type>(value as object);
            return data;
        }
    }

    public class TokenCheck : ArgumentAttribute
    {
        public TokenCheck(int state = -80, string message = null) : base(state, message) { }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
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
        [Injection]
        readonly HttpClient httpClient;

        public SessionCheck(int state = -81, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Session check";
        }

        public override async ValueTask<IResult> Proces(dynamic value)
        {
            //var bing = await httpClient.GetStringAsync("https://www.bing.com/");

            var key = value.Key as string;

            //..1: check 2: convert

            var session = new Session { Account = key };

            return this.ResultCreate(session);//return out session
        }
    }
}
