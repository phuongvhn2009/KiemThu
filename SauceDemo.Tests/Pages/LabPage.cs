using OpenQA.Selenium;

namespace SauceDemo.Tests.Pages;

public class LabPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    private readonly By start = By.CssSelector("#start button");
    private readonly By finish = By.Id("finish");
    private readonly By alertButton = By.CssSelector("button[onclick='jsAlert()']");
    private readonly By confirmButton = By.CssSelector("button[onclick='jsConfirm()']");
    private readonly By promptButton = By.CssSelector("button[onclick='jsPrompt()']");
    private readonly By result = By.Id("result");
    private readonly By frame = By.Id("mce_0_ifr");
    private readonly By editorBody = By.Id("tinymce");
    private readonly By heading = By.TagName("h3");

    public void Open(string path) => Driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/" + path);
    public void StartLoading() => Tim(start).Click();
    // The no-wait branch preserves Lab 3's intentional synchronization failure.
    public string Finish(bool wait) => (wait ? Tim(finish) : Driver.FindElement(finish)).Text;
    public string Heading => Tim(heading).Text;
    public string Result => Tim(result).Text;
    private IAlert CurrentAlert() => Wait.Until(d =>
    {
        try { return d.SwitchTo().Alert(); }
        catch (NoAlertPresentException) { return null; }
    })!;

    public string? OpenAlert(string kind)
    {
        var button = kind switch
        {
            "jsAlert" => alertButton,
            "jsConfirm" => confirmButton,
            "jsPrompt" => promptButton,
            _ => throw new ArgumentException("Unknown alert action", nameof(kind))
        };
        Tim(button).Click();
        return CurrentAlert().Text;
    }
    public void AcceptAlert() => CurrentAlert().Accept();
    public void DismissAlert() => CurrentAlert().Dismiss();
    public void AnswerPrompt(string text)
    {
        var alert = CurrentAlert();
        alert.SendKeys(text);
        alert.Accept();
    }
    public string EditFrame(string text)
    {
        Driver.SwitchTo().Frame(Tim(frame));
        try
        {
            var editor = Tim(editorBody);
            editor.Click();
            editor.SendKeys(Keys.Control + "a" + Keys.Null);
            editor.SendKeys(Keys.Delete);
            editor.SendKeys(text);
            Wait.Until(_ => editor.Text == text);
            return editor.Text;
        }
        finally { Driver.SwitchTo().DefaultContent(); }
    }
}
