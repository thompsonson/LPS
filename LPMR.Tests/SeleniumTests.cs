using System;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace LPMR.Tests
{
    [TestFixture]
    public class SeleniumTests
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;
        
        [SetUp]
        public void SetupTest()
        {
            driver = new FirefoxDriver();
            baseURL = "http://localhost:8014/";
            verificationErrors = new StringBuilder();
            // fire up the browser
            driver.Navigate().GoToUrl(baseURL);
        }
        
        [TearDown]
        public void TeardownTest()
        {
            try
            {
                //driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            Assert.AreEqual("", verificationErrors.ToString());
        }

        [Test]
        public void VerifyUserCannotEditMonitorsTest()
        {
            // log on
            logon("user", "password", "/monitor");
            // check for the link
            Assert.IsFalse(IsElementDisplayed(By.Id("editMonitor1")));
            // TODO: Check for the NEw MOnitor link
            //log off
            logout();
        }
        
        [Test]
        public void LogOnAndEditTest()
        {
            // log on
            logon("admin", "password", "/monitor");
            /* click edit and verify the validation on the Name, RunInterval and Query field */
            driver.FindElement(By.Id("editMonitor1")).Click();

            // do similar for the Query input
            checkValidation("Name", "", "test name");
            // do similar for the Query input
            checkValidation("RunInterval", "12", "33");
            // do similar for the Query input
            checkValidation("SQL", "select* from test", "select * from test");

            // save the entries - TODO: check the DB
            driver.FindElement(By.LinkText("Save")).Click();
            // confirm no error showing
            Assert.IsFalse(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
            // check it's saved and the edit link is back
            Thread.Sleep(1000);
            Assert.IsTrue(IsElementDisplayed(By.Id("editMonitor1")));

            logout();

            /*
            driver.FindElement(By.Id("Alert")).Click();
            driver.FindElement(By.LinkText("Save")).Click();
            driver.FindElement(By.Id("new")).Click();
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("Test");
            driver.FindElement(By.Name("RunInterval")).Clear();
            driver.FindElement(By.Name("RunInterval")).SendKeys("100");
            driver.FindElement(By.Name("SQL")).Clear();
            driver.FindElement(By.Name("SQL")).SendKeys("Select* from helloworld");
            driver.FindElement(By.CssSelector("div.formErrorContent")).Click();
            driver.FindElement(By.LinkText("Save")).Click();
            driver.FindElement(By.Id("form-validation-field-6")).Clear();
            driver.FindElement(By.Id("form-validation-field-6")).SendKeys("Select * from helloworld");
            driver.FindElement(By.LinkText("Save")).Click();
             * */
        }

        private void logout()
        {
            Console.WriteLine("Logging out");
            /* log on to the admin interface */
            driver.Url = baseURL + "/logout";
            //TODO: how to verify log off is successful?
            // need to wait for the redirect
            Thread.Sleep(1000);
            driver.Url = baseURL + "/monitor";
            Thread.Sleep(500);
            //http://localhost:8014/login?returnUrl=/monitor
            Assert.AreEqual(baseURL + "login?returnUrl=/monitor", driver.Url);
        }

        private void logon(string Username, string Password, string returnURL = "/")
        {
            Console.WriteLine("Logging in");
            /* log on to the admin interface */
            //driver.Navigate().GoToUrl(baseURL + "/login?returnUrl="+ returnURL);
            driver.Url = baseURL + "/login?returnUrl=" + returnURL;
            driver.FindElement(By.Name("Username")).Clear();
            driver.FindElement(By.Name("Username")).SendKeys(Username);
            driver.FindElement(By.Name("Password")).Clear();
            driver.FindElement(By.Name("Password")).SendKeys(Password);
            driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();

            /* wait for the monitor page to load and confirm */
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until((d) =>
            {
                return d.FindElement(By.Id("welcome"));
                //return d.Title.StartsWith("selenium"); 
            });
            Assert.AreEqual("Welcome " + Username, driver.FindElement(By.Id("welcome")).Text);
        }

        private void checkValidation(string inputName, string incorrectValueToEnter, string correctValueToEnter)
        {
            Console.WriteLine("Checking Validation");
            // confirm no error showing
            Assert.IsFalse(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
            // put incorrect value in the input field
            driver.FindElement(By.Name(inputName)).Clear();
            driver.FindElement(By.Name(inputName)).SendKeys(incorrectValueToEnter);
            driver.FindElement(By.Name(inputName)).SendKeys(Keys.Tab);
            Assert.IsTrue(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
            driver.FindElement(By.CssSelector("div.formErrorContent")).Click();
            // need to wait for a bit as it doesn't disappear immediately
            Thread.Sleep(1000);
            Assert.IsFalse(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
            // now click the save link to confirm the validation occurs there
            driver.FindElement(By.LinkText("Save")).Click();
            Assert.IsTrue(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
            // put the correct value in the input field
            driver.FindElement(By.Name(inputName)).Clear();
            driver.FindElement(By.Name(inputName)).SendKeys(correctValueToEnter);
            driver.FindElement(By.Name(inputName)).SendKeys(Keys.Tab);
            // need to wait for a bit as it doesn't disappear immediately
            Thread.Sleep(1000);
            Assert.IsFalse(IsElementDisplayed(By.CssSelector("div.formErrorContent")));
        }

        private bool IsElementDisplayed(By by)
        {
            try
            {
                return driver.FindElement(by).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        private string CloseAlertAndGetItsText() {
            try {
                IAlert alert = driver.SwitchTo().Alert();
                if (acceptNextAlert) {
                    alert.Accept();
                } else {
                    alert.Dismiss();
                }
                return alert.Text;
            } finally {
                acceptNextAlert = true;
            }
        }
    }
}
