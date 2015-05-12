/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

#if !CF && !SILVERLIGHT

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Db4objects.Db4o.CS.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Encoding;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Foundation.Net.SSL
{
    public class SslSocketTestCase : ITestCase
    {
        private const string Message = "SslSocketFactory test case!";
        private X509Certificate2 _serverCertificate;

        public void Test()
        {
            ISocket4Factory sslSocketFactory = new SslSocketFactory(new StandardSocket4Factory(), ServerCertificate());
            var serverSocket = sslSocketFactory.CreateServerSocket(0);

            var clientEncryptedContentsSocketFactory = NewPassThroughSocketFactory();
            var clientPassThroughSocketFactory =
                new PassThroughSocketFactory(new SslSocketFactory(clientEncryptedContentsSocketFactory,
                    ValidateServerCertificate));

            var clientThread = StartClient(serverSocket.GetLocalPort(), clientPassThroughSocketFactory);

            var msg = ReadString(serverSocket.Accept());

            Assert.AreEqual(Message, msg);
            AssertAreNotEqual(clientEncryptedContentsSocketFactory.LastClient.Written,
                clientPassThroughSocketFactory.LastClient.Written);

            clientThread.Join();
        }

        public void TestClientCertificateValidation()
        {
            ISocket4Factory sslSocketFactory = new SslSocketFactory(new StandardSocket4Factory(), ServerCertificate());
            var serverSocket = sslSocketFactory.CreateServerSocket(0);

            ThreadPool.QueueUserWorkItem(delegate { serverSocket.Accept(); });

            var clientSocketFactory = new SslSocketFactory(new StandardSocket4Factory(), delegate { return false; });

            Assert.Expect(typeof (AuthenticationException),
                delegate { clientSocketFactory.CreateSocket("localhost", serverSocket.GetLocalPort()); });
        }

        public void TestServerAcceptSocketTimeout()
        {
            var serverThread =
                new Thread(
                    delegate(object serverSocket)
                    {
                        Assert.Expect(typeof (Exception), delegate { AsServerSocket(serverSocket).Accept(); });
                    });

            AssertTimeoutBehavior(serverThread, 20, new StandardSocket4Factory());
        }

        public void TestServerReadSocketTimeout()
        {
            var server = new Thread(delegate(object serverSocket)
            {
                var client = AsServerSocket(serverSocket).Accept();
                client.SetSoTimeout(1000);
                Assert.Expect(typeof (Exception), delegate { client.Read(new byte[1], 0, 1); });
            });

            var serverTimeout = MinutesToMiliseconds(5);
            ISocket4Factory clientSocketFactory = new SslSocketFactory(new StandardSocket4Factory(),
                delegate { return true; });

            AssertTimeoutBehavior(server, serverTimeout, clientSocketFactory);
        }

        private void AssertTimeoutBehavior(Thread serverTrigger, int serverTimeout, ISocket4Factory clientSocketFactory)
        {
            ISocket4Factory sslSocketFactory = new SslSocketFactory(new StandardSocket4Factory(), ServerCertificate());
            var serverSocket = sslSocketFactory.CreateServerSocket(0);
            serverSocket.SetSoTimeout(serverTimeout);

            serverTrigger.IsBackground = true;
            serverTrigger.Start(serverSocket);

            var clientSocket = clientSocketFactory.CreateSocket("localhost", serverSocket.GetLocalPort());

            if (!serverTrigger.Join(MinutesToMiliseconds(2)))
            {
                serverTrigger.Abort();
                Assert.Fail("Server thread should have timedout.");
            }
        }

        private static IServerSocket4 AsServerSocket(object socket)
        {
            return (IServerSocket4) socket;
        }

        private static int MinutesToMiliseconds(int minutes)
        {
            return 1000*60*minutes;
        }

        private static void AssertAreNotEqual(byte[] encrypted, byte[] plainText)
        {
            Assert.AreNotEqual(encrypted.Length, plainText.Length);
            var diffCount = 0;
            for (var i = 0; i < encrypted.Length && i < plainText.Length; i++)
            {
                if (encrypted[i] != plainText[i])
                {
                    diffCount++;
                }
            }

            Assert.IsGreater(0, diffCount);
            TestPlatform.Out.WriteLine("Diff count {0} in {1} bytes.", diffCount, plainText.Length);
        }

        private static Thread StartClient(int port, ISocket4Factory factory)
        {
            var clientTread = new Thread(delegate()
            {
                var clientSocket = factory.CreateSocket("localhost", port);
                SendString(clientSocket, Message);
            });

            clientTread.Name = "SslSocketTest thread";
            clientTread.Start();

            return clientTread;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslpolicyerrors)
        {
            return certificate.Subject.Contains("CN=ssl-test.db4o.anywere");
        }

        private static void SendString(ISocket4 clientSocket, string message)
        {
            var data = BytesToSendFor(message);
            clientSocket.Write(data, 0, data.Length);
        }

        private static string ReadString(ISocket4 socket)
        {
            var buffer = ReadBufferFrom(socket, Const4.IntLength);
            var marshalledStringSize = buffer.ReadInt();

            var marshalledString = ReadBufferFrom(socket, marshalledStringSize);
            var io = new LatinStringIO();
            return io.ReadLengthAndString(marshalledString);
        }

        private static ByteArrayBuffer ReadBufferFrom(ISocket4 socket, int length)
        {
            var buffer = ReadFrom(socket, length);
            return new ByteArrayBuffer(buffer);
        }

        private static byte[] ReadFrom(ISocket4 socket, int length)
        {
            var buffer = new byte[length];
            socket.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private static byte[] BytesToSendFor(string message)
        {
            var io = new LatinStringIO();
            var marshalledStringLength = io.Length(message);
            var buffer = new ByteArrayBuffer(marshalledStringLength + Const4.IntLength);

            buffer.WriteInt(marshalledStringLength);

            io.WriteLengthAndString(buffer, message);
            buffer.Seek(0);
            return buffer.ReadBytes(buffer.Length());
        }

        private static PassThroughSocketFactory NewPassThroughSocketFactory()
        {
            return new PassThroughSocketFactory(new StandardSocket4Factory());
        }

        private X509Certificate2 ServerCertificate()
        {
            if (_serverCertificate == null)
            {
                var bytes = Certificates.CreateSelfSignCertificate("CN=ssl-test.db4o.anywere, OU=db4o",
                    DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), "db4o-ssl-test");
                _serverCertificate = new X509Certificate2(bytes, "db4o-ssl-test");
            }

            return _serverCertificate;
        }
    }
}

#endif