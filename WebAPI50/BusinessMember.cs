using Business.AspNet;
//using Business.Core;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using static WebAPI50.Test004X;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI50
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

        [Injection]
        HttpClient httpClient;

        [Injection]
        IApplicationBuilder app;

        public BusinessMember(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<BusinessMember> logger, string test123, ApplicationBuilder app)
        {
            logger.LogInformation(test123);

            //httpClient = httpClientFactory.CreateClient();

            cache.Set("key", 123);
            Debug.Assert(123 == cache.Get<int>("key"));
            Debug.Assert(nameof(test123) == test123);

            this.BindAfter += () =>
            {
                this.app.ToString().Log();
                "BindAfter".Log();

                this.Configer.CallAfterMethod += async method =>
                {
                    if (method.Result is IResult result)
                    {
                        if (1 == result.State)
                        {

                            //...//
                            //result.m = "ssssss";
                            method.Result = this.ResultCreate(result.State, "ssssss");
                        }
                    }
                };
            };


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
        /// zzzzZZZZZZZZZZZZZZZZZZZZZzzzzzzzzzzzz
        /// </summary>
        //[Parameters]
        public class Z
        {
            /// <summary>
            /// aaa2
            /// </summary>
            public string a { get; set; }

            /// <summary>
            /// bbb2
            /// </summary>
            public DateTime b { get; set; }

            public int c { get; set; }

            public Test001Result2 d { get; set; }

            public bool e { get; set; }

            public List<string> f { get; set; }

            public struct Test001Result2
            {
                /// <summary>
                /// aaa2
                /// </summary>
                public string a { get; set; }

                /// <summary>
                /// bbb2
                /// </summary>
                public string b { get; set; }
            }
        }

        //post json
        [NotBody]
        public virtual async ValueTask<IResult<Z>> TestZ(Token token2,  HttpFile files, Context context)
        {
            var d = await context.Request.Body.StreamReadStringAsync();

            var d2 = d.TryJsonDeserialize<Z>();

            return this.ResultCreate(d2);
        }

        public class SourceAddArg
        {
            [Size(Min = 1, Max = 128)]
            public string Name { get; set; }

            [Size(Max = 512)]
            public string Remark { get; set; }
        }

        public virtual async ValueTask<dynamic> SourceAdd(Token token, [Parameters] SourceAddArg arg)
        {
            return this.ResultCreate(new { token, arg });
        }

        public virtual async ValueTask<IResult<(Test0010? aaa, string bbb)>> TestAnonymous(Test0010? arg, string aaa) => this.ResultCreate((arg, aaa));

        public struct TestJsonArg
        {
            public string A { get; set; }

            public int B { get; set; }

            public int C { get; set; }
        }

        [Testing("test2", "{\"arg\":{\"A\":\"00112233\",\"B\":333,\"C\":666}}")]
        public virtual async ValueTask<dynamic> TestJson(TestJsonArg arg)
        {
            return this.ResultCreate(arg);
        }

        /// <summary>
        /// Test0011Test0011Test0011Test0011
        /// </summary>
        [DynamicObject]
        public class Test00122
        {
            /// <summary>
            /// C31C31C31C31C31C31
            /// </summary>
            public string C31 { get; set; }

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            public int C32 { get; set; }

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            [DynamicObject]
            public IEnumerable<Test0013> C33 { get; set; }

            public struct Test0013
            {
                /// <summary>
                /// C31C31C31C31C31C31
                /// </summary>
                public string C311 { get; set; }

                /// <summary>
                /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
                /// </summary>
                public int C322 { get; set; }
            }
        }
        //[{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555},{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}]
        [Testing("test2", "{\"arg\":[\"{\\\"C31\\\":\\\"222\\\",\\\"C32\\\":555,\\\"C33\\\":[{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555},{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}]}\"," +
            "\"{\\\"C31\\\":\\\"333\\\",\\\"C32\\\":666}\"]," +
            "" +
            "\"arg2\":{\"C31\":\"222\",\"C32\":555,\"C33\":[\"{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555}\",\"{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}\"]}}")]
        [Testing("test3", "{\"arg2\":{\"C31\":\"222\",\"C32\":555,\"C33\":[\"{\\\"C311\\\":\\\"222\\\",\\\"C322\\\":555}\",\"{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}\"]}}")]//,\"C33\":[\"{\\\"C311\\\":\\\"333\\\",\\\"C322\\\":666}\"]
        [Doc(Group = "Module 1")]
        public virtual async ValueTask<dynamic> Test00555(IEnumerable<Test00122> arg, Test00122 arg2, [DynamicObject] dynamic arg3)
        {
            //var dd = new Test00122 { C31 = "222", C32 = 555, C33 = new List<string> { new Test00122.Test0013 { C311 = "222", C322 = 555 }.JsonSerialize(), new Test00122.Test0013 { C311 = "333", C322 = 666 }.JsonSerialize() } }.JsonSerialize().JsonSerialize();
            //var dd = new Test00122 { C31 = "222", C32 = 555, C33 = new List<Test00122.Test0013> { new Test00122.Test0013 { C311 = "222", C322 = 555 }, new Test00122.Test0013 { C311 = "333", C322 = 666 } } }.JsonSerialize();

            //var dd3 = arg.JsonSerialize().TryJsonDeserialize<IList<Test00122>>();

            return this.ResultCreate(new { arg, arg2, arg3 });
        }

        public virtual async ValueTask<dynamic> TestFile(Context context)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "aaa.txt");

            var txt = System.IO.File.ReadAllBytes(path);

            return context.File(txt, "application/octet-stream", "aaa.txt");
        }

        /// <summary>
        /// Test0011Test0011Test0011Test0011
        /// </summary>
        public class Test001222
        {
            /// <summary>
            /// C31C31C31C31C31C31
            /// </summary>
            public string C31 { get; set; }

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            public int C32 { get; set; }

            /// <summary>
            /// C322C322C322C322C322
            /// </summary>
            public int C322;

            /// <summary>
            /// C333C333C333C333C333C333
            /// </summary>
            public IEnumerable<TeamMember> C333;

            /// <summary>
            /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
            /// </summary>
            public IEnumerable<Test0013> C33;

            public class Test0013
            {
                /// <summary>
                /// C31C31C31C31C31C31
                /// </summary>
                public IEnumerable<TeamMember> C311;

                /// <summary>
                /// C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32C32
                /// </summary>
                public int C322;
            }

            public class TeamMember
            {
                /// <summary>
                /// UserId
                /// </summary>
                public string UserId { get; set; }
                /// <summary>
                /// UserImg
                /// </summary>
                public string UserImg { get; set; }
            }
        }

        [Doc(Group = "Module 1")]
        public virtual async ValueTask<IResult<Test001222>> TestDoc00555(Test001222 arg)
        {
            return this.ResultCreate(arg);
        }

        /// <summary>
        /// test doc Test001
        /// and Test001
        /// </summary>
        /// <param name="session">A token
        /// sample222</param>
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
        [Doc(Group = "Module 1", Position = 1)]
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
        "{\"arg\":{\"menu_item\":\"\",\"bbbb\":\"\",\"bbb\":\"\",\"aaa\":[\"aa\",\"bb\"],\"a\":\"http://127.0.0.1:5000/doc/index.html\",\"b\":\"\",\"c\":{\"c1\":\"ok\",\"c2\":\"😀😭\",\"c3\":[{\"c31\":\"cc1\",\"c32\":\"cc2\",\"aaa\":[]},{\"c31\":\"cc3\",\"c32\":\"cc4\",\"aaa\":[]},{\"c31\":\"cc5\",\"c32\":\"cc6\",\"aaa\":[]}]},\"d\":19,\"e\":false,\"f\":\"2019-12-02T06:24\",\"myEnum\":4},\"dateTime\":\"2019-12-02T08:24\",\"mm\":111.0123456,\"fff\":555,\"bbb\":true}")]
        public virtual async ValueTask<IResult<Test004>> Test001(Session session, Test004 arg, [CheckNull(CheckValueType = true)] DateTime? dateTime, HttpFile httpFile = default, [Ignore(IgnoreMode.BusinessArg)][Test2] decimal mm = 0.0234m, [Ignore(IgnoreMode.BusinessArg)] int fff = 666, [Ignore(IgnoreMode.BusinessArg)] bool bbb = true, Context context = null, WebSocket webSocket = null)
        {
            //var bing = await httpClient.GetStringAsync("https://www.bing.com/");
            //this.SendAsync<string>("sss");
            var r = Utils.Hosting;

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

            if (null != httpFile)
            {
                var files2 = await httpFile.GetFileAsync();
                var files3 = await httpFile.GetFileAsync("db2.sh");
                var files4 = httpFile.GetFilesAsync();

                await foreach (var item in files4)
                {
                    item.Key.Log();
                }


                var files = httpFile?.Select(c => new { key = c.Key, length = c.Value.Length }).ToList();
            }

            //await receive.WebSocket?.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
            //var data = this.GetSocketObject(new object[] { arg, dateTime, mm, fff, bbb }).ToBytes();

            //var data = this.GetSocketData(new object[] { new Test0011 { C31 = "aaaadd111111111111" }, 2233 }, "Test010").ToBytes();

            //webSocket.SendAsync(data);

            //this.SendAsync(WebSockets, new object[] { new Test0011 { C31 = "aaaadd111111111111" }, 2233 }, null, "Test010");

            this.SendAsync(new Test0011 { C31 = "aaaadd22222222222222" });

            //await Test010(new Test0011 { C31 = "aaaadd22222222222222" }, 2233);

            webSocket.SendAsync("sssssssssss", "123456");

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

        [Command("abc", Group = Grouping.MessagePack)]
        public virtual async Task<dynamic> Test004(Session session, Token token, List<Test001> arg, Context context = null, WebSocket socket = null)
        {
            return this.ResultCreate(new { token, arg, State = token.Remote, context.Request.Headers }, "aaaa!@#$");
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

        ///// <summary>
        ///// Test010!
        ///// </summary>
        ///// <param name="test">Test004XTest004XTest004XTest004X</param>
        ///// <param name="b"></param>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[Push]
        //public virtual async Task Test010(Session session, Token token, Test0011 test, int b, [Ignore(IgnoreMode.Arg)] params string[] id) => this.SendAsync(new object[] { test, b }, id);

        /// <summary>
        /// MyLogicArg!
        /// </summary>
        public struct MyLogicArg
        {
            /// <summary>
            /// AAA
            /// </summary>
            [CheckNull]
            public string A { get; set; }

            /// <summary>
            /// BBB
            /// </summary>
            public string B { get; set; }
        }
        public struct WebSocketPushLogicResult
        {
            /// <summary>
            /// CCC
            /// </summary>
            public string C { get; set; }

            /// <summary>
            /// DDD
            /// </summary>
            public string D { get; set; }
        }

        [Push]
        public virtual async ValueTask<IResult<WebSocketPushLogicResult>> WebSocketPushLogic(Token token, MyLogicArg arg, [Ignore(IgnoreMode.Arg)] params string[] id)
        {
            var pushResult = new WebSocketPushLogicResult { C = arg.A, D = arg.B };

            this.SendAsync(pushResult, id);// The push data must be consistent with the return object

            return this.ResultCreate(pushResult);// Used as a return standard convention
        }
        public virtual async ValueTask<IResult<MyLogicArg>> MyLogic(Token token, Context context, HttpFile files, MyLogicArg arg)
        {
            return this.ResultCreate(arg);
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

        public virtual async Task<List<string>> Test014(Dictionary<string, string> aaa)
        {
            return new List<string> { "sssss" };
        }

        [Use(typeof(Context))]
        [InjectionArgCheck]
        public class TestInjectionArg
        {
            public string Method { get; set; }

            public string Body { get; set; }
        }

        public class InjectionArgCheck : ArgumentAttribute
        {
            public InjectionArgCheck(int state = -811, string message = null) : base(state, message)
            {
                this.CanNull = false;
                this.Description = "InjectionArg";
            }

            public override async ValueTask<IResult> Proces(dynamic value)
            {
                var context = value as Context;

                var session = new TestInjectionArg { Method = context.Request.Method, Body = await context.Request.Body.StreamReadStringAsync() };

                return this.ResultCreate(session);//return out session
            }
        }

        public virtual async Task<IResult<TestInjectionArg>> TestInjection(TestInjectionArg arg)
        {
            return this.ResultCreate(arg);
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
        [CheckNull(CheckValueType = true)]
        public decimal? D { get; set; }

        public bool E { get; set; }

        /// <summary>
        /// FF
        /// </summary>
        [CheckNull(CheckValueType = true)]
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

    //public override async ValueTask<IResult> Proces(dynamic value)
    //{
    //    return this.ResultCreate(value + 0.1m);
    //}

    public override async ValueTask<IResult> Proces<Type>(dynamic token, dynamic value)
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
