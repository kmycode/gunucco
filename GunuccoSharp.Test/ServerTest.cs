using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp.Test
{
    [TestClass]
    public class ServerTest
    {
        [TestMethod]
        public async Task GetServerVersion()
        {
            var client = TestUtil.GetClient();

            var ver = await client.Server.GetVersionAsync();

            int j;

            Assert.IsFalse(string.IsNullOrWhiteSpace(ver));
            foreach (var num in ver.Split('.'))
            {
                Assert.IsTrue(int.TryParse(num, out j));
            }
        }
    }
}
