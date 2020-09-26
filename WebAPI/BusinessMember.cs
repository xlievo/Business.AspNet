using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI
{
    [Info("API/v2")]
    public class BusinessMember : BusinessBase
    {
        static readonly Timer timer = new System.Threading.Timer(new TimerCallback(async obj =>
        {
            await Business.Core.Configer.BusinessList["API/v2"].Command.AsyncCall("Test010", new object[] { new Test0011 { C31 = DateTime.Now.ToString(), C32 = "c32" }, 1123 });
        }));

        static BusinessMember()
        {
            //timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1));

            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        if (0 == Utils.WebSocketContainer.WebSockets.Count)
            //        {
            //            await Task.Delay(1);
            //            continue;
            //        }

            //        if (!Business.Core.Configer.BusinessList.TryGetValue("API/v2", out Business.Core.IBusiness business))
            //        {
            //            await Task.Delay(1);
            //            continue;
            //        }

            //        Parallel.For(0, 1000000, async c =>
            //         {
            //             await business.Command.AsyncCall("Test010", new object[] { new Test0011 { C31 = DateTime.Now.ToString(), C32 = "c32" }, 1123 });
            //         });

            //        break;
            //    }
            //});
        }

        static readonly string guids = string.Join(",", Enumerable.Range(0, 200).AsParallel().Select(c => Guid.NewGuid().ToString("N")));

        HttpClient httpClient;

        public BusinessMember(IHttpClientFactory httpClientFactory, IMemoryCache cache, string test123)
        {
            httpClient = httpClientFactory.CreateClient();

            cache.Set("key", 123);
            Debug.Assert(123 == cache.Get<int>("key"));
            Debug.Assert("test123" == test123);

            //this.BindAfter += () =>
            //{
            //    //Business.Core.Configer.BusinessList["API/v2"].Command.AsyncCall("Test010", new object[] { new Test0011 { C31 = "c31", C32 = "c32" }, 1123 });

            //    Task.Run(async () =>
            //    {
            //        for (; ; )
            //        {
            //            try
            //            {
            //                while (0 == WebSockets.Count)
            //                {
            //                    await Task.Delay(TimeSpan.FromSeconds(1));
            //                }

            //                Parallel.For(0, 1000000, i =>
            //                {
            //                    if (0 == WebSockets.Count) { return; }

            //                    Configer.BusinessList["API/v2"].Command.AsyncCall("Test010", new object[] { new Test0011 { C31 = guids }, 1123 });
            //                });

            //                await Task.Delay(30000);

            //                var first = WebSockets.FirstOrDefault();
            //                var webSocket = first.Value;

            //                if (webSocket?.State == WebSocketState.Open)
            //                {
            //                    await webSocket?.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, default);

            //                    WebSockets.TryRemove(first.Key, out _);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine(ex);
            //            }

            //        }
            //    });
            //};
        }

        public class Test001_Arg
        {
            public class Test004
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
                /// </summary>
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
                /// MyEnumMyEnumMyEnumMyEnumMyEnumMyEnum
                /// </summary>
                public MyEnum myEnum { get; set; }

                public class Test0010
                {
                    /// <summary>
                    /// C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1
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

                    B = 2,

                    /// <summary>
                    /// MyEnum C
                    /// </summary>
                    C = 4,
                }
            }
        }

        /// <summary>
        /// test doc Test001
        /// and Test001
        /// </summary>
        /// <param name="session">A token sample222</param>
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
        //[JsonCommand("jjjTest001jjj")]
        //[Command("wwwwwwwwwwww", Group = "j")]
        //[Command(Group = "zzz")]
        [Testing("test2",
         "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":0},\"dateTime\":\"2019-12-02T04:24\"}")]
        [Testing("test3",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":2},\"dateTime\":\"2019-12-02T05:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
        [Testing("test4",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T06:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
        [Testing("test5",
        "{\"arg\":{\"AAA\":[],\"A\":\"http://127.0.0.1:5000/doc/index.html\",\"B\":\"\",\"C\":{\"C1\":\"\",\"C2\":\"\",\"C3\":[]},\"D\":0,\"E\":false,\"F\":\"2019-12-02T06:24\",\"myEnum\":2},\"dateTime\":\"2019-12-02T07:24\",\"mm\":99.0234,\"fff\":777,\"bbb\":false}")]
        [Testing("test, important logic, do not delete!!!",
        "{\"arg\":{\"menu_item\":\"\",\"bbbb\":\"\",\"bbb\":\"\",\"aaa\":[\"aa\",\"bb\"],\"a\":\"http://127.0.0.1:5000/doc/index.html\",\"b\":\"\",\"c\":{\"c1\":\"ok\",\"c2\":\"😀😭\",\"c3\":[{\"c31\":\"cc1\",\"c32\":\"cc2\",\"aaa\":[]},{\"c31\":\"cc3\",\"c32\":\"cc4\",\"aaa\":[]},{\"c31\":\"cc5\",\"c32\":\"cc6\",\"aaa\":[]}]},\"d\":0,\"e\":false,\"f\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T08:24\",\"mm\":111.0123456,\"fff\":555,\"bbb\":true}")]
        public virtual async Task<IResult<Test004>> Test001(Session session, Test004 arg, DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true, Context context = null, WebSocket webSocket = null)
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

            //await receive.WebSocket?.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
            //var data = this.GetSocketObject(new object[] { arg, dateTime, mm, fff, bbb }).ToBytes();

            //var data = this.GetSocketData(new object[] { new Test0011 { C31 = "aaaadd111111111111" }, 2233 }, "Test010").ToBytes();

            //webSocket.SendAsync(data);

            //this.SendAsync(WebSockets, new object[] { new Test0011 { C31 = "aaaadd111111111111" }, 2233 }, null, "Test010");

            await Test010(new Test0011 { C31 = "aaaadd22222222222222" }, 2233);

            webSocket.SendObjectAsync("sssssssssss", "123456", false);

            //await webSocket.CloseAsync();

            var dd = this.ResultCreate(arg, "ssssssssssssss!@#", 99999) as IResultObject;
            dd.Business = new BusinessInfo("sss111", "xxxxxxx");


            var dd2 = dd.ToBytes();

            Business.AspNet.ResultObject<Test004X> ss2 = dd2;

            var ss3 = dd2.ToResultObject<Business.AspNet.ResultObject<Test004X>>();

            var ss4 = dd2.ToResult<Test004X>();

            return this.ResultCreate(arg, "ssssssssssssss!@#", 99999);

            //return this.ResultCreate(new { session, arg, files });
        }

        [Command("abc", Group = Utils.GroupWebSocket)]
        public virtual async Task<dynamic> Test004(Session session, Token token, List<Test001> arg, Context context = null, WebSocket socket = null)
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

        /// <summary>
        /// Test010!
        /// </summary>
        /// <param name="test">Test004XTest004XTest004XTest004X</param>
        /// <param name="b"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Push]
        public virtual async Task Test010(Test0011 test, int b, [Ignore(IgnoreMode.Arg)] params string[] id) => this.SendAsync(new object[] { test, b }, id);

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

        public virtual async Task<List<string>> Test014(Dictionary<string, string> aaa)
        {
            return new List<string> { "sssss" };
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
        /// Test004 MENU_ITEMMENU_ITEMMENU_ITEM@@@
        /// </summary>
        public string MENU_ITEM { get; set; }

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
        /// MyEnum C
        /// MyEnum C
        /// MyEnum C
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
