

using System.Net;


new Webhost(27001).Run();

class Webhost
{
    int port;
    string pathBase = @"C:/Users/user/Desktop/Step dersler/Nadir html.css/Lesson 7/code/";
    HttpListener listener;

    public Webhost(int port)
    {
        this.port = port;
    }

    public void Run()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($@"http://localhost:{port}/");
        listener.Start();
        Console.WriteLine($"Http server started on {port}");
        while (true)
        {
            var context = listener.GetContext();
            Task.Run(() =>
            {
                HandleRequest(context);
            });
        }


    }


    private void HandleRequest(HttpListenerContext context)
    {
        
        var url = context.Request.RawUrl;
        var path = $@"{pathBase}{url.Split('/').Last()}";

        var responce = context.Response;

        StreamWriter streamWriter = new StreamWriter(responce.OutputStream);

       try
       {

            var src = File.ReadAllText(path);
            streamWriter.Write(src);
       }
       catch(Exception ex)
       {
            //Console.WriteLine(ex.Message);
            var src = $@"{pathBase}404.html";
            streamWriter.Write(src);
       }
       finally
       {
            streamWriter.Close();
       }

    }


}