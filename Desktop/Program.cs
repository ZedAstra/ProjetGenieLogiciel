using System.Drawing;
using System.Text;
using Photino.NET;
using Photino.NET.Server;

namespace Desktop
{
    //NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
    // Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".

    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            PhotinoServer
                .CreateStaticFileServer(args, 10000, 255, "dist", out string baseUrl)
                .RunAsync();

            string windowTitle = "Desktop";

            var window = new PhotinoWindow()
                .SetTitle(windowTitle)
                .SetUseOsDefaultSize(false)
                .SetSize(new Size(1280, 720))
                .Center()
                .SetResizable(true)
                .RegisterWebMessageReceivedHandler((object sender, string message) =>
                {
                    var window = (PhotinoWindow)sender;

                    // The message argument is coming in from sendMessage.
                    // "window.external.sendMessage(message: string)"
                    string response = $"Received message: \"{message}\"";

                    // Send a message back the to JavaScript event handler.
                    // "window.external.receiveMessage(callback: Function)"
                    window.SendWebMessage(response);
                })
#if DEBUG
                .Load("http://localhost:5173");
#else
                .Load(baseUrl);
#endif

            window.WaitForClose();
        }
    }
}
