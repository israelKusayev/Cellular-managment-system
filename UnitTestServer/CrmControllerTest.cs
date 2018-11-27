﻿using System;
using System.Net.Http;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestServer
{
    [TestClass]
    public class CrmControllerTest
    {
        [TestMethod]
        public void LoginTest()
        {
            Employee emplyee;
            using (var client = new HttpClient())
            {
                var result = client.GetAsync($"http://localhost:54377/Api/Crm/Israel/1").Result;
                emplyee = result.Content.ReadAsAsync<Employee>().Result;
            }
            Assert.AreEqual("Israel", emplyee.UserName);
        }

        [TestMethod]
        public void LoginTestFaild()
        {
            Employee emplyee;
            using (var client = new HttpClient())
            {
                var result = client.GetAsync($"http://localhost:54377/Api/Crm/Israel/1").Result;
                emplyee = result.Content.ReadAsAsync<Employee>().Result;
            }
            Assert.AreNotEqual("Shay", emplyee.UserName);
        }
    }
}
