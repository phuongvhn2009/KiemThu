using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Tests.Pages;

public class LabPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    public void Open(string path) => Driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/" + path);
    public void StartLoading() => Find(By.CssSelector("#start button")).Click();
    public string Finish(bool wait) => (wait ? Find(By.Id("finish")) : Driver.FindElement(By.Id("finish"))).Text;
    public string Alert(string kind, string? input = null)
    {
        Find(By.CssSelector($"button[onclick='{kind}()']")).Click();
        var alert = new WebDriverWait(Driver, TimeSpan.FromSeconds(Timeout)).Until(d =>
        {
            try { return d.SwitchTo().Alert(); }
            catch (NoAlertPresentException) { return null; }
        })!;
        NUnit.Framework.Assert.That(alert.Text, NUnit.Framework.Is.EqualTo(kind == "jsAlert" ? "I am a JS Alert" : kind == "jsConfirm" ? "I am a JS Confirm" : "I am a JS prompt"));
        if (kind == "jsConfirm") alert.Dismiss();
        else { if (kind == "jsPrompt" && input != null) alert.SendKeys(input); alert.Accept(); }
        return Find(By.Id("result")).Text;
    }
    public string EditFrame(string text)
    {
        new WebDriverWait(Driver, TimeSpan.FromSeconds(Timeout)).Until(d => d.FindElement(By.Id("mce_0_ifr")));
        Driver.SwitchTo().Frame("mce_0_ifr");
        try
        {
            var editor = Find(By.Id("tinymce"));
            editor.Click();
            editor.SendKeys(Keys.Control + "a" + Keys.Null);
            editor.SendKeys(Keys.Delete);
            editor.SendKeys(text);
            new WebDriverWait(Driver, TimeSpan.FromSeconds(Timeout)).Until(_ => editor.Text == text);
            return editor.Text;
        }
        finally { Driver.SwitchTo().DefaultContent(); }
    }
}
