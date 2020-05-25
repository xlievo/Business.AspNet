using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebAPI
{
    [Info("API/v2")]
    public class BusinessMember : BusinessBase
    {
        /// <summary>
        /// test doc Test001
        /// and Test001
        /// </summary>
        /// <param name="token">A token sample222</param>
        /// <param name="arg"></param>
        /// <param name="dateTime"></param>
        /// <param name="httpFile"></param>
        /// <param name="mm">mmmmmmmm!</param>
        /// <param name="fff"></param>
        /// <param name="bbb"></param>
        /// <returns>
        /// rrrrrr
        /// rrrrrr2
        /// </returns>
        //[Doc(Group = "Module 1", Position = 1)]
        //[Command("AAA")]
        //[Command("jjjTest001jjj", Group = Utils.GroupJson)]
        [JsonCommand("jjjTest001jjj")]
        //[Command("wwwwwwwwwwww", Group = "j")]
        //[Command(Group = "zzz")]
        [Testing("test2",
             "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T04:24\",99.0234,777,false]",
             "{\"AAA\":\"111\",\"BBB\":\"222\"}")]
        [Testing("test3",
             "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T05:24\",99.0234,777,false]")]
        [Testing("test4",
             "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T06:24\",99.0234,777,false]")]
        [Testing("test5",
             "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T07:24\",99.0234,777,false]")]
        [Testing("test, important logic, do not delete!!!",
             "[{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"ok\",\"C2\":\"😀😭\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":\"C\"},\"2019-12-02T08:24\",99.0234,777,false]")]
        public virtual async Task<IResult<Test004>> Test001(Session session, Test004 arg, DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)]int fff = 666, [Ignore(IgnoreMode.BusinessArg)]bool bbb = true, Context context = null)
        {
            context?.Response.Headers.TryAdd("sss", "qqq");

            var ss = System.Text.Encoding.UTF8.GetBytes("a1");
            dynamic args = new System.Dynamic.ExpandoObject();
            args.token = session;
            args.arg = arg;
            if (args.arg.B == "ex")
            {
                throw new Exception("Method exception!");
            }

            if (args.arg.B == "ex2")
            {
                return this.ResultCreate(-911, "dsddsa");
            }

            var files = httpFile?.Select(c => new { key = c.Key, length = c.Value.Length }).ToList();

            return this.ResultCreate(arg);
            //return this.ResultCreate(new { session, arg, files });
        }

        [Command("abc", Group = Utils.GroupWebSocket)]
        public virtual async Task<dynamic> Test004(Session session, Token token, List<Test001> arg, Context context = null, WebSocket socket = null, [Ignore(IgnoreMode.BusinessArg)][Test2]decimal mm = 0.0234m)
        {
            return this.ResultCreate(new { token, arg, State = token.Remote }, "aaaa!@#$");
        }

        public virtual async Task<MyEnum> Test005(Test001 test001)
        {
            return MyEnum.B;
        }

        public virtual async Task<IResult<MyEnum>> Test006(Test0011 test0011)
        {
            return this.ResultCreate(MyEnum.B);
        }

        public virtual async Task<List<MyEnum>> Test007(List<Test001> test001)
        {
            return new List<MyEnum> { MyEnum.B };
        }

        public virtual async Task<IResult<List<MyEnum>>> Test008(string a, string b)
        {
            return this.ResultCreate(new List<MyEnum> { MyEnum.B });
        }

        public virtual async Task<List<Test0011>> Test009(string a, string b)
        {
            return new List<Test0011> { new Test0011 { AAA = new List<string> { "sssss" } } };
        }

        public virtual async Task<IResult<Test004X>> Test010(Test004X test)
        {
            return this.ResultCreate(test);
        }

        public virtual async Task<List<string>> Test011(string a, string b)
        {
            return new List<string> { "sssss" };
        }

        /// <summary>
        /// Test012Test012Test012Test012
        /// </summary>
        /// <returns></returns>
        public virtual async Task Test012()
        {
            return;
        }

        /// <summary>
        /// Test013Test013Test013Test013
        /// </summary>
        /// <returns></returns>
        public virtual async Task<string> Test013()
        {
            return "ssss";
        }
    }

    /// <summary>
    /// Test001Test001Test001Test001
    /// </summary>
    public struct Test001
    {
        /// <summary>
        /// aaaaaaaaaa
        /// </summary>
        public string A { get; set; }

        /// <summary>
        /// bbbbbbbbbbbbbbbbbb
        /// </summary>
        public string B { get; set; }
    }

    /// <summary>
    /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
    /// </summary>
    public enum MyEnum
    {
        /// <summary>
        /// MyEnum A
        /// </summary>
        A = 0,
        /// <summary>
        /// 
        /// </summary>
        B = 2,
        /// <summary>
        /// MyEnum C
        /// </summary>
        C = 4,
    }

    public class Test002 : Test001<Test0011>
    {
        /// <summary>
        /// Test002 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BB { get; set; }
    }

    public class Test003<T> : Test001<T>
    {
        /// <summary>
        /// Test003 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BBB { get; set; }
    }

    /// <summary>
    /// Test004Test004Test004Test004Test004Test004Test004
    /// Test004Test004Test004Test004Test004Test004Test004
    /// </summary>
    //[Parameters(Group = Utils.BusinessJsonGroup)]
    public class Test004 : Test003<Test0011>
    {
        /// <summary>
        /// Test004 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB@@@
        /// </summary>
        public string BBBB { get; set; }
    }

    /// <summary>
    /// Test001Test001Tes
    /// t001Test001Test001Test001
    /// </summary>
    public class Test001<T>
    {
        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        public List<string> AAA { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        //[Test]
        [Alias("password")]
        [CheckNull]
        //[@CheckEmail]
        [CheckUrl]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }


        //[Test]
        public Test0010<T> C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        public DateTime F { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MyEnum myEnum { get; set; }

        /// <summary>
        /// Test0010 Test0010 Test0010 Test0010
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Test0010<T>
        {
            /// <summary>
            /// C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1
            /// </summary>
            [Test]
            public string C1 { get; set; }

            /// <summary>
            /// C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2
            /// </summary>
            public string C2 { get; set; }

            /// <summary>
            /// C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3
            /// </summary>
            public List<T> C3 { get; set; }

            //public string C22 { get; set; }

            //public Test0011 C33 { get; set; }

            //public Test0011 C34 { get; set; }

            //public string C35 { get; set; }
        }
    }

    /// <summary>
    /// Test0011Test0011Test0011Test0011Test0011
    /// </summary>
    public class Test0011
    {
        /// <summary>
        /// C31C31C31C31C31C31
        /// </summary>
        public string C31 { get; set; }

        /// <summary>
        /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
        /// </summary>
        public string C32 { get; set; }

        /// <summary>
        /// AAA@@@
        /// </summary>
        public List<string> AAA { get; set; }
    }

    public struct Test004X
    {
        /// <summary>
        /// Test004 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB@@@
        /// </summary>
        public string BBBB { get; set; }

        /// <summary>
        /// Test003 BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string BBB { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// </summary>
        public List<string> AAA { get; set; }

        /// <summary>
        /// AAAAAAAAAaaaaaaaaaaaaaaaaaaaaaaaAAAAAAA
        /// <h5><code>CheckNull</code></h5><h5><code>CheckUrl</code></h5>
        /// </summary>
        [Alias("password")]
        [CheckNull]
        //[@CheckEmail]
        [CheckUrl]
        public string A { get; set; }

        /// <summary>
        /// BBBBBBBBbbbbbbbbbbbbbbbbbBBBBBBBBBBBBBBBBBB
        /// </summary>
        public string B { get; set; }

        /// <summary>
        /// Test0010 Test0010 Test0010 Test0010
        /// </summary>
        public Test0010 C { get; set; }

        /// <summary>
        /// DDD
        /// </summary>
        public decimal D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        public DateTime F { get; set; }

        /// <summary>
        /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum<br/><strong>A : 0</strong> MyEnum A<br/><strong>B : 2</strong><br/><strong>C : 4</strong> MyEnum C
        /// </summary>
        public MyEnum myEnum { get; set; }

        public struct Test0010
        {
            /// <summary>
            /// C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1
            /// <h5><code>Test</code></h5>
            /// </summary>
            public string C1 { get; set; }

            /// <summary>
            /// C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2
            /// </summary>
            public string C2 { get; set; }

            /// <summary>
            /// C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3
            /// </summary>
            public List<Test0011> C3 { get; set; }

            public struct Test0011
            {
                /// <summary>
                /// C31C31C31C31C31C31
                /// </summary>
                public string C31 { get; set; }

                /// <summary>
                /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
                /// </summary>
                public string C32 { get; set; }

                /// <summary>
                /// AAA@@@
                /// </summary>
                public List<string> AAA { get; set; }
            }
        }

        /// <summary>
        /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum<br/><strong>A : 0</strong> MyEnum A<br/><strong>B : 2</strong><br/><strong>C : 4</strong> MyEnum C
        /// </summary>
        public enum MyEnum
        {
            A = 0,

            B = 2,

            C = 4,
        }
    }
}

public class Test2Attribute : ArgumentAttribute
{
    public Test2Attribute(int state = 112, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces(dynamic value)
    {
        return this.ResultCreate(value + 0.1m);
    }
}

public class TestAttribute : ArgumentAttribute
{
    public TestAttribute(int state = 111, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value)
    {
        switch (value)
        {
            case "ok":
                return this.ResultCreate("OK!!!");
            case "error":
                return this.ResultCreate(this.State, $"{this.Alias} cannot be empty");

            case "data":
                return this.ResultCreate(value + "1122");

            default: break;
        }

        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(new Exception("Argument exception!")).Throw();

        return default;
    }
}
