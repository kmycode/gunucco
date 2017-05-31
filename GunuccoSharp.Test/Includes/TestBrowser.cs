using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunuccoSharp.Test
{
    class TestBrowser : IDisposable
    {
        private ChromeDriver driver;
        public IWebDriver Driver => this.driver;

        public TestBrowser()
        {
            // Chrome
            this.driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(@"C:\Users\KMY\Documents\Visual Studio 2017\Projects\Gunucco\GunuccoSharp.Test\bin\Debug\netcoreapp1.1"));
        }

        public TestBrowser(string uri) : this()
        {
            this.driver.Url = uri;
        }

        public void Dispose()
        {
            this.driver.Dispose();
        }

        public IWebElement GetOauthRequestForm()
        {
            var form = this.driver.FindElements(By.TagName("form"))
                        .Where(e => !string.IsNullOrEmpty(e.GetAttribute("action")))
                        .Single(e => e.GetAttribute("action").EndsWith("/web/oauth/done"));
            return form;
        }

        public IEnumerable<string> GetOauthScopeList()
        {
            var form = this.GetOauthRequestForm();
            return form.FindElements(By.TagName("ul")).First().FindElements(By.TagName("li")).Select(e => e.Text).ToArray();
        }
    }
}
