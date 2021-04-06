using NUnit.Framework;
using Moq;
using SportsExerciseBattle.Database;
using SportsExerciseBattle.HelperObjects;
using SportsExerciseBattle.REST_Server;
using SportsExerciseBattle;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using Npgsql;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace SportsExerciseBattle_TEST
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SendHttpContentTest()
        {
            var mockTcpHandler = new Mock<ITCP>();
            var mockRequestContext = new Mock<IReqContext>();
            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;

            mockTcpHandler.Setup(c => c.GetStream(It.IsAny<TcpClient>())).Returns(memoryStream);
            mockRequestContext.Setup(c => c.StatusCode).Returns("200 OK");
            mockRequestContext.Setup(c => c.ContentType).Returns("text/plain");
           mockRequestContext.Setup(c => c.Payload).Returns("SendHttpContentTest");

            WebHandler contentWriter = new WebHandler(mockTcpHandler.Object, mockRequestContext.Object);

            contentWriter.SendHttpContent();

            string expectedValue = "HTTP/1.1 200 OK\r\nServer: SEB-Server\r\n";
            expectedValue += "Content-Type: text/plain\r\nContent-Length: 19\r\n\r\n";
            expectedValue += "SendHttpContentTest\r\n";

            string actualValue = Encoding.ASCII.GetString(memoryStream.ToArray());

            Console.WriteLine(actualValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void BadRequestWrongMethodTest()
        {
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/users";
            HeaderInfo["RequestMethod"] = "PUT";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;

            reqContext.RequestCoordinator(true);
            Assert.AreEqual("400 Bad Request", reqContext.StatusCode);
        }

        [Test]
        public void ProfileTestSuccess()
        {
            // There is a dummy person in the Database with username Test
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/users/Test";
            HeaderInfo["RequestMethod"] = "GET";
            HeaderInfo["Authorization"] = "Basic Test-sebToken";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;

            reqContext.RequestCoordinator(true);
            Assert.AreEqual("200 OK", reqContext.StatusCode);
        }

        [Test]
        public void StatsTestSuccess()
        {
            // There is a dummy person in the Database with username Test
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/stats";
            HeaderInfo["RequestMethod"] = "GET";
            HeaderInfo["Authorization"] = "Basic Test-sebToken";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;

            reqContext.RequestCoordinator(true);
            Assert.AreEqual("200 OK", reqContext.StatusCode);
            Assert.AreEqual("{\"userID\":1,\"Count\":0,\"ELO\":1337}", reqContext.Payload);
        }

        [Test]
        public void HistoryTestSuccess()
        {
            // There is a dummy person in the Database with username Test
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/history";
            HeaderInfo["RequestMethod"] = "GET";
            HeaderInfo["Authorization"] = "Basic Test-sebToken";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;

            reqContext.RequestCoordinator(true);
            Assert.AreEqual("200 OK", reqContext.StatusCode);
            Assert.AreEqual("[]", reqContext.Payload);
        }

        [Test]
        public void TournamentTestSuccess()
        {
            // There is a dummy person in the Database with username Test
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/tournament";
            HeaderInfo["RequestMethod"] = "GET";
            HeaderInfo["Authorization"] = "Basic Test-sebToken";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;
          
            reqContext.RequestCoordinator(false);
            Assert.AreEqual("200 OK", reqContext.StatusCode);
            Assert.AreEqual("[]", reqContext.Payload);
        }

        [Test]
        public void GetTokenTestSuccess()
        {
            // There is a dummy person in the Database with username Test
            var HeaderInfo = new Dictionary<string, string>();
            HeaderInfo["RequestPath"] = "/sessions";
            HeaderInfo["RequestMethod"] = "POST";
            HeaderInfo["Username"] = "Test";
            HeaderInfo["Password"] = "123";

            ReqContext reqContext = new ReqContext();
            reqContext.HeaderInfo = HeaderInfo;

            reqContext.RequestCoordinator(false);
            Assert.AreEqual("200 OK", reqContext.StatusCode);
            Assert.AreEqual("Basic Test-sebToken", reqContext.Payload);
        }

        [Test]
        public void ConnectionToDbTest()
        {
            Assert.AreEqual(0, DatabaseHandler.PingDataBase());
        }

        [Test]
        public void TournamentMasterTest()
        {
            Server.activeTournament = true;

            Task.Run(() => Server.TournamentMaster(100));
            Thread.Sleep(1000);

            Assert.AreEqual(false, Server.activeTournament);
        }

        [Test]
        public void BadRequestTest()
        {
            var reqContext = new ReqContext();
            reqContext.RequestCoordinator(true);
            Assert.AreEqual("400 Bad Request", reqContext.StatusCode);
        }
    }
}